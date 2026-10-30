using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AbsClean.Api.Domain;

/// <summary>
/// Codes de parrainage : « ABS- » + 7 chiffres aléatoires, le premier non nul. Ex. ABS-1765321.
/// Que des chiffres : rien d'ambigu à recopier (0/O, 1/I) ni de donnée personnelle dans le code.
/// </summary>
public static partial class CodeParrain
{
    public const string Prefixe = "ABS-";
    public const int NbChiffres = 7;

    public static string Generer() =>
        Prefixe + RandomNumberGenerator.GetInt32(1_000_000, 10_000_000).ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// Code saisi par un client : espaces retirés, majuscules ; « 1765321 » ou « ABS1765321 » deviennent « ABS-1765321 ».
    /// Les anciens codes (ex. ABS-MARIE7) restent acceptés tels quels. Null si vide.
    /// </summary>
    public static string? Normaliser(string? saisi)
    {
        var code = new string((saisi ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if (code.Length == 0) return null;
        return SansTiret().Match(code) is { Success: true } m ? Prefixe + m.Groups[1].Value : code;
    }

    [GeneratedRegex(@"^(?:ABS)?(\d{7})$")]
    private static partial Regex SansTiret();
}
