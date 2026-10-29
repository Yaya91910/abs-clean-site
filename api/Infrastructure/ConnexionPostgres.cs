using Npgsql;

namespace AbsClean.Api.Infrastructure;

public static class ConnexionPostgres
{
    /// <summary>
    /// Accepte une chaîne Npgsql classique (Host=…;Database=…) ou une URL postgresql://utilisateur:motdepasse@hôte:port/base,
    /// format fourni par Render et la plupart des hébergeurs, que Npgsql ne lit pas directement.
    /// </summary>
    public static string? Normaliser(string? valeur)
    {
        if (string.IsNullOrWhiteSpace(valeur)
            || !(valeur.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) || valeur.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
            return valeur;

        var uri = new Uri(valeur);
        var identifiants = uri.UserInfo.Split(':', 2);
        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(identifiants[0]),
            Password = identifiants.Length > 1 ? Uri.UnescapeDataString(identifiants[1]) : null,
            SslMode = SslMode.Prefer,
        }.ConnectionString;
    }
}
