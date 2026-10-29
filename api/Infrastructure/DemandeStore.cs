using AbsClean.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AbsClean.Api.Infrastructure;

/// <summary>Sort du code de parrainage saisi par le client (un code refusé n'empêche jamais l'envoi).</summary>
public enum CodeParrainSaisi { Aucun, Applique, Inconnu, AutoParrainage, DejaParraine }

/// <param name="JetonIntervention">Jeton brut du lien « intervention réalisée » envoyé au gérant ; seule son empreinte est stockée.</param>
public sealed record DemandeEnregistree(Demande Demande, string JetonIntervention, CodeParrainSaisi CodeParrain);

/// <summary>
/// Enregistre les demandes avec un numéro de devis unique (ABS-2026-0001), un code de parrainage personnel
/// unique, et applique le code de parrainage saisi s'il est valable (CAHIER-DES-CHARGES.md §9 bis).
/// </summary>
public sealed class DemandeStore(AbsCleanDb db, TimeProvider horloge, ILogger<DemandeStore> log)
{
    private const int TentativesMax = 3;

    public async Task<DemandeEnregistree> EnregistrerAsync(NouvelleDemande d, ResultatDevis devis, CancellationToken ct)
    {
        var maintenant = horloge.GetUtcNow().UtcDateTime;
        var jeton = JetonIntervention.Nouveau();
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
            JetonInterventionHash = JetonIntervention.Empreinte(jeton),
        };

        var (statutCode, parrain) = await VerifierCodeSaisiAsync(CodeParrain.Normaliser(d.CodeParrainage), demande, ct);
        if (parrain is not null)
        {
            demande.CodeParrainUtilise = parrain.CodeParrainPersonnel;
            demande.ParrainId = parrain.Id;
        }

        // Deux envois simultanés peuvent viser le même numéro ou le même code : les index uniques tranchent, on réessaie.
        for (var tentative = 1; ; tentative++)
        {
            // Année française : un devis du 1er janvier à 0 h 30 porte bien le numéro de la nouvelle année.
            demande.Numero = await NumeroSuivantAsync(HeureParis.Depuis(maintenant).Year, ct);
            demande.CodeParrainPersonnel = await CodeLibreAsync(demande.Prenom, ct);
            db.Demandes.Add(demande);
            var parrainage = parrain is null ? null : new Parrainage { ParrainId = parrain.Id, Filleul = demande, CreeLe = maintenant };
            if (parrainage is not null) db.Parrainages.Add(parrainage);
            try
            {
                await db.SaveChangesAsync(ct);
                return new DemandeEnregistree(demande, jeton, statutCode);
            }
            catch (DbUpdateException) when (tentative < TentativesMax)
            {
                db.Entry(demande).State = EntityState.Detached;
                if (parrainage is not null) db.Entry(parrainage).State = EntityState.Detached;
            }
        }
    }

    /// <summary>Anti-fraude : code inconnu ignoré, auto-parrainage refusé, un seul parrain par client.</summary>
    private async Task<(CodeParrainSaisi, Demande?)> VerifierCodeSaisiAsync(string? code, Demande filleul, CancellationToken ct)
    {
        if (code is null) return (CodeParrainSaisi.Aucun, null);

        var parrain = await db.Demandes.AsNoTracking().FirstOrDefaultAsync(x => x.CodeParrainPersonnel == code, ct);
        if (parrain is null)
        {
            log.LogInformation("Code de parrainage inconnu ignoré : {Code}.", code);
            return (CodeParrainSaisi.Inconnu, null);
        }

        var memeAdresse = parrain.CodePostal == filleul.CodePostal && Simplifier(parrain.Adresse) == Simplifier(filleul.Adresse);
        if (parrain.Telephone == filleul.Telephone || string.Equals(parrain.Email, filleul.Email, StringComparison.OrdinalIgnoreCase) || memeAdresse)
        {
            log.LogWarning("Auto-parrainage refusé : code {Code}.", code);
            return (CodeParrainSaisi.AutoParrainage, null);
        }

        var email = filleul.Email.ToLower();
        var dejaParraine = await db.Demandes.AnyAsync(
            x => x.ParrainId != null && (x.Telephone == filleul.Telephone || x.Email.ToLower() == email), ct);
        if (dejaParraine)
        {
            log.LogInformation("Client déjà parrainé : code {Code} ignoré.", code);
            return (CodeParrainSaisi.DejaParraine, null);
        }

        return (CodeParrainSaisi.Applique, parrain);
    }

    /// <summary>1 caractère aléatoire si possible, 2 sinon (3 en dernier recours, journalisé).</summary>
    private async Task<string> CodeLibreAsync(string prenom, CancellationToken ct)
    {
        foreach (var (nb, essais) in new[] { (1, 8), (2, 30) })
        {
            for (var i = 0; i < essais; i++)
            {
                var code = CodeParrain.Generer(prenom, nb);
                if (!await db.Demandes.AnyAsync(x => x.CodeParrainPersonnel == code, ct)) return code;
            }
        }
        log.LogWarning("Codes à 1 et 2 caractères épuisés pour la racine {Racine} : code à 3 caractères.", CodeParrain.Racine(prenom));
        return CodeParrain.Generer(prenom, 3);
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

    private static string Simplifier(string adresse) => new(adresse.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static string? VideEnNull(string? valeur) =>
        string.IsNullOrWhiteSpace(valeur) ? null : valeur.Trim();
}
