using System.Text;
using AbsClean.Api.Domain;

namespace AbsClean.Api.Infrastructure.Envoi;

public static class Sms
{
    public const int LongueurMax = 160;

    /// <summary>
    /// SMS au gérant (CAHIER-DES-CHARGES.md §9.7) : « Nouveau devis ABS : {Prénom} {Nom}, {Ville}, à partir de {total} € - {téléphone} ».
    /// Tient dans un seul SMS facturé : alphabet SMS standard (GSM 7 bits, 160 caractères) — un seul caractère hors
    /// alphabet (« — », « ê », « ’ »…) ferait passer tout le message en Unicode, 70 caractères par SMS, donc 2 SMS.
    /// Nom et ville sont convertis (<see cref="VersGsm7"/>) puis raccourcis si besoin.
    /// </summary>
    public static string NouveauDevis(Demande d)
    {
        string Texte(string identite, string ville) =>
            $"Nouveau devis ABS : {identite}, {ville}, à partir de {DevisPdf.Euros(d.Total)} - {DevisPdf.FormatTelephone(d.Telephone)}";

        var identite = VersGsm7($"{d.Prenom} {d.Nom}");
        var ville = VersGsm7(d.Ville);
        var texte = Texte(identite, ville);
        while (Longueur(texte) > LongueurMax)
        {
            if (identite.Length >= ville.Length) identite = identite[..^2] + ".";
            else ville = ville[..^2] + ".";
            texte = Texte(identite, ville);
        }
        return texte;
    }

    /// <summary>
    /// Texte en alphabet SMS standard : les lettres qui y figurent restent (é, è, à, ù, Ç…), les autres perdent
    /// leur accent (ê → e, ç → c, ÿ → y), quelques signes courants sont remplacés (’ → ', œ → oe, — → -),
    /// et ce qui reste (émojis…) devient « ? ».
    /// </summary>
    public static string VersGsm7(string texte)
    {
        var resultat = new StringBuilder(texte.Length);
        foreach (var r in texte.EnumerateRunes())
        {
            if (!r.IsBmp)
            {
                resultat.Append('?'); // émojis…
                continue;
            }
            var c = (char)r.Value;
            if (Gsm7.Contains(c)) resultat.Append(c);
            else if (Remplacements.TryGetValue(c, out var remplacement)) resultat.Append(remplacement);
            else if (SansAccent(c) is var lettre && Gsm7.Contains(lettre)) resultat.Append(lettre);
            else resultat.Append('?');
        }
        return resultat.ToString();
    }

    private static char SansAccent(char c) => c.ToString().Normalize(NormalizationForm.FormD)[0];

    /// <summary>Longueur facturée en alphabet SMS standard : les caractères de l'extension (€, [, ]…) comptent double.</summary>
    public static int Longueur(string texte) => texte.Sum(c => Gsm7Extension.Contains(c) ? 2 : 1);

    /// <summary>Numéro français (06 12 34 56 78, +33 6…) → format international sans « + » attendu par Brevo (33612345678).</summary>
    public static string? VersInternational(string? numero)
    {
        if (string.IsNullOrWhiteSpace(numero)) return null;
        var chiffres = new string(numero.Where(char.IsDigit).ToArray());
        if (chiffres.Length == 10 && chiffres[0] == '0') return "33" + chiffres[1..];
        return chiffres.Length is >= 6 and <= 15 ? chiffres : null;
    }

    private static readonly Dictionary<char, string> Remplacements = new()
    {
        ['’'] = "'", ['‘'] = "'", ['“'] = "\"", ['”'] = "\"", ['«'] = "\"", ['»'] = "\"",
        ['–'] = "-", ['—'] = "-", ['…'] = "...", ['œ'] = "oe", ['Œ'] = "OE", ['\u00A0'] = " ", ['\u202F'] = " ",
    };

    private const string Gsm7Extension = "^{}\\[~]|€";

    private const string Gsm7 = "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà^{}\\[~]|€";

    public static bool EstGsm7(string texte) => texte.All(c => Gsm7.Contains(c));
}
