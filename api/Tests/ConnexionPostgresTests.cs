using System.Net;
using AbsClean.Api.Infrastructure;
using Npgsql;

namespace AbsClean.Api.Tests;

public class ConnexionPostgresTests
{
    [Fact]
    public void Url_postgresql_convertie_pour_npgsql()
    {
        var chaine = ConnexionPostgres.Normaliser("postgresql://abs_user:p%40ss%3Aword@dpg-abc123-a.frankfurt-postgres.render.com:5433/abs_clean");

        var b = new NpgsqlConnectionStringBuilder(chaine);
        Assert.Equal("dpg-abc123-a.frankfurt-postgres.render.com", b.Host);
        Assert.Equal(5433, b.Port);
        Assert.Equal("abs_clean", b.Database);
        Assert.Equal("abs_user", b.Username);
        Assert.Equal("p@ss:word", b.Password);
    }

    [Fact]
    public void Url_sans_port_utilise_5432()
    {
        var b = new NpgsqlConnectionStringBuilder(ConnexionPostgres.Normaliser("postgres://u:p@dpg-interne/base"));

        Assert.Equal(5432, b.Port);
        Assert.Equal("dpg-interne", b.Host);
    }

    [Theory]
    [InlineData("Host=localhost;Database=abs_clean_dev")]
    [InlineData("")]
    [InlineData(null)]
    public void Chaine_classique_inchangee(string? chaine)
    {
        Assert.Equal(chaine, ConnexionPostgres.Normaliser(chaine));
    }
}

public class RoutageTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Adresse_api_inconnue_renvoie_404_et_non_le_site()
    {
        var reponse = await factory.CreerClient().GetAsync("/api/inexistant");

        Assert.Equal(HttpStatusCode.NotFound, reponse.StatusCode);
    }
}
