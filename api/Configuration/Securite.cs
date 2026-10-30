namespace AbsClean.Api.Configuration;

/// <summary>Durcissement HTTP : taille des requêtes, HSTS, en-têtes de sécurité (appelés depuis Program.cs).</summary>
public static class Securite
{
    /// <summary>Une demande de devis pèse quelques kilo-octets : au-delà, la requête est refusée (413).</summary>
    public const long TailleMaxRequete = 64 * 1024;

    public static WebApplicationBuilder AjouterSecurite(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(o =>
        {
            o.Limits.MaxRequestBodySize = TailleMaxRequete;
            o.AddServerHeader = false;
        });
        builder.Services.AddHsts(o => o.MaxAge = TimeSpan.FromDays(365));
        return builder;
    }

    /// <summary>
    /// En-têtes ajoutés à toutes les réponses. La politique de contenu (CSP), propre à la page du site,
    /// est posée par <see cref="Infrastructure.PageIndex"/>.
    /// </summary>
    public static WebApplication UtiliserEntetesSecurite(this WebApplication app)
    {
        // Taille annoncée trop grande : refus immédiat, sans lire le corps (Kestrel coupe aussi les envois sans taille annoncée).
        app.Use((http, suite) =>
        {
            if (http.Request.ContentLength > TailleMaxRequete)
            {
                http.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                return Task.CompletedTask;
            }
            return suite(http);
        });

        app.Use((http, suite) =>
        {
            var entetes = http.Response.Headers;
            entetes.XContentTypeOptions = "nosniff";
            entetes.XFrameOptions = "DENY";
            entetes["Referrer-Policy"] = "strict-origin-when-cross-origin";
            // Aucune fonction sensible du navigateur n'est utile au site : toutes refusées, y compris aux contenus intégrés.
            entetes["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=(), accelerometer=(), gyroscope=(), magnetometer=(), display-capture=(), browsing-topics=()";
            entetes["Cross-Origin-Opener-Policy"] = "same-origin";
            entetes["X-Permitted-Cross-Domain-Policies"] = "none";
            return suite(http);
        });
        return app;
    }
}
