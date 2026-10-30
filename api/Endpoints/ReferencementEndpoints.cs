using System.Text;
using System.Xml.Linq;
using AbsClean.Api.Infrastructure;
using Microsoft.Extensions.Options;

namespace AbsClean.Api.Endpoints;

/// <summary>
/// Référencement (§14) : robots.txt et plan du site pour les moteurs de recherche, avec l'adresse publique (Site__Url).
/// La page d'administration et l'API ne sont pas à indexer.
/// </summary>
public static class ReferencementEndpoints
{
    /// <summary>Pages publiques du site, dans l'ordre d'importance.</summary>
    public static readonly string[] Pages = ["/", "/mentions-legales", "/confidentialite", "/parrainage/conditions"];

    public static IEndpointRouteBuilder MapReferencementEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/robots.txt", (IOptions<SiteOptions> site, HttpContext http) =>
        {
            http.Response.Headers.CacheControl = "public, max-age=86400";
            return Results.Text($"""
                User-agent: *
                Disallow: /admin/
                Disallow: /api/

                Sitemap: {site.Value.Base}/sitemap.xml

                """, "text/plain", Encoding.UTF8);
        });

        app.MapGet("/sitemap.xml", (IOptions<SiteOptions> site, HttpContext http) =>
        {
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var plan = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(ns + "urlset", Pages.Select(page => new XElement(ns + "url",
                    new XElement(ns + "loc", site.Value.Base + page),
                    new XElement(ns + "priority", page == "/" ? "1.0" : "0.3")))));
            http.Response.Headers.CacheControl = "public, max-age=86400";
            return Results.Text(plan.Declaration + "\n" + plan.Root, "application/xml", Encoding.UTF8);
        });
        return app;
    }
}
