using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace AbsClean.Api.Infrastructure;

/// <summary>
/// Page du site compilé (wwwroot/index.html), servie :
/// - avec l'adresse publique du site à la place de __SITE_URL__ (les aperçus de partage exigent des adresses absolues) ;
/// - avec une politique de contenu (CSP) qui n'autorise que les scripts du site et, par leur empreinte,
///   les quelques scripts intégrés à la page par la compilation Angular.
/// </summary>
public sealed partial class PageIndex(IWebHostEnvironment env, IOptions<SiteOptions> site)
{
    private readonly Lazy<(string Html, string Csp)?> _page = new(() =>
    {
        var fichier = env.WebRootFileProvider.GetFileInfo("index.html");
        if (!fichier.Exists) return null;
        using var lecteur = new StreamReader(fichier.CreateReadStream());
        var html = lecteur.ReadToEnd().Replace("__SITE_URL__", site.Value.Base);
        return (html, PolitiqueDeContenu(html));
    });

    public IResult Servir(HttpContext http)
    {
        if (_page.Value is not var (html, csp)) return Results.NotFound();
        http.Response.Headers.CacheControl = "no-cache";
        http.Response.Headers.ContentSecurityPolicy = csp;
        // Page du gérant (lien de son e-mail) : jamais dans les moteurs de recherche.
        if (http.Request.Path.StartsWithSegments("/admin"))
            http.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
        return Results.Content(html, "text/html; charset=utf-8");
    }

    public static string PolitiqueDeContenu(string html)
    {
        var empreintes = ScriptIntegre().Matches(html)
            .Select(m => $"'sha256-{Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(m.Groups[1].Value)))}'");
        return string.Join("; ",
            "default-src 'self'",
            $"script-src 'self' {string.Join(' ', empreintes)}".TrimEnd(),
            // Styles des composants Angular et attributs style : intégrés à la page.
            "style-src 'self' 'unsafe-inline'",
            "img-src 'self' data:",
            "font-src 'self'",
            "connect-src 'self'",
            // Ni cadres intégrés ni workers : le site n'en utilise pas.
            "frame-src 'none'",
            "worker-src 'none'",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'",
            "frame-ancestors 'none'");
    }

    [GeneratedRegex(@"<script>(.*?)</script>", RegexOptions.Singleline)]
    private static partial Regex ScriptIntegre();
}
