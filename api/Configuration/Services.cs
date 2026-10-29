using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AbsClean.Api.Endpoints;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AbsClean.Api.Configuration;

/// <summary>Enregistrement des services de l'API, regroupés par thème (appelés depuis Program.cs).</summary>
public static class Services
{
    /// <summary>JSON (énumérations en camelCase), erreurs au format ProblemDetails, horloge.</summary>
    public static WebApplicationBuilder AjouterApi(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();
        builder.Services.ConfigureHttpJsonOptions(o =>
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
        builder.Services.AddSingleton<PageIndex>();
        return builder;
    }

    /// <summary>PostgreSQL (EF Core) et enregistrement des demandes.</summary>
    public static WebApplicationBuilder AjouterDonnees(this WebApplicationBuilder builder)
    {
        var connexion = ConnexionPostgres.Normaliser(builder.Configuration.GetConnectionString("Default"));
        builder.Services.AddDbContext<AbsCleanDb>(o => o.UseNpgsql(connexion));
        builder.Services.AddScoped<DemandeStore>();
        builder.Services.AddScoped<ServiceParrainage>();
        return builder;
    }

    /// <summary>
    /// PDF, e-mails et SMS (§9). Sans clé d'API Brevo : mode test, tout est écrit dans le dossier Envoi:Test:Dossier.
    /// Les envois partent en arrière-plan, après la réponse au client.
    /// </summary>
    public static WebApplicationBuilder AjouterEnvois(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var services = builder.Services;

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        services.Configure<EntrepriseOptions>(config.GetSection("Entreprise"));
        services.Configure<EmailOptions>(config.GetSection("Email"));
        services.Configure<SmsOptions>(config.GetSection("Sms"));
        services.Configure<EnvoiTestOptions>(config.GetSection("Envoi:Test"));
        services.Configure<EnvoisOptions>(config.GetSection("Envois"));
        services.AddSingleton(sp => new DevisPdf(sp.GetRequiredService<IOptions<EntrepriseOptions>>().Value));
        services.AddScoped<NotificationsDemande>();
        services.AddSingleton<FileEnvois>();
        services.AddHostedService<EnvoisEnArrierePlan>();

        var mode = new ModeEnvois(
            EmailReel: !string.IsNullOrWhiteSpace(config["Email:ApiKey"]),
            SmsReel: !string.IsNullOrWhiteSpace(config["Sms:ApiKey"]));
        services.AddSingleton(mode);

        if (mode.EmailReel) services.AddHttpClient<IEnvoiEmail, BrevoEmail>(c => c.Timeout = TimeSpan.FromSeconds(15));
        else services.AddSingleton<IEnvoiEmail, DossierEmail>();
        if (mode.SmsReel) services.AddHttpClient<IEnvoiSms, BrevoSms>(c => c.Timeout = TimeSpan.FromSeconds(15));
        else services.AddSingleton<IEnvoiSms, DossierSms>();

        return builder;
    }

    /// <summary>Limitation de débit par IP (§8) : envoi des demandes, et consultations du parrainage.</summary>
    public static WebApplicationBuilder AjouterAntiAbus(this WebApplicationBuilder builder)
    {
        var limiteDemandes = builder.Configuration.GetValue("RateLimit:Demandes:Limite", 5);
        var fenetreDemandes = TimeSpan.FromMinutes(builder.Configuration.GetValue("RateLimit:Demandes:FenetreMinutes", 10));

        builder.Services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.AddPolicy(DemandeEndpoints.PolitiqueDebit, ParIp(limiteDemandes, fenetreDemandes));
            o.AddPolicy(ParrainageEndpoints.PolitiqueConsultation, ParIp(60, TimeSpan.FromMinutes(1)));
        });
        return builder;
    }

    /// <summary>
    /// Derrière le proxy de l'hébergeur (Render) : IP réelle du client et schéma HTTPS d'origine.
    /// ForwardLimit = 1 : seule l'entrée ajoutée par le proxy compte, pas celles envoyées par le client.
    /// CORS : inutile en production (site et API sur le même domaine), utile en développement.
    /// </summary>
    public static WebApplicationBuilder AjouterHebergement(this WebApplicationBuilder builder)
    {
        if (builder.Configuration.GetValue<bool>("Proxy:EntetesTransferees"))
        {
            builder.Services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                o.ForwardLimit = 1;
                o.KnownIPNetworks.Clear();
                o.KnownProxies.Clear();
            });
        }

        var origineAutorisee = builder.Configuration["Cors:AllowedOrigin"];
        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            if (!string.IsNullOrWhiteSpace(origineAutorisee))
                p.WithOrigins(origineAutorisee).AllowAnyHeader().WithMethods("GET", "POST");
        }));
        return builder;
    }

    private static Func<HttpContext, RateLimitPartition<string>> ParIp(int limite, TimeSpan fenetre) =>
        http => RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "inconnue",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = limite, Window = fenetre, QueueLimit = 0 });
}
