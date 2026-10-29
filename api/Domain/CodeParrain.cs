using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace AbsClean.Api.Domain;

/// <summary>
/// Codes de parrainage lisibles : « ABS- » + prénom en majuscules sans accents (6 lettres max)
/// + 1 à 2 caractères aléatoires, sans caractères ambigus (0/O, 1/I). Ex. ABS-MARIE7.
/// </summary>
public static class CodeParrain
{
    public const string Prefixe = "ABS-";

    /// <summary>Caractères aléatoires autorisés : ni 0, O, 1 ni I.</summary>
    public const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Racine(string prenom)
    {
        var sansAccents = new string(prenom.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());
        var lettres = new string(sansAccents.ToUpperInvariant().Where(c => c is >= 'A' and <= 'Z').Take(6).ToArray());
        return Prefixe + (lettres.Length > 0 ? lettres : "CLIENT");
    }

    public static string Generer(string prenom, int nbAleatoires) =>
        Racine(prenom) + new string(Enumerable.Range(0, nbAleatoires).Select(_ => Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)]).ToArray());

    /// <summary>Code saisi par un client : espaces retirés, majuscules. Null si vide.</summary>
    public static string? Normaliser(string? saisi)
    {
        var code = new string((saisi ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        return code.Length == 0 ? null : code;
    }
}
