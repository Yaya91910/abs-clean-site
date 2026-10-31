using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AbsClean.Api.Endpoints;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
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

    /// <summary>
    /// Limitation de débit par IP (§8) : envoi des demandes, consultations du parrainage,
    /// et une limite générale large sur tout le reste contre les inondations de requêtes.
    /// </summary>
    public static WebApplicationBuilder AjouterAntiAbus(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var limiteDemandes = config.GetValue("RateLimit:Demandes:Limite", 5);
        var fenetreDemandes = TimeSpan.FromMinutes(config.GetValue("RateLimit:Demandes:FenetreMinutes", 10));
        var limiteGenerale = config.GetValue("RateLimit:Global:LimiteParMinute", 300);

        builder.Services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.GlobalLimiter = PartitionedRateLimiter.Create(ParIp(limiteGenerale, TimeSpan.FromMinutes(1)));
            o.AddPolicy(DemandeEndpoints.PolitiqueDebit, ParIp(limiteDemandes, fenetreDemandes));
            o.AddPolicy(ParrainageEndpoints.PolitiqueConsultation, ParIp(60, TimeSpan.FromMinutes(1)));
        });
        return builder;
    }

    /// <summary>
    /// Derrière le proxy de l'hébergeur (Render) : IP réelle du client et schéma HTTPS d'origine.
    /// ForwardLimit = 1 : seule l'entrée ajoutée par le proxy compte, pas celles envoyées par le client.
    /// Compression des fichiers du site. CORS : inutile en production (site et API sur le même domaine), utile en développement.
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

        // Compression du site (HTML, scripts, styles, SVG) : ~4 fois moins de données sur mobile.
        // Pas les réponses JSON de l'API : petites, et elles contiennent des données du client (prudence face à BREACH).
        builder.Services.AddResponseCompression(o =>
        {
            o.EnableForHttps = true;
            o.Providers.Add<BrotliCompressionProvider>();
            o.Providers.Add<GzipCompressionProvider>();
            o.MimeTypes = ["text/html", "text/css", "text/javascript", "application/javascript", "image/svg+xml"];
        });
        builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);
        builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);

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
            CleClient(http.Connection.RemoteIpAddress),
            _ => new FixedWindowRateLimiterOptions { PermitLimit = limite, Window = fenetre, QueueLimit = 0 });

    /// <summary>
    /// Clé de limitation : l'adresse IPv4, ou le bloc /64 d'une adresse IPv6. Un abonné IPv6 dispose en général
    /// de tout un bloc /64 : compter adresse par adresse lui permettrait de contourner la limite en en changeant.
    /// </summary>
    public static string CleClient(IPAddress? ip)
    {
        if (ip is null) return "inconnue";
        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
        if (ip.AddressFamily != AddressFamily.InterNetworkV6) return ip.ToString();

        var octets = ip.GetAddressBytes();
        Array.Clear(octets, 8, 8);
        return $"{new IPAddress(octets)}/64";
    }
}
