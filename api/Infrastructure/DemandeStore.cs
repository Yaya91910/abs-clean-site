using AbsClean.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AbsClean.Api.Infrastructure;

/// <summary>Enregistre les demandes avec un numéro de devis unique et lisible : ABS-2026-0001.</summary>
public sealed class DemandeStore(AbsCleanDb db, TimeProvider horloge)
{
    private const int TentativesMax = 3;

    public async Task<Demande> EnregistrerAsync(NouvelleDemande d, ResultatDevis devis, CancellationToken ct)
    {
        var maintenant = horloge.GetUtcNow().UtcDateTime;
        var demande = new Demande
        {
            CreeLe = maintenant,
            Profil = d.Profil,
            Frequence = d.Frequence,
            NbVitres = d.NbVitres,
            NbBaies = d.NbBaies,
            NbVelux = d.NbVelux,
            NbVitrines = d.NbVitrines,
            Total = devis.Total,
            Nom = d.Nom!.Trim(),
            Prenom = d.Prenom!.Trim(),
            Telephone = ValidationDemande.NormaliserTelephone(d.Telephone!)!,
            Email = d.Email!.Trim(),
            Adresse = d.Adresse!.Trim(),
            CodePostal = d.CodePostal!.Trim(),
            Ville = d.Ville!.Trim(),
            Acces = VideEnNull(d.Acces),
            Creneaux = d.Creneaux?.Distinct().ToList() ?? [],
            Commentaire = VideEnNull(d.Commentaire),
            ConsentementLe = maintenant,
        };

        // Deux envois simultanés peuvent viser le même numéro : l'index unique tranche, on réessaie.
        for (var tentative = 1; ; tentative++)
        {
            demande.Numero = await NumeroSuivantAsync(maintenant.Year, ct);
            db.Demandes.Add(demande);
            try
            {
                await db.SaveChangesAsync(ct);
                return demande;
            }
            catch (DbUpdateException) when (tentative < TentativesMax)
            {
                db.Entry(demande).State = EntityState.Detached;
            }
        }
    }

    private async Task<string> NumeroSuivantAsync(int annee, CancellationToken ct)
    {
        var prefixe = $"ABS-{annee}-";
        var dernier = await db.Demandes
            .Where(x => x.Numero.StartsWith(prefixe))
            .OrderByDescending(x => x.Numero.Length)
            .ThenByDescending(x => x.Numero)
            .Select(x => x.Numero)
            .FirstOrDefaultAsync(ct);

        var rang = dernier is null ? 1 : int.Parse(dernier[prefixe.Length..]) + 1;
        return $"{prefixe}{rang:0000}";
    }

    private static string? VideEnNull(string? valeur) =>
        string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();
}
