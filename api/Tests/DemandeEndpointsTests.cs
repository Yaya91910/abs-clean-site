using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AbsClean.Api.Domain;

namespace AbsClean.Api.Tests;

public class DemandeEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreerClient();

    private static Dictionary<string, object?> Corps(Action<Dictionary<string, object?>>? modifier = null)
    {
        var corps = new Dictionary<string, object?>
        {
            ["profil"] = "particulier",
            ["frequence"] = "ponctuel",
            ["nbVitres"] = 8,
            ["nbBaies"] = 2,
            ["nbVelux"] = 1,
            ["nom"] = "Martin",
            ["prenom"] = "Léa",
            ["telephone"] = "06 12 34 56 78",
            ["email"] = "lea.martin@example.com",
            ["adresse"] = "12 rue des Lilas",
            ["codePostal"] = "91000",
            ["ville"] = "Évry-Courcouronnes",
            ["creneaux"] = new[] { "semaineMatin", "samedi" },
            ["consentement"] = true,
        };
        modifier?.Invoke(corps);
        return corps;
    }

    [Fact]
    public async Task Demande_valide_est_enregistree_avec_un_numero_et_le_total_serveur()
    {
        var reponse = await _client.PostAsJsonAsync("/api/demandes", Corps(c => c["total"] = 1));

        Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
        var json = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        var numero = json.GetProperty("numero").GetString()!;
        Assert.Matches($@"^ABS-{DateTime.UtcNow.Year}-\d{{4}}$", numero);
        Assert.Equal(265m, json.GetProperty("total").GetDecimal());

        var enregistree = factory.AvecBase(db => db.Demandes.Single(d => d.Numero == numero));
        Assert.Equal(StatutDemande.Nouvelle, enregistree.Statut);
        Assert.Equal(265m, enregistree.Total);
        Assert.Equal("0612345678", enregistree.Telephone);
        Assert.Equal([Creneau.SemaineMatin, Creneau.Samedi], enregistree.Creneaux);
        Assert.False(enregistree.SmsEnvoye);
    }

    [Fact]
    public async Task Numeros_de_devis_sont_successifs()
    {
        var premier = await (await _client.PostAsJsonAsync("/api/demandes", Corps())).Content.ReadFromJsonAsync<JsonElement>();
        var second = await (await _client.PostAsJsonAsync("/api/demandes", Corps())).Content.ReadFromJsonAsync<JsonElement>();

        var rang = (JsonElement j) => int.Parse(j.GetProperty("numero").GetString()![^4..]);
        Assert.Equal(rang(premier) + 1, rang(second));
    }

    [Theory]
    [InlineData("nom", "")]
    [InlineData("prenom", null)]
    [InlineData("telephone", "08 12 34 56 78")]
    [InlineData("email", "pas-un-email")]
    [InlineData("adresse", " ")]
    [InlineData("codePostal", "ABCDE")]
    [InlineData("ville", "")]
    public async Task Champ_invalide_renvoie_une_erreur_par_champ(string champ, string? valeur)
    {
        var reponse = await _client.PostAsJsonAsync("/api/demandes", Corps(c => c[champ] = valeur));

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        var json = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("errors").TryGetProperty(champ, out _));
    }

    [Fact]
    public async Task Consentement_manquant_est_refuse()
    {
        var avant = factory.AvecBase(db => db.Demandes.Count());

        var reponse = await _client.PostAsJsonAsync("/api/demandes", Corps(c => c.Remove("consentement")));

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        var json = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("errors").TryGetProperty("consentement", out _));
        Assert.Equal(avant, factory.AvecBase(db => db.Demandes.Count()));
    }

    [Fact]
    public async Task Honeypot_rempli_est_rejete_sans_enregistrement()
    {
        var avant = factory.AvecBase(db => db.Demandes.Count());

        var reponse = await _client.PostAsJsonAsync("/api/demandes", Corps(c => c["siteWeb"] = "http://spam.example"));

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        Assert.Equal("application/problem+json", reponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal(avant, factory.AvecBase(db => db.Demandes.Count()));
    }
}

public class LimiteDebitFactory : ApiFactory
{
    protected override int LimiteDemandes => 2;
}

public class LimiteDebitTests(LimiteDebitFactory factory) : IClassFixture<LimiteDebitFactory>
{
    [Fact]
    public async Task Trop_d_envois_depuis_la_meme_ip_renvoie_429()
    {
        var client = factory.CreerClient();
        var codes = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
            codes.Add((await client.PostAsJsonAsync("/api/demandes", new { nbVitres = 1 })).StatusCode);

        Assert.Equal([HttpStatusCode.BadRequest, HttpStatusCode.BadRequest, HttpStatusCode.TooManyRequests], codes);
    }
}
