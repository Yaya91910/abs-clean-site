using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AbsClean.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace AbsClean.Api.Tests;

/// <summary>Site factice avec un script intégré, comme celui qu'ajoute la compilation Angular.</summary>
public class SiteAvecScriptFactory : ApiFactory
{
    public const string Script = "document.body.dataset.ok='1'";
    private readonly string _racine = Directory.CreateTempSubdirectory("abs-wwwroot-").FullName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        File.WriteAllText(Path.Combine(_racine, "index.html"), $"<html><body><script>{Script}</script></body></html>");
        builder.UseWebRoot(_racine);
        base.ConfigureWebHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(_racine)) Directory.Delete(_racine, recursive: true);
    }
}

public class SecuriteTests(SiteAvecScriptFactory factory) : IClassFixture<SiteAvecScriptFactory>
{
    [Fact]
    public async Task La_page_du_site_n_autorise_que_ses_propres_scripts()
    {
        var reponse = await factory.CreerClient().GetAsync("/");
        var csp = string.Join(";", reponse.Headers.GetValues("Content-Security-Policy"));

        var empreinte = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(SiteAvecScriptFactory.Script)));
        Assert.Contains($"script-src 'self' 'sha256-{empreinte}'", csp);
        Assert.Contains("frame-ancestors 'none'", csp);
        Assert.Contains("object-src 'none'", csp);
        Assert.Contains("frame-src 'none'", csp);
        Assert.Contains("worker-src 'none'", csp);
        Assert.DoesNotContain("unsafe-eval", csp);
    }

    [Fact]
    public async Task La_page_d_accueil_repond_aussi_aux_requetes_head()
    {
        var reponse = await factory.CreerClient().SendAsync(new HttpRequestMessage(HttpMethod.Head, "/"));

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/api/health")]
    [InlineData("/api/tarifs")]
    public async Task Entetes_de_securite_sur_toutes_les_reponses(string adresse)
    {
        var reponse = await factory.CreerClient().GetAsync(adresse);

        Assert.Equal("nosniff", reponse.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", reponse.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", reponse.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("browsing-topics=()", reponse.Headers.GetValues("Permissions-Policy").Single());
        Assert.Equal("none", reponse.Headers.GetValues("X-Permitted-Cross-Domain-Policies").Single());
        Assert.False(reponse.Headers.Contains("Server"));
    }

    [Fact]
    public async Task Requete_trop_volumineuse_refusee()
    {
        var enorme = JsonSerializer.Serialize(new { commentaire = new string('a', 200 * 1024) });
        var reponse = await factory.CreerClient().PostAsync("/api/demandes", new StringContent(enorme, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, reponse.StatusCode);
    }

    [Theory]
    [InlineData("nom", "Martin\r\nBcc: pirate@example.com")]
    [InlineData("ville", "Évry\nNouveau devis")]
    [InlineData("prenom", "Léa\u0000")]
    public async Task Caracteres_de_controle_refuses_dans_les_champs_d_une_ligne(string champ, string valeur)
    {
        var corps = Exemple.Demande();
        corps[champ] = valeur;

        var reponse = await factory.CreerClient().PostAsJsonAsync("/api/demandes", corps);

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        var json = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("errors").TryGetProperty(champ, out _));
    }

    [Fact]
    public async Task Le_commentaire_garde_ses_retours_a_la_ligne()
    {
        var corps = Exemple.Demande();
        corps["commentaire"] = "Vitres hautes.\nChien à la maison.";

        await Exemple.EnvoyerAsync(factory.CreerClient(), corps);
    }

    [Fact]
    public async Task Le_jeton_du_gerant_n_est_pas_envoye_au_serveur_avec_l_adresse()
    {
        var numero = (await Exemple.EnvoyerAsync(factory.CreerClient(), Exemple.Demande())).GetProperty("numero").GetString();

        var email = factory.Envois.Emails.Single(e => e.A == "gerant@example.com" && e.Sujet.Contains(numero!));
        Assert.Contains("/admin/intervention#demande=", email.Html);
        Assert.DoesNotContain("/admin/intervention?", email.Html);
    }
}

public class LimiteGeneraleFactory : ApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("RateLimit:Global:LimiteParMinute", "3");
        base.ConfigureWebHost(builder);
    }
}

public class LimiteGeneraleTests(LimiteGeneraleFactory factory) : IClassFixture<LimiteGeneraleFactory>
{
    [Fact]
    public async Task Trop_de_requetes_depuis_la_meme_ip_renvoie_429()
    {
        var client = factory.CreerClient();
        var codes = new List<HttpStatusCode>();
        for (var i = 0; i < 4; i++) codes.Add((await client.GetAsync("/api/tarifs")).StatusCode);

        Assert.Equal([HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.TooManyRequests], codes);
    }
}
