using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AbsClean.Api.Tests;

public class DevisEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_repond_ok()
    {
        var reponse = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
    }

    [Fact]
    public async Task Tarifs_exposent_la_grille()
    {
        var json = await _client.GetFromJsonAsync<JsonElement>("/api/tarifs");

        var vitre = json.GetProperty("elements").EnumerateArray().Single(e => e.GetProperty("code").GetString() == "vitre");
        Assert.Equal(20m, vitre.GetProperty("prixUnitaire").GetDecimal());
        Assert.Equal(99, json.GetProperty("quantiteMax").GetInt32());
    }

    [Fact]
    public async Task Estimation_calcule_le_total()
    {
        var reponse = await _client.PostAsJsonAsync("/api/devis/estimation",
            new { profil = "particulier", frequence = "ponctuel", nbVitres = 8, nbBaies = 2, nbVelux = 1 });

        Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
        var json = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(265m, json.GetProperty("total").GetDecimal());
        Assert.Equal(3, json.GetProperty("lignes").GetArrayLength());
    }

    [Fact]
    public async Task Prix_envoye_par_le_client_est_ignore()
    {
        var reponse = await _client.PostAsJsonAsync("/api/devis/estimation",
            new { nbVitres = 2, total = 1, prixUnitaire = 1 });

        var json = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(40m, json.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task Estimation_vide_renvoie_un_problem_details()
    {
        var reponse = await _client.PostAsJsonAsync("/api/devis/estimation", new { nbVitres = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        Assert.Equal("application/problem+json", reponse.Content.Headers.ContentType?.MediaType);
        var json = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("errors").TryGetProperty("quantites", out _));
    }

    [Fact]
    public async Task Quantite_hors_bornes_renvoie_l_erreur_du_champ()
    {
        var reponse = await _client.PostAsJsonAsync("/api/devis/estimation", new { nbVitres = 100 });

        Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        var json = await reponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("errors").TryGetProperty("nbVitres", out _));
    }
}
