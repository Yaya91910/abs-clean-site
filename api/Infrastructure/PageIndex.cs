using Microsoft.Extensions.Options;

namespace AbsClean.Api.Infrastructure;

/// <summary>
/// Page d'accueil du site compilé (wwwroot/index.html), servie avec l'adresse publique du site à la place de __SITE_URL__ :
/// les aperçus de partage (Open Graph) exigent des adresses absolues.
/// </summary>
public sealed class PageIndex(IWebHostEnvironment env, IOptions<SiteOptions> site)
{
    private readonly Lazy<string?> _html = new(() =>
    {
        var fichier = env.WebRootFileProvider.GetFileInfo("index.html");
        if (!fichier.Exists) return null;
        using var lecteur = new StreamReader(fichier.CreateReadStream());
        return lecteur.ReadToEnd().Replace("__SITE_URL__", site.Value.Base);
    });

    public IResult Servir(HttpContext http)
    {
        if (_html.Value is not { } html) return Results.NotFound();
        http.Response.Headers.CacheControl = "no-cache";
        return Results.Content(html, "text/html; charset=utf-8");
    }
}
