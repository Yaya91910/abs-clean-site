using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;

namespace AbsClean.Api.Infrastructure;

/// <summary>
/// Sert les versions précompressées du site (main-XXXX.js.br, .gz, créées au build par web/scripts/precompresser.mjs)
/// quand le navigateur les accepte : Brotli au niveau maximal, sans calcul à chaque visite.
/// Sans fichier précompressé (développement, tests), la compression à la volée prend le relais.
/// </summary>
public static class FichiersPrecompresses
{
    private const string CleOriginal = "precompresse.type";
    private static readonly string[] Extensions = [".js", ".css", ".svg"];
    private static readonly (string Encodage, string Suffixe)[] Encodages = [("br", ".br"), ("gzip", ".gz")];
    private static readonly FileExtensionContentTypeProvider Types = new();

    /// <summary>À placer avant UseStaticFiles : redirige la requête vers la version compressée si elle existe.</summary>
    public static IApplicationBuilder UtiliserFichiersPrecompresses(this IApplicationBuilder app, IWebHostEnvironment env) =>
        app.Use((http, suite) =>
        {
            var chemin = http.Request.Path.Value;
            if (chemin is null || !(HttpMethods.IsGet(http.Request.Method) || HttpMethods.IsHead(http.Request.Method))
                || !Extensions.Any(e => chemin.EndsWith(e, StringComparison.OrdinalIgnoreCase)))
                return suite(http);

            var acceptes = http.Request.Headers.AcceptEncoding.ToString();
            foreach (var (encodage, suffixe) in Encodages)
            {
                if (!acceptes.Contains(encodage, StringComparison.OrdinalIgnoreCase)) continue;
                if (!env.WebRootFileProvider.GetFileInfo(chemin + suffixe).Exists) continue;
                Types.TryGetContentType(chemin, out var type);
                http.Items[CleOriginal] = (type ?? "application/octet-stream", encodage);
                http.Request.Path = chemin + suffixe;
                break;
            }
            return suite(http);
        });

    /// <summary>Types des fichiers .br et .gz pour le serveur de fichiers (corrigés ensuite par <see cref="Preparer"/>).</summary>
    public static IContentTypeProvider TypesDeContenu()
    {
        var types = new FileExtensionContentTypeProvider();
        types.Mappings[".br"] = "application/octet-stream";
        types.Mappings[".gz"] = "application/octet-stream";
        return types;
    }

    /// <summary>Remet le type d'origine et annonce l'encodage (la compression à la volée voit l'en-tête et s'abstient).</summary>
    public static void Preparer(StaticFileResponseContext contexte)
    {
        if (contexte.Context.Items[CleOriginal] is not (string type, string encodage)) return;
        var entetes = contexte.Context.Response.Headers;
        entetes.Vary = HeaderNames.AcceptEncoding;
        entetes.ContentType = type;
        entetes.ContentEncoding = encodage;
    }
}
