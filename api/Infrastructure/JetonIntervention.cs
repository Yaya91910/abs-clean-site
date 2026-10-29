using System.Security.Cryptography;
using System.Text;

namespace AbsClean.Api.Infrastructure;

/// <summary>Jeton du lien « intervention réalisée » envoyé au gérant : aléatoire (256 bits), seule son empreinte est stockée.</summary>
public static class JetonIntervention
{
    public static string Nouveau() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string Empreinte(string jeton) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(jeton)));
}
