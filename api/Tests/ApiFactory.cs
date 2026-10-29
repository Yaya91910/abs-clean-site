using System.Collections.Concurrent;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AbsClean.Api.Tests;

/// <summary>
/// API de test : PostgreSQL remplacé par une base SQLite dans un fichier temporaire (propre à chaque fabrique, créée avant
/// le démarrage, une connexion par opération comme avec PostgreSQL : l'envoi en arrière-plan et le test y accèdent en parallèle),
/// e-mails et SMS remplacés par <see cref="EnvoisFactices"/>. Par défaut, les envois sont faits avant la réponse (tests déterministes).
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _fichier = Path.Combine(Path.GetTempPath(), $"abs-clean-test-{Guid.NewGuid():N}.db");
    private string Connexion => $"Data Source={_fichier};Pooling=False";

    /// <summary>Envois autorisés par fenêtre : élevé par défaut pour ne pas gêner les autres tests.</summary>
    protected virtual int LimiteDemandes => 1000;

    protected virtual bool EnvoisEnArrierePlan => false;

    /// <summary>Remplace l'horloge système (ex. pour tester un changement d'année).</summary>
    protected virtual TimeProvider Horloge => TimeProvider.System;

    /// <summary>E-mails et SMS « envoyés » pendant les tests : rien ne sort, rien n'est écrit sur disque.</summary>
    public EnvoisFactices Envois { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        using (var db = new AbsCleanDb(new DbContextOptionsBuilder<AbsCleanDb>().UseSqlite(Connexion).Options))
            db.Database.EnsureCreated();

        builder.UseSetting("RateLimit:Demandes:Limite", LimiteDemandes.ToString());
        if (builder.GetSetting("RateLimit:Global:LimiteParMinute") is null)
            builder.UseSetting("RateLimit:Global:LimiteParMinute", "100000");
        builder.UseSetting("Envois:EnArrierePlan", EnvoisEnArrierePlan.ToString());
        builder.UseSetting("Email:GerantTo", "gerant@example.com");
        builder.UseSetting("Sms:GerantNumero", "06 11 22 33 44");
        builder.UseSetting("Site:Url", "https://abs-clean.test");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AbsCleanDb>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AbsCleanDb>>();
            services.AddDbContext<AbsCleanDb>(o => o.UseSqlite(Connexion));
            services.RemoveAll<IEnvoiEmail>();
            services.RemoveAll<IEnvoiSms>();
            services.AddSingleton<IEnvoiEmail>(Envois);
            services.AddSingleton<IEnvoiSms>(Envois);
            services.RemoveAll<TimeProvider>();
            services.AddSingleton(Horloge);
        });
    }

    public HttpClient CreerClient() => CreateClient();

    public T AvecBase<T>(Func<AbsCleanDb, T> lecture)
    {
        using var scope = Services.CreateScope();
        return lecture(scope.ServiceProvider.GetRequiredService<AbsCleanDb>());
    }

    /// <summary>Attend qu'une condition sur la base devienne vraie (envois en arrière-plan).</summary>
    public async Task AttendreAsync(Func<AbsCleanDb, bool> condition, TimeSpan? delai = null)
    {
        var limite = DateTime.UtcNow + (delai ?? TimeSpan.FromSeconds(10));
        while (!AvecBase(condition))
        {
            if (DateTime.UtcNow > limite) throw new TimeoutException("Condition non atteinte dans le délai.");
            await Task.Delay(50);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) File.Delete(_fichier);
    }
}

public sealed class EnvoisFactices : IEnvoiEmail, IEnvoiSms
{
    public ConcurrentQueue<EmailSortant> Emails { get; } = new();
    public ConcurrentQueue<(string Numero, string Texte)> Sms { get; } = new();
    public bool EmailEnPanne { get; set; }
    public bool SmsEnPanne { get; set; }

    public Task EnvoyerAsync(EmailSortant email, CancellationToken ct)
    {
        if (EmailEnPanne) throw new HttpRequestException("Service e-mail indisponible (simulé).");
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

/// <summary>Horloge figée, pour les tests dépendant de la date.</summary>
public sealed class HorlogeFixe(DateTimeOffset maintenant) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => maintenant;
}
