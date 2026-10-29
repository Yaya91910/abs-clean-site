using AbsClean.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AbsClean.Api.Infrastructure;

public sealed class AbsCleanDb(DbContextOptions<AbsCleanDb> options) : DbContext(options)
{
    public DbSet<Demande> Demandes => Set<Demande>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var demande = modelBuilder.Entity<Demande>();

        demande.HasIndex(d => d.Numero).IsUnique();
        demande.Property(d => d.Numero).HasMaxLength(20);

        demande.Property(d => d.Profil).HasConversion<string>().HasMaxLength(20);
        demande.Property(d => d.Frequence).HasConversion<string>().HasMaxLength(20);
        demande.Property(d => d.Statut).HasConversion<string>().HasMaxLength(20);
        demande.Property(d => d.Total).HasPrecision(10, 2);

        demande.Property(d => d.Nom).HasMaxLength(60);
        demande.Property(d => d.Prenom).HasMaxLength(60);
        demande.Property(d => d.Telephone).HasMaxLength(10);
        demande.Property(d => d.Email).HasMaxLength(254);
        demande.Property(d => d.Adresse).HasMaxLength(200);
        demande.Property(d => d.CodePostal).HasMaxLength(5);
        demande.Property(d => d.Ville).HasMaxLength(100);
        demande.Property(d => d.Acces).HasMaxLength(200);
        demande.Property(d => d.Commentaire).HasMaxLength(ValidationDemande.CommentaireMax);

        // Créneaux stockés en texte ("SemaineMatin,Samedi") : portable PostgreSQL / SQLite.
        demande.Property(d => d.Creneaux)
            .HasConversion(
                c => string.Join(',', c),
                s => s.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Enum.Parse<Creneau>).ToList(),
                new ValueComparer<List<Creneau>>(
                    (a, b) => a!.SequenceEqual(b!),
                    c => c.Aggregate(0, (h, v) => HashCode.Combine(h, v)),
                    c => c.ToList()))
            .HasMaxLength(100);
    }
}
