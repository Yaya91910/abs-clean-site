using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AbsClean.Api.Infrastructure;

/// <summary>
/// Utilisée uniquement par les outils EF Core (dotnet ef migrations …) : construit le contexte sans démarrer l'API.
/// Aucune connexion n'est ouverte pour créer une migration ; pour l'appliquer, la variable ConnectionStrings__Default prime.
/// </summary>
public sealed class AbsCleanDbConception : IDesignTimeDbContextFactory<AbsCleanDb>
{
    public AbsCleanDb CreateDbContext(string[] args)
    {
        var connexion = Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? "Host=localhost;Database=abs_clean_dev";
        return new AbsCleanDb(new DbContextOptionsBuilder<AbsCleanDb>().UseNpgsql(ConnexionPostgres.Normaliser(connexion)).Options);
    }
}
