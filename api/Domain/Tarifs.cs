namespace AbsClean.Api.Domain;

public enum Profil { Particulier, Professionnel }

public enum Frequence { Ponctuel, Mensuel, Trimestriel }

/// <summary>Un élément facturable du devis (vitre, baie vitrée…).</summary>
public sealed record ElementTarif(string Code, string Libelle, decimal PrixUnitaire, bool APartirDe);

/// <summary>
/// Grille tarifaire (CAHIER-DES-CHARGES.md §7). Source unique : le front la lit via GET /api/tarifs.
/// </summary>
public static class Tarifs
{
    public const int QuantiteMin = 0;
    public const int QuantiteMax = 99;

    public const string MentionEstimation = "Estimation confirmée après échange";
    public const string MentionOption = "Encadrements, rails et rebords : sur devis";

    public static readonly ElementTarif Vitre = new("vitre", "Vitre", 20m, APartirDe: false);
    public static readonly ElementTarif Baie = new("baie", "Baie vitrée", 40m, APartirDe: true);
    public static readonly ElementTarif Velux = new("velux", "Velux", 25m, APartirDe: false);
    public static readonly ElementTarif Vitrine = new("vitrine", "Vitrine", 50m, APartirDe: true);

    public static IReadOnlyList<ElementTarif> Elements { get; } = [Vitre, Baie, Velux, Vitrine];

    /// <summary>
    /// Pourcentage de réduction par fréquence. À FOURNIR par le gérant :
    /// tant que la valeur est null, aucune réduction n'est appliquée et l'interface l'indique.
    /// </summary>
    public static decimal? ReductionPourcent(Frequence frequence) => frequence switch
    {
        Frequence.Ponctuel => 0m,
        _ => null,
    };
}
