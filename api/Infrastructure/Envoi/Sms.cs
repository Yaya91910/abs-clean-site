using AbsClean.Api.Domain;

namespace AbsClean.Api.Infrastructure.Envoi;

public static class Sms
{
    public const int LongueurMax = 160;

    /// <summary>
    /// SMS au gérant (CAHIER-DES-CHARGES.md §9.7) : « Nouveau devis ABS : {Prénom} {Nom}, {Ville}, à partir de {total} € — {téléphone} ».
    /// Nom et ville sont raccourcis si besoin pour tenir en 160 caractères.
    /// </summary>
    public static string NouveauDevis(Demande d)
    {
        string Texte(string identite, string ville) =>
            $"Nouveau devis ABS : {identite}, {ville}, à partir de {DevisPdf.Euros(d.Total)} — {DevisPdf.FormatTelephone(d.Telephone)}";

        var identite = $"{d.Prenom} {d.Nom}";
        var ville = d.Ville;
        var texte = Texte(identite, ville);
        while (texte.Length > LongueurMax)
        {
            if (identite.Length >= ville.Length) identite = identite[..^2] + "…";
            else ville = ville[..^2] + "…";
            texte = Texte(identite, ville);
        }
        return texte;
    }

    /// <summary>Numéro français (06 12 34 56 78, +33 6…) → format international sans « + » attendu par Brevo (33612345678).</summary>
    public static string? VersInternational(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero)) return null;
        var chiffres = new string(numero.Where(char.IsDigit).ToArray());
        if (chiffres.Length == 10 && chiffres[0] == '0') return "33" + chiffres[1..];
        return chiffres.Length is >= 6 and <= 15 ? chiffres : null;
    }

    private const string Gsm7 = "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà^{}\\[~]|€";

    public static bool EstGsm7(string texte) => texte.All(c => Gsm7.Contains(c));
}
