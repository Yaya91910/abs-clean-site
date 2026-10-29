using System.Text.RegularExpressions;

namespace AbsClean.Api.Domain;

/// <summary>
/// Corps de POST /api/demandes (CLAUDE.md §8). Aucun champ de prix : le total est recalculé côté serveur.
/// <see cref="SiteWeb"/> est le champ piège (honeypot), invisible pour un humain.
/// </summary>
public sealed record NouvelleDemande(
    Profil Profil = Profil.Particulier,
    Frequence Frequence = Frequence.Ponctuel,
    int NbVitres = 0,
    int NbBaies = 0,
    int NbVelux = 0,
    int NbVitrines = 0,
    string? Nom = null,
    string? Prenom = null,
    string? Telephone = null,
    string? Email = null,
    string? Adresse = null,
    string? CodePostal = null,
    string? Ville = null,
    string? Acces = null,
    IReadOnlyList<Creneau>? Creneaux = null,
    string? Commentaire = null,
    string? CodeParrainage = null,
    bool Consentement = false,
    string? SiteWeb = null)
{
    public QuantitesDevis Quantites => new(NbVitres, NbBaies, NbVelux, NbVitrines);

    public bool HoneypotRempli => !string.IsNullOrWhiteSpace(SiteWeb);
}

public static partial class ValidationDemande
{
    public const int CommentaireMax = 1000;

    /// <summary>Renvoie les erreurs par champ (mêmes règles que le formulaire, §8). Vide si tout est valide.</summary>
    public static Dictionary<string, string[]> Valider(NouvelleDemande d)
    {
        var erreurs = CalculDevis.Valider(d.Quantites);

        void Erreur(string champ, string message) => erreurs[champ] = [message];

        void Requis(string champ, string? valeur, int max, string libelle, bool feminin = false)
        {
            var v = valeur?.Trim() ?? "";
            if (v.Length == 0) Erreur(champ, $"{libelle} est {(feminin ? "requise" : "requis")}.");
            else if (v.Length > max) Erreur(champ, $"{libelle} ne doit pas dépasser {max} caractères.");
        }

        void Facultatif(string champ, string? valeur, int max, string libelle)
        {
            if ((valeur?.Trim().Length ?? 0) > max) Erreur(champ, $"{libelle} ne doit pas dépasser {max} caractères.");
        }

        Requis("nom", d.Nom, 60, "Le nom");
        Requis("prenom", d.Prenom, 60, "Le prénom");

        if (string.IsNullOrWhiteSpace(d.Telephone)) Erreur("telephone", "Le téléphone est requis.");
        else if (NormaliserTelephone(d.Telephone) is null) Erreur("telephone", "Numéro de téléphone français invalide (ex. 06 12 34 56 78).");

        if (string.IsNullOrWhiteSpace(d.Email)) Erreur("email", "L'e-mail est requis.");
        else if (d.Email.Trim().Length > 254 || !EmailRegex().IsMatch(d.Email.Trim())) Erreur("email", "Adresse e-mail invalide.");

        Requis("adresse", d.Adresse, 200, "L'adresse d'intervention", feminin: true);

        if (string.IsNullOrWhiteSpace(d.CodePostal)) Erreur("codePostal", "Le code postal est requis.");
        else if (!CodePostalRegex().IsMatch(d.CodePostal.Trim())) Erreur("codePostal", "Le code postal doit contenir 5 chiffres.");

        Requis("ville", d.Ville, 100, "La ville", feminin: true);
        Facultatif("acces", d.Acces, 200, "L'accès");
        Facultatif("commentaire", d.Commentaire, CommentaireMax, "Le commentaire");

        if (!d.Consentement) Erreur("consentement", "Votre accord est nécessaire pour que nous puissions vous recontacter.");

        return erreurs;
    }

    /// <summary>
    /// Numéro français 01–05, 06, 07, 09 (espaces tolérés) → 10 chiffres sans espace, ou null s'il est invalide.
    /// </summary>
    public static string? NormaliserTelephone(string telephone)
    {
        var chiffres = telephone.Replace(" ", "");
        return TelephoneRegex().IsMatch(chiffres) ? chiffres : null;
    }

    [GeneratedRegex(@"^0[1-79]\d{8}$")]
    private static partial Regex TelephoneRegex();

    [GeneratedRegex(@"^\d{5}$")]
    private static partial Regex CodePostalRegex();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
