using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AbsClean.Api.Tests;

/// <summary>Données d'une demande valide, avec un client unique à chaque appel.</summary>
internal static class Exemple
{
    private static int _n;

    public static Dictionary<string, object?> Demande(string prenom = "Léa", string? codeParrainage = null)
    {
        var n = Interlocked.Increment(ref _n);
        return new()
        {
            ["nbVitres"] = 2, ["nom"] = "Martin", ["prenom"] = prenom,
            ["telephone"] = $"06{n:00000000}", ["email"] = $"qualite{n}@example.com",
            ["adresse"] = $"{n} avenue des Tests", ["codePostal"] = "91000", ["ville"] = "Évry",
            ["consentement"] = true, ["codeParrainage"] = codeParrainage,
        };
    }

    public static async Task<JsonElement> EnvoyerAsync(HttpClient client, Dictionary<string, object?> corps)
    {
        var reponse = await client.PostAsJsonAsync("/api/demandes", corps);
        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        return await reponse.Content.ReadFromJsonAsync<JsonElement>();
    }
}

public class ArrierePlanFactory : ApiFactory
{
    protected override bool EnvoisEnArrierePlan => true;
}

public class EnvoisEnArrierePlanTests(ArrierePlanFactory factory) : IClassFixture<ArrierePlanFactory>
{
    [Fact]
    public async Task La_reponse_part_avant_les_envois_qui_suivent_en_arriere_plan()
    {
        var numero = (await Exemple.EnvoyerAsync(factory.CreerClient(), Exemple.Demande())).GetProperty("numero").GetString();

        await factory.AttendreAsync(db => db.Demandes.Any(d => d.Numero == numero && d.EmailClientEnvoye && d.EmailGerantEnvoye && d.SmsEnvoye));
        Assert.Contains(factory.Envois.Emails, e => e.Sujet.Contains(numero!) && e.PieceJointe is not null);
    }
}

public class RepriseFactory : ApiFactory
{
    protected override bool EnvoisEnArrierePlan => false;
}

public class RepriseTests(RepriseFactory factory) : IClassFixture<RepriseFactory>
{
    [Fact]
    public async Task Les_demandes_sans_aucun_envoi_sont_reprises_au_demarrage()
    {
        factory.Envois.EmailEnPanne = true;
        factory.Envois.SmsEnPanne = true;
        var numero = (await Exemple.EnvoyerAsync(factory.CreerClient(), Exemple.Demande())).GetProperty("numero").GetString();
        var avant = factory.AvecBase(db => db.Demandes.Single(d => d.Numero == numero));
        Assert.False(avant.EmailClientEnvoye || avant.EmailGerantEnvoye || avant.SmsEnvoye);

        // Le service d'envoi revient ; au démarrage suivant, la reprise relance la demande.
        factory.Envois.EmailEnPanne = false;
        factory.Envois.SmsEnPanne = false;
        var service = factory.Services.GetServices<IHostedService>().OfType<EnvoisEnArrierePlan>().Single();
        Assert.True(await service.ReprendreAsync(CancellationToken.None) >= 1);

        await factory.AttendreAsync(db => db.Demandes.Any(d => d.Numero == numero && d.EmailClientEnvoye && d.EmailGerantEnvoye && d.SmsEnvoye));
        var apres = factory.AvecBase(db => db.Demandes.Single(d => d.Numero == numero));
        Assert.NotEqual(avant.JetonInterventionHash, apres.JetonInterventionHash);

        // Le nouveau lien envoyé au gérant fonctionne.
        var emailGerant = factory.Envois.Emails.Single(e => e.A == "gerant@example.com" && e.Sujet.Contains(numero!));
        var lien = Regex.Match(emailGerant.Html, @"demande=(\d+)&amp;jeton=([A-Za-z0-9_-]+)");
        var validation = await factory.CreerClient().PostAsJsonAsync($"/api/admin/demandes/{lien.Groups[1].Value}/intervention-realisee", new { jeton = lien.Groups[2].Value });
        Assert.Equal(HttpStatusCode.OK, validation.StatusCode);
    }
}

