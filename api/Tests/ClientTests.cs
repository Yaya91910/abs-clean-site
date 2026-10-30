using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AbsClean.Api.Tests;

public class ClientTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreerClient();

    private static string Txt(JsonElement j, string p) => j.GetProperty(p).GetString()!;

    [Fact]
    public async Task Un_nouveau_client_recoit_un_numero_CLI()
    {
        var reponse = await Exemple.EnvoyerAsync(_client, Exemple.Demande());

        Assert.Matches(@"^CLI-\d{5}$", Txt(reponse, "numeroClient"));
        Assert.False(reponse.GetProperty("clientReconnu").GetBoolean());
        Assert.Equal(JsonValueKind.Null, reponse.GetProperty("contact").ValueKind);
    }

    [Fact]
    public async Task Meme_email_meme_client_sans_doublon_casse_ignoree()
    {
        var premiere = Exemple.Demande();
        var r1 = await Exemple.EnvoyerAsync(_client, premiere);

        var seconde = Exemple.Demande();
        seconde["email"] = ((string)premiere["email"]!).ToUpperInvariant();
        var r2 = await Exemple.EnvoyerAsync(_client, seconde);

        Assert.Equal(Txt(r1, "numeroClient"), Txt(r2, "numeroClient"));
        Assert.Equal(1, factory.AvecBase(db => db.Clients.Count(c => c.Numero == Txt(r1, "numeroClient"))));
        Assert.Equal(2, factory.AvecBase(db => db.Demandes.Count(d => d.Client.Numero == Txt(r1, "numeroClient"))));
    }

    [Fact]
    public async Task Meme_telephone_meme_client_meme_avec_un_autre_email()
    {
        var premiere = Exemple.Demande();
        var r1 = await Exemple.EnvoyerAsync(_client, premiere);

        var seconde = Exemple.Demande();
        seconde["telephone"] = premiere["telephone"];
        var r2 = await Exemple.EnvoyerAsync(_client, seconde);

        Assert.Equal(Txt(r1, "numeroClient"), Txt(r2, "numeroClient"));
        // Un seul des deux identifiants : rattaché au même client, mais pas d'accueil personnalisé (rien n'est révélé).
        Assert.False(r2.GetProperty("clientReconnu").GetBoolean());
    }

    [Fact]
    public async Task Client_reconnu_avec_email_et_telephone_identiques()
    {
        var corps = Exemple.Demande("Nina");
        await Exemple.EnvoyerAsync(_client, corps);

        var r2 = await Exemple.EnvoyerAsync(_client, new Dictionary<string, object?>(corps));

        Assert.True(r2.GetProperty("clientReconnu").GetBoolean());
    }

    [Fact]
    public async Task Clients_differents_numeros_differents_et_successifs()
    {
        var a = Txt(await Exemple.EnvoyerAsync(_client, Exemple.Demande()), "numeroClient");
        var b = Txt(await Exemple.EnvoyerAsync(_client, Exemple.Demande()), "numeroClient");

        Assert.Equal(int.Parse(a[4..]) + 1, int.Parse(b[4..]));
    }

    [Fact]
    public async Task Le_gerant_recoit_la_fiche_le_client_son_devis_avec_le_client_existant_mis_en_avant()
    {
        var corps = Exemple.Demande("Paul");
        await Exemple.EnvoyerAsync(_client, corps);
        var r2 = await Exemple.EnvoyerAsync(_client, new Dictionary<string, object?>(corps));
        var numero = Txt(r2, "numero");

        var gerant = factory.Envois.Emails.Single(e => e.A == "gerant@example.com" && e.Sujet.Contains(numero));
        Assert.StartsWith("Client existant · ", gerant.Sujet);
        Assert.Contains("CLIENT EXISTANT · 2e DEMANDE", gerant.Html);
        Assert.Contains($"tel:{((string)corps["telephone"]!).Replace(" ", "")}", gerant.Html);
        Assert.Equal($"Fiche-{numero}-{Txt(r2, "numeroClient")}.pdf", gerant.PieceJointe?.Nom);

        var client = factory.Envois.Emails.Single(e => e.A == (string)corps["email"]! && e.Sujet.Contains(numero));
        Assert.Equal($"Devis-{numero}.pdf", client.PieceJointe?.Nom);
        Assert.Contains(Txt(r2, "numeroClient"), client.Html);
        Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(gerant.PieceJointe!.Contenu, 0, 5));
    }
}

public class ContactEntrepriseFactory : ApiFactory
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseSetting("Entreprise:Telephone", "01 00 00 00 00");
        builder.UseSetting("Entreprise:Email", "contact@example.com");
        base.ConfigureWebHost(builder);
    }
}

public class ContactDirectTests(ContactEntrepriseFactory factory) : IClassFixture<ContactEntrepriseFactory>
{
    [Fact]
    public async Task Un_client_reconnu_recoit_les_coordonnees_directes_d_abs_clean()
    {
        var client = factory.CreerClient();
        var corps = Exemple.Demande();
        var r1 = await Exemple.EnvoyerAsync(client, corps);
        Assert.Equal(JsonValueKind.Null, r1.GetProperty("contact").ValueKind);

        var r2 = await Exemple.EnvoyerAsync(client, new Dictionary<string, object?>(corps));

        Assert.Equal("01 00 00 00 00", r2.GetProperty("contact").GetProperty("telephone").GetString());
        Assert.Equal("contact@example.com", r2.GetProperty("contact").GetProperty("email").GetString());
    }
}
