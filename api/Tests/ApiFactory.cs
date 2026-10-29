using System.Collections.Concurrent;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;
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

    /// <summary>E-mails et SMS « envoyés » pendant les tests : rien ne sort, rien n'est écrit sur disque.</summary>
    public EnvoisFactices Envois { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connexion.Open();
        builder.UseSetting("RateLimit:Demandes:Limite", LimiteDemandes.ToString());
        builder.UseSetting("Email:GerantTo", "gerant@example.com");
        builder.UseSetting("Sms:GerantNumero", "06 11 22 33 44");
        builder.UseSetting("Site:Url", "https://abs-clean.test");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AbsCleanDb>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AbsCleanDb>>();
            services.AddDbContext<AbsCleanDb>(o => o.UseSqlite(_connexion));
            services.RemoveAll<IEnvoiEmail>();
            services.RemoveAll<IEnvoiSms>();
            services.AddSingleton<IEnvoiEmail>(Envois);
            services.AddSingleton<IEnvoiSms>(Envois);
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

public sealed class EnvoisFactices : IEnvoiEmail, IEnvoiSms
{
    public ConcurrentQueue<EmailSortant> Emails { get; } = new();
    public ConcurrentQueue<(string Numero, string Texte)> Sms { get; } = new();
    public bool SmsEnPanne { get; set; }

    public Task EnvoyerAsync(EmailSortant email, CancellationToken ct)
    {
        Emails.Enqueue(email);
        return Task.CompletedTask;
    }

    public Task EnvoyerAsync(string numero, string texte, CancellationToken ct)
    {
        if (SmsEnPanne) throw new HttpRequestException("Service SMS indisponible (simulé).");
        Sms.Enqueue((numero, texte));
        return Task.CompletedTask;
    }
}
