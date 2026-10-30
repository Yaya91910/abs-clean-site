using System.Text.RegularExpressions;
using AbsClean.Api.Endpoints;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.EntityFrameworkCore;

namespace AbsClean.Api.Configuration;

/// <summary>Démarrage et chaîne de traitement des requêtes (appelés depuis Program.cs).</summary>
public static partial class Pipeline
{
    /// <summary>Applique les migrations au démarrage si Database__MigrerAuDemarrage=true (préproduction, tests de bout en bout).</summary>
    public static WebApplication MigrerSiDemande(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>("Database:MigrerAuDemarrage"))
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<AbsCleanDb>().Database.Migrate();
        }
        return app;
    }

    /// <summary>Signale le mode test des envois : simple avertissement en développement, erreur ailleurs (rien ne part !).</summary>
    public static WebApplication SignalerModeEnvois(this WebApplication app)
    {
        var mode = app.Services.GetRequiredService<ModeEnvois>();
        if (mode.Complet) return app;

        const string message = "Envois en MODE TEST (e-mail réel : {Email}, SMS réel : {Sms}) : les messages sont écrits dans un dossier local, rien n'est envoyé. Renseigner Email__ApiKey et Sms__ApiKey.";
        if (app.Environment.IsDevelopment()) app.Logger.LogWarning(message, mode.EmailReel, mode.SmsReel);
        else app.Logger.LogError(message, mode.EmailReel, mode.SmsReel);
        return app;
    }

    /// <summary>En-têtes du proxy, erreurs, HSTS, compression, fichiers statiques du site, puis routage, CORS et limitation de débit.</summary>
    public static WebApplication UtiliserChaine(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>("Proxy:EntetesTransferees"))
            app.UseForwardedHeaders();

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.UseResponseCompression();
        app.UtiliserFichiersPrecompresses(app.Environment);

        // Fichiers du site (wwwroot, présent dans l'image Docker). Ceux dont le nom porte une empreinte
        // (main-YZCV272D.js) changent de nom à chaque version : en cache un an. Les autres (favicon, image
        // de partage, photos) peuvent être remplacés sous le même nom : un jour seulement.
        // index.html est servi à part (PageIndex), jamais mis en cache.
        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = FichiersPrecompresses.TypesDeContenu(),
            OnPrepareResponse = c =>
            {
                FichiersPrecompresses.Preparer(c);
                c.Context.Response.Headers.CacheControl = FichierVersionne().IsMatch(c.File.Name)
                    ? "public, max-age=31536000, immutable"
                    : "public, max-age=86400";
            },
        });
        // Routage explicitement après les fichiers statiques : sinon la route de repli (index.html)
        // est choisie avant eux et le site ne reçoit jamais ses scripts ni ses images.
        app.UseRouting();
        app.UseCors();
        app.UseRateLimiter();
        return app;
    }

    /// <summary>Routes de l'API, puis le site pour toute autre adresse hors /api et hors fichier (/, /devis, /admin/intervention…).</summary>
    public static WebApplication MapperRoutes(this WebApplication app)
    {
        app.MapGet("/api/health", (ModeEnvois mode) => TypedResults.Ok(new
        {
            status = "ok",
            envois = new { email = mode.EmailReel ? "reel" : "test", sms = mode.SmsReel ? "reel" : "test" },
        }));
        app.MapDevisEndpoints();
        app.MapDemandeEndpoints();
        app.MapParrainageEndpoints();
        app.MapReferencementEndpoints();

        var page = app.Services.GetRequiredService<PageIndex>();
        // GET et HEAD : certains services de surveillance testent la disponibilité par une requête HEAD.
        app.MapMethods("/", [HttpMethods.Get, HttpMethods.Head], page.Servir);
        app.MapFallback("{*chemin:regex(^(?!api(/|$)).*$):nonfile}", page.Servir);
        return app;
    }

    /// <summary>Nom avec l'empreinte ajoutée par la compilation Angular (outputHashing) : main-YZCV272D.js (et sa version précompressée .br ou .gz), inter-latin-wght-normal-NRMW37G5.woff2.</summary>
    [GeneratedRegex(@"-[A-Z0-9]{8}\.[a-z0-9]+(\.br|\.gz)?$")]
    private static partial Regex FichierVersionne();
}
