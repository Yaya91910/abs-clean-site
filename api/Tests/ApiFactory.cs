using AbsClean.Api.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AbsClean.Api.Tests;

/// <summary>API de test : PostgreSQL remplacé par une base SQLite en mémoire, propre à chaque fabrique.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connexion = new("DataSource=:memory:");

    /// <summary>Envois autorisés par fenêtre : élevé par défaut pour ne pas gêner les autres tests.</summary>
    protected virtual int LimiteDemandes => 1000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connexion.Open();
        builder.UseSetting("RateLimit:Demandes:Limite", LimiteDemandes.ToString());
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AbsCleanDb>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AbsCleanDb>>();
            services.AddDbContext<AbsCleanDb>(o => o.UseSqlite(_connexion));
        });
    }

    public HttpClient CreerClient()
    {
        var client = CreateClient();
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AbsCleanDb>().Database.EnsureCreated();
        return client;
    }

    public T AvecBase<T>(Func<AbsCleanDb, T> lecture)
    {
        using var scope = Services.CreateScope();
        return lecture(scope.ServiceProvider.GetRequiredService<AbsCleanDb>());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connexion.Dispose();
    }
}
