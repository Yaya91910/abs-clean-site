namespace AbsClean.Api.Domain;

public sealed record QuantitesDevis(int NbVitres, int NbBaies, int NbVelux, int NbVitrines = 0);

public sealed record LigneDevis(string Code, string Libelle, int Quantite, decimal PrixUnitaire, decimal Montant, bool APartirDe);

public sealed record ResultatDevis(
    IReadOnlyList<LigneDevis> Lignes,
    decimal Total,
    Profil Profil,
    Frequence Frequence,
    decimal? ReductionPourcent,
    string Mention);

/// <summary>Calcul du devis. Fait foi côté serveur : tout prix envoyé par le navigateur est ignoré.</summary>
public static class CalculDevis
{
    /// <summary>Renvoie les erreurs par champ (vide si les quantités sont valides).</summary>
    public static Dictionary<string, string[]> Valider(QuantitesDevis q)
    {
        var erreurs = new Dictionary<string, string[]>();

        void Borne(string champ, int valeur)
        {
            if (valeur is < Tarifs.QuantiteMin or > Tarifs.QuantiteMax)
                erreurs[champ] = [$"La quantité doit être comprise entre {Tarifs.QuantiteMin} et {Tarifs.QuantiteMax}."];
        }

        Borne("nbVitres", q.NbVitres);
        Borne("nbBaies", q.NbBaies);
        Borne("nbVelux", q.NbVelux);
        Borne("nbVitrines", q.NbVitrines);

        if (erreurs.Count == 0 && q.NbVitres + q.NbBaies + q.NbVelux + q.NbVitrines == 0)
            erreurs["quantites"] = ["Ajoutez au moins une vitre, baie vitrée, velux ou vitrine."];

        return erreurs;
    }

    public static ResultatDevis Calculer(QuantitesDevis q, Profil profil, Frequence frequence)
    {
        if (Valider(q) is { Count: > 0 } erreurs)
            throw new ArgumentException(string.Join(" ", erreurs.Values.SelectMany(e => e)), nameof(q));

        var lignes = new List<LigneDevis>();

        void Ajouter(ElementTarif element, int quantite)
        {
            if (quantite > 0)
                lignes.Add(new(element.Code, element.Libelle, quantite, element.PrixUnitaire, quantite * element.PrixUnitaire, element.APartirDe));
        }

        Ajouter(Tarifs.Vitre, q.NbVitres);
        Ajouter(Tarifs.Baie, q.NbBaies);
        Ajouter(Tarifs.Velux, q.NbVelux);
        Ajouter(Tarifs.Vitrine, q.NbVitrines);

        // Profil Particulier / Professionnel : aucune différence de prix connue (À CONFIRMER).
        var reduction = Tarifs.ReductionPourcent(frequence);
        var brut = lignes.Sum(l => l.Montant);
        var total = reduction is > 0 ? Math.Round(brut * (100 - reduction.Value) / 100, 2) : brut;

        return new ResultatDevis(lignes, total, profil, frequence, reduction, Tarifs.MentionEstimation);
    }
}
