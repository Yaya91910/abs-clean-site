namespace AbsClean.Api.Infrastructure;

/// <summary>Adresse publique du site (section « Site ») : liens de parrainage, QR codes, lien de validation, aperçu de partage. Domaine À FOURNIR.</summary>
public sealed class SiteOptions
{
    public string Url { get; set; } = "http://localhost:4200";

    public string Base => Url.TrimEnd('/');
}
