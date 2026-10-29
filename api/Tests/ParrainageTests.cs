using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace AbsClean.Api.Tests;

public class ParrainageTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreerClient();
    private static int _compteur;

    /// <summary>Chaque appel crée un client distinct (téléphone, e-mail et adresse uniques).</summary>
    private static Dictionary<string, object?> Client(string prenom = "Léa", string? codeParrainage = null)
    {
        var n = Interlocked.Increment(ref _compteur);
        return new()
        {
            ["nbVitres"] = 2, ["nom"] = "Martin", ["prenom"] = prenom,
            ["telephone"] = $"07{n:00000000}", ["email"] = $"client{n}@example.com",
            ["adresse"] = $"{n} rue des Lilas", ["codePostal"] = "91000", ["ville"] = "Évry",
            ["consentement"] = true, ["codeParrainage"] = codeParrainage,
        };
    }

    private async Task<JsonElement> EnvoyerAsync(Dictionary<string, object?> corps)
    {
        var reponse = await _client.PostAsJsonAsync("/api/demandes", corps);
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        return await reponse.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static string Code(JsonElement reponse) => reponse.GetProperty("codeParrain").GetString()!;

    private static string? Saisi(JsonElement reponse) =>
        reponse.GetProperty("codeParrainageSaisi") is { ValueKind: JsonValueKind.String } v ? v.GetString() : null;

    [Fact]
    public async Task Chaque_demande_recoit_un_code_personnel_et_son_lien()
    {
        var reponse = await EnvoyerAsync(Client("Léa"));

        Assert.Matches("^ABS-LEA[A-Z2-9]{1,2}$", Code(reponse));
        Assert.Equal($"https://abs-clean.test/devis?parrain={Code(reponse)}", reponse.GetProperty("lienParrainage").GetString());
        Assert.Null(Saisi(reponse));
    }

    [Fact]
    public async Task Codes_uniques_meme_avec_beaucoup_de_prenoms_identiques()
    {
        var codes = new List<string>();
        for (var i = 0; i < 45; i++) codes.Add(Code(await EnvoyerAsync(Client("Marie"))));

        Assert.Equal(codes.Count, codes.Distinct().Count());
        Assert.All(codes, c => Assert.Matches("^ABS-MARIE[A-Z2-9]{1,3}$", c));
    }

    [Fact]
    public async Task Verification_d_un_code_sans_donnees_personnelles()
    {
        var code = Code(await EnvoyerAsync(Client("Paul")));

        var connu = await _client.GetAsync($"/api/parrainage/{code.ToLowerInvariant()}");
        Assert.Equal(HttpStatusCode.OK, connu.StatusCode);
        var corps = await connu.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Paul", corps);
        Assert.DoesNotContain("Martin", corps);
        Assert.DoesNotContain("@", corps);

        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/parrainage/ABS-INCONNUZZ")).StatusCode);
    }

    [Fact]
    public async Task Qr_code_decodable_renvoie_le_lien_de_parrainage()
    {
        var code = Code(await EnvoyerAsync(Client("Nina")));

        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ServiceParrainage>();
        Assert.Equal($"https://abs-clean.test/devis?parrain={code}", Decoder(service, code));

        var svg = await _client.GetAsync($"/api/parrainage/{code}/qr.svg");
        Assert.Equal("image/svg+xml", svg.Content.Headers.ContentType?.MediaType);
        Assert.Contains("<svg", await svg.Content.ReadAsStringAsync());
        var png = await _client.GetAsync($"/api/parrainage/{code}/qr.png");
        Assert.Equal("image/png", png.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public void Qr_code_decodable_pour_de_nombreux_codes()
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ServiceParrainage>();
        foreach (var prenom in new[] { "Léa", "Marie", "Jean-François", "Zoé", "Maximilien" })
            for (var i = 0; i < 60; i++)
            {
                var code = CodeParrain.Generer(prenom, 1 + i % 2);
                Assert.Equal($"https://abs-clean.test/devis?parrain={code}", Decoder(service, code));
            }
    }

    [Fact]
    public async Task Code_inconnu_ignore_sans_bloquer_la_demande()
    {
        var reponse = await EnvoyerAsync(Client("Hugo", codeParrainage: "ABS-INCONNUZZ"));

        Assert.Equal("ignore", Saisi(reponse));
        var numero = reponse.GetProperty("numero").GetString();
        Assert.Null(factory.AvecBase(db => db.Demandes.Single(d => d.Numero == numero).ParrainId));
    }

    [Fact]
    public async Task Auto_parrainage_refuse()
    {
        var parrain = Client("Ines");
        var code = Code(await EnvoyerAsync(parrain));

        // Même téléphone que le parrain.
        var memeTelephone = Client("Ines", codeParrainage: code);
        memeTelephone["telephone"] = parrain["telephone"];
        Assert.Equal("ignore", Saisi(await EnvoyerAsync(memeTelephone)));

        // Même e-mail, casse différente.
        var memeEmail = Client("Ines", codeParrainage: code);
        memeEmail["email"] = ((string)parrain["email"]!).ToUpperInvariant();
        Assert.Equal("ignore", Saisi(await EnvoyerAsync(memeEmail)));

        // Même adresse, écrite autrement.
        var memeAdresse = Client("Ines", codeParrainage: code);
        memeAdresse["adresse"] = ((string)parrain["adresse"]!).ToUpperInvariant().Replace(" ", "  ");
        Assert.Equal("ignore", Saisi(await EnvoyerAsync(memeAdresse)));

        Assert.False(factory.AvecBase(db => db.Parrainages.Any(p => p.Parrain.CodeParrainPersonnel == code)));
    }

    [Fact]
    public async Task Un_client_ne_peut_avoir_qu_un_seul_parrain()
    {
        var codeA = Code(await EnvoyerAsync(Client("Alice")));
        var codeB = Code(await EnvoyerAsync(Client("Bruno")));

        var filleul = Client("Chloé", codeParrainage: codeA);
        Assert.Equal("applique", Saisi(await EnvoyerAsync(filleul)));

        var nouvelleDemande = Client("Chloé", codeParrainage: codeB);
        nouvelleDemande["telephone"] = filleul["telephone"];
        Assert.Equal("ignore", Saisi(await EnvoyerAsync(nouvelleDemande)));
    }

    [Fact]
    public async Task Remise_activee_uniquement_apres_validation_du_gerant()
    {
        var parrain = Client("Sophie");
        var code = Code(await EnvoyerAsync(parrain));
        var filleul = await EnvoyerAsync(Client("Tom", codeParrainage: code));
        Assert.Equal("applique", Saisi(filleul));
        var numero = filleul.GetProperty("numero").GetString()!;

        var parrainage = factory.AvecBase(db => db.Parrainages.Single(p => p.Filleul.Numero == numero));
        Assert.Equal(StatutParrainage.EnAttente, parrainage.Statut);

        // Lien à usage unique reçu par le gérant.
        var emailGerant = factory.Envois.Emails.Single(e => e.A == "gerant@example.com" && e.Sujet.Contains(numero));
        var lien = Regex.Match(emailGerant.Html, @"demande=(\d+)&amp;jeton=([A-Za-z0-9_-]+)");
        Assert.True(lien.Success);
        var url = $"/api/admin/demandes/{lien.Groups[1].Value}/intervention-realisee";

        Assert.Equal(HttpStatusCode.Forbidden, (await _client.PostAsJsonAsync(url, new { jeton = "faux" })).StatusCode);
        Assert.Equal(StatutParrainage.EnAttente, factory.AvecBase(db => db.Parrainages.Single(p => p.Id == parrainage.Id).Statut));

        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsJsonAsync(url, new { jeton = lien.Groups[2].Value })).StatusCode);
        var valide = factory.AvecBase(db => db.Parrainages.Single(p => p.Id == parrainage.Id));
        Assert.Equal(StatutParrainage.Valide, valide.Statut);
        Assert.NotNull(valide.ValideLe);
        Assert.Contains(factory.Envois.Emails, e => e.A == (string)parrain["email"]! && e.Sujet == "Votre proche Tom est devenu client : −10 % sur votre prochaine intervention");

        // Usage unique.
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync(url, new { jeton = lien.Groups[2].Value })).StatusCode);
    }

    [Fact]
    public async Task Le_pdf_et_l_email_du_client_contiennent_son_code()
    {
        var corps = Client("Emma");
        var code = Code(await EnvoyerAsync(corps));

        var email = factory.Envois.Emails.Single(e => e.A == (string)corps["email"]!);
        Assert.Contains(code, email.Html);
        Assert.Contains($"https://abs-clean.test/api/parrainage/{code}/qr.png", email.Html);
        Assert.NotNull(email.PieceJointe);
    }

    private static string Decoder(ServiceParrainage service, string code)
    {
        using var donnees = service.Generer(code);
        const int echelle = 4;
        var n = donnees.ModuleMatrix.Count;
        var pixels = new byte[n * echelle * n * echelle];
        for (var y = 0; y < n * echelle; y++)
            for (var x = 0; x < n * echelle; x++)
                pixels[y * n * echelle + x] = donnees.ModuleMatrix[y / echelle][x / echelle] ? (byte)0 : (byte)255;

        var source = new RGBLuminanceSource(pixels, n * echelle, n * echelle, RGBLuminanceSource.BitmapFormat.Gray8);
        // Image synthétique parfaite : lecture en « code-barres pur », la détection générique échoue sur certains motifs.
        var indices = new Dictionary<DecodeHintType, object> { [DecodeHintType.PURE_BARCODE] = true };
        return new QRCodeReader().decode(new BinaryBitmap(new HybridBinarizer(source)), indices).Text;
    }
}