public class ValidationConcurrenteTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Un_double_clic_ne_valide_et_ne_previent_le_parrain_qu_une_fois()
    {
        var client = factory.CreerClient();
        var parrain = Exemple.Demande("Sophie");
        var code = (await Exemple.EnvoyerAsync(client, parrain)).GetProperty("codeParrain").GetString();
        var numero = (await Exemple.EnvoyerAsync(client, Exemple.Demande("Tom", code))).GetProperty("numero").GetString();

        var emailGerant = factory.Envois.Emails.Single(e => e.A == "gerant@example.com" && e.Sujet.Contains(numero!));
        var lien = Regex.Match(emailGerant.Html, @"demande=(\d+)&amp;jeton=([A-Za-z0-9_-]+)");
        var url = $"/api/admin/demandes/{lien.Groups[1].Value}/intervention-realisee";
        var corps = new { jeton = lien.Groups[2].Value };

        var reponses = await Task.WhenAll(client.PostAsJsonAsync(url, corps), client.PostAsJsonAsync(url, corps));

        Assert.Equal([HttpStatusCode.OK, HttpStatusCode.Conflict], reponses.Select(r => r.StatusCode).OrderBy(s => s));
        Assert.Single(factory.Envois.Emails, e => e.A == (string)parrain["email"]! && e.Sujet.StartsWith("Votre proche Tom"));
    }
}

public class NouvelAnFactory : ApiFactory
{
    // 31 décembre 2026, 23 h 30 UTC = 1er janvier 2027, 0 h 30 à Paris.
    protected override TimeProvider Horloge => new HorlogeFixe(new DateTimeOffset(2026, 12, 31, 23, 30, 0, TimeSpan.Zero));
}

public class NouvelAnTests(NouvelAnFactory factory) : IClassFixture<NouvelAnFactory>
{
    [Fact]
    public async Task Le_numero_de_devis_suit_l_annee_francaise()
    {
        var reponse = await Exemple.EnvoyerAsync(factory.CreerClient(), Exemple.Demande());

        Assert.Equal("ABS-2027-0001", reponse.GetProperty("numero").GetString());
    }

    [Fact]
    public void Heure_de_paris_a_partir_de_l_utc()
    {
        Assert.Equal(new DateTime(2027, 1, 1, 0, 30, 0), HeureParis.Depuis(new DateTime(2026, 12, 31, 23, 30, 0, DateTimeKind.Utc)));
        Assert.Equal(new DateTime(2026, 7, 14, 14, 0, 0), HeureParis.Depuis(new DateTime(2026, 7, 14, 12, 0, 0, DateTimeKind.Utc)));
    }
}

/// <summary>Site compilé factice (wwwroot temporaire) pour tester la page servie par l'API.</summary>
public class SiteFactory : ApiFactory
{
    public string Racine { get; } = Directory.CreateTempSubdirectory("abs-wwwroot-").FullName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        File.WriteAllText(Path.Combine(Racine, "index.html"),
            """<html><head><meta property="og:image" content="__SITE_URL__/og-image.png"></head><body>site</body></html>""");
        File.WriteAllText(Path.Combine(Racine, "logo.svg"), "<svg/>");
        File.WriteAllText(Path.Combine(Racine, "main-YZCV272D.js"), string.Concat(Enumerable.Repeat("console.log('ABS clean');\n", 200)));
        // Version Brotli « précompressée » (contenu factice reconnaissable : le serveur doit l'envoyer telle quelle).
        File.WriteAllBytes(Path.Combine(Racine, "main-YZCV272D.js.br"), [0x0B, 0x42, 0x52]);
        builder.UseWebRoot(Racine);
        base.ConfigureWebHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // La fabrique peut être libérée deux fois (Dispose puis DisposeAsync) : on ne supprime qu'une fois.
        if (disposing && Directory.Exists(Racine)) Directory.Delete(Racine, recursive: true);
    }
}

