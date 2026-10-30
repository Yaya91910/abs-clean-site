using System.Threading.Channels;
using AbsClean.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AbsClean.Api.Infrastructure.Envoi;

/// <summary>Envoi à traiter : la demande et le jeton brut de son lien de validation (jamais stocké en clair).</summary>
public sealed record EnvoiAFaire(int DemandeId, string JetonIntervention);

/// <summary>File des envois : la réponse au client part dès l'enregistrement, les envois suivent.</summary>
public sealed class FileEnvois
{
    private readonly Channel<EnvoiAFaire> _file = Channel.CreateUnbounded<EnvoiAFaire>(new() { SingleReader = true });

    public ValueTask AjouterAsync(EnvoiAFaire envoi) => _file.Writer.WriteAsync(envoi);

    public IAsyncEnumerable<EnvoiAFaire> LireAsync(CancellationToken ct) => _file.Reader.ReadAllAsync(ct);
}

/// <summary>
/// Traite la file des envois. Au démarrage, reprend les demandes récentes dont aucun envoi n'a abouti
/// (serveur redémarré entre l'enregistrement et les envois, ou service d'envoi indisponible).
/// </summary>
public sealed class EnvoisEnArrierePlan(FileEnvois file, IServiceScopeFactory scopes, TimeProvider horloge, ILogger<EnvoisEnArrierePlan> log)
    : BackgroundService
{
    /// <summary>Au-delà, une demande sans aucun envoi n'est plus relancée automatiquement.</summary>
    public static readonly TimeSpan FenetreReprise = TimeSpan.FromHours(48);

    protected override async Task ExecuteAsync(CancellationToken arret)
    {
        try
        {
            await ReprendreAsync(arret);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            log.LogError(e, "Reprise des envois en attente impossible.");
        }

        await foreach (var envoi in file.LireAsync(arret))
            await TraiterAsync(envoi, arret);
    }

    public async Task<int> ReprendreAsync(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AbsCleanDb>();
        var depuis = horloge.GetUtcNow().UtcDateTime - FenetreReprise;
        var enAttente = await db.Demandes
            .Where(d => d.CreeLe >= depuis && !d.EmailClientEnvoye && !d.EmailGerantEnvoye && !d.SmsEnvoye && d.InterventionRealiseeLe == null)
            .ToListAsync(ct);

        foreach (var demande in enAttente)
        {
            // Le jeton d'origine n'est connu que par son empreinte : on en crée un nouveau pour le lien du gérant.
            var jeton = JetonIntervention.Nouveau();
            demande.JetonInterventionHash = JetonIntervention.Empreinte(jeton);
            await db.SaveChangesAsync(ct);
            await file.AjouterAsync(new EnvoiAFaire(demande.Id, jeton));
        }

        if (enAttente.Count > 0)
            log.LogWarning("{Nombre} demande(s) sans envoi reprise(s) au démarrage.", enAttente.Count);
        return enAttente.Count;
    }

    private async Task TraiterAsync(EnvoiAFaire envoi, CancellationToken arret)
    {
        try
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AbsCleanDb>();
            var demande = await db.Demandes.Include(d => d.Client).FirstOrDefaultAsync(d => d.Id == envoi.DemandeId, arret);
            if (demande is null) return;

            await scope.ServiceProvider.GetRequiredService<NotificationsDemande>().EnvoyerAsync(demande, envoi.JetonIntervention, arret);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // Les échecs d'envoi sont déjà gérés un par un ; ici, une erreur inattendue ne doit pas arrêter la file.
            log.LogError(e, "Envois de la demande {Id} interrompus.", envoi.DemandeId);
        }
    }
}