public class SiteTests(SiteFactory factory) : IClassFixture<SiteFactory>
{
    [Theory]
    [InlineData("/")]
    [InlineData("/devis")]
    [InlineData("/admin/intervention")]
    [InlineData("/mentions-legales")]
    public async Task Les_pages_du_site_ont_des_adresses_absolues_pour_le_partage(string adresse)
    {
        var reponse = await factory.CreerClient().GetAsync(adresse);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Equal("no-cache", reponse.Headers.CacheControl?.ToString());
        Assert.Contains("""content="https://abs-clean.test/og-image.png""", await reponse.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Les_fichiers_sont_servis_et_mis_en_cache_les_absents_et_l_api_inconnue_en_404()
    {
        var client = factory.CreerClient();

        // Nom avec empreinte : en cache un an ; nom fixe (remplaçable, ex. photo) : un jour.
        var script = await client.GetAsync("/main-YZCV272D.js");
        Assert.Equal(HttpStatusCode.OK, script.StatusCode);
        Assert.Equal("public, max-age=31536000, immutable", script.Headers.CacheControl?.ToString());
        var logo = await client.GetAsync("/logo.svg");
        Assert.Equal(HttpStatusCode.OK, logo.StatusCode);
        Assert.Equal("public, max-age=86400", logo.Headers.CacheControl?.ToString());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/absent.png")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/inexistant")).StatusCode);
    }

    [Theory]
    [InlineData("br")]
    [InlineData("gzip")]
    public async Task Le_site_est_compresse_mais_pas_les_reponses_json(string encodage)
    {
        var client = factory.CreerClient();

        foreach (var adresse in new[] { "/", "/main-YZCV272D.js" })
        {
            var requete = new HttpRequestMessage(HttpMethod.Get, adresse);
            requete.Headers.AcceptEncoding.ParseAdd(encodage);
            var reponse = await client.SendAsync(requete);
            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            Assert.Equal([encodage], reponse.Content.Headers.ContentEncoding);
        }

        var api = new HttpRequestMessage(HttpMethod.Get, "/api/health");
        api.Headers.AcceptEncoding.ParseAdd(encodage);
        Assert.Empty((await client.SendAsync(api)).Content.Headers.ContentEncoding);
    }

    [Fact]
    public async Task La_version_precompressee_est_servie_telle_quelle_avec_le_bon_type()
    {
        var requete = new HttpRequestMessage(HttpMethod.Get, "/main-YZCV272D.js");
        requete.Headers.AcceptEncoding.ParseAdd("gzip, deflate, br");
        var reponse = await factory.CreerClient().SendAsync(requete);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Equal(["br"], reponse.Content.Headers.ContentEncoding);
        Assert.Equal("text/javascript", reponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal("public, max-age=31536000, immutable", reponse.Headers.CacheControl?.ToString());
        Assert.Contains("Accept-Encoding", reponse.Headers.Vary);
        Assert.Equal([0x0B, 0x42, 0x52], await reponse.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Brotli_refuse_par_le_navigateur_n_est_pas_servi()
    {
        var requete = new HttpRequestMessage(HttpMethod.Get, "/main-YZCV272D.js");
        requete.Headers.AcceptEncoding.ParseAdd("br;q=0, gzip");
        var reponse = await factory.CreerClient().SendAsync(requete);

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Equal(["gzip"], reponse.Content.Headers.ContentEncoding);
    }

    [Fact]
    public async Task La_grille_des_tarifs_est_gardee_cinq_minutes()
    {
        var reponse = await factory.CreerClient().GetAsync("/api/tarifs");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        Assert.Equal("public, max-age=300", reponse.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Robots_et_plan_du_site_utilisent_l_adresse_publique()
    {
        var client = factory.CreerClient();

        var robots = await client.GetStringAsync("/robots.txt");
        Assert.Contains("Disallow: /admin/", robots);
        Assert.Contains("Disallow: /api/", robots);
        Assert.Contains("Sitemap: https://abs-clean.test/sitemap.xml", robots);

        var plan = await client.GetAsync("/sitemap.xml");
        Assert.Equal("application/xml", plan.Content.Headers.ContentType?.MediaType);
        var xml = System.Xml.Linq.XDocument.Parse(await plan.Content.ReadAsStringAsync());
        var adresses = xml.Descendants().Where(e => e.Name.LocalName == "loc").Select(e => e.Value).ToList();
        Assert.Equal(["https://abs-clean.test/", "https://abs-clean.test/mentions-legales", "https://abs-clean.test/confidentialite", "https://abs-clean.test/parrainage/conditions"], adresses);
    }

    [Fact]
    public async Task La_page_du_gerant_n_est_pas_indexee()
    {
        var client = factory.CreerClient();

        var admin = await client.GetAsync("/admin/intervention");
        Assert.Equal("noindex, nofollow", string.Join(", ", admin.Headers.GetValues("X-Robots-Tag")));
        Assert.False((await client.GetAsync("/")).Headers.Contains("X-Robots-Tag"));
    }

    [Fact]
    public async Task Health_indique_le_mode_des_envois()
    {
        var json = await factory.CreerClient().GetFromJsonAsync<JsonElement>("/api/health");

        Assert.Equal("ok", json.GetProperty("status").GetString());
        Assert.Equal("test", json.GetProperty("envois").GetProperty("email").GetString());
        Assert.Equal("test", json.GetProperty("envois").GetProperty("sms").GetString());
    }
}
