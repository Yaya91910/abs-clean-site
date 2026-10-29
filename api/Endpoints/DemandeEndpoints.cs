using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.Extensions.Options;

namespace AbsClean.Api.Endpoints;

/// <summary>Réponse de POST /api/demandes.</summary>
/// <param name="CodeParrainageSaisi">Sort du code saisi : « applique », « ignore » (inconnu, auto-parrainage…) ou null si aucun.</param>
public sealed record DemandeAcceptee(
    string Numero,
    decimal Total,
    IReadOnlyList<LigneDevis> Lignes,
    string Mention,
    string CodeParrain,
    string LienParrainage,
    string? CodeParrainageSaisi);

public static class DemandeEndpoints
{
    public const string PolitiqueDebit = "envoi-demande";

    public static IEndpointRouteBuilder MapDemandeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/demandes", EnregistrerAsync).RequireRateLimiting(PolitiqueDebit);
        return app;
    }

    private static async Task<IResult> EnregistrerAsync(
        NouvelleDemande demande,
        DemandeStore store,
        ServiceParrainage parrainage,
        FileEnvois file,
        NotificationsDemande notifications,
        IOptions<EnvoisOptions> envois,
        ILogger<NouvelleDemande> log,
        CancellationToken ct)
    {
        // Honeypot rempli : un robot. Réponse volontairement générique.
        if (demande.HoneypotRempli)
        {
            log.LogWarning("Demande rejetée : champ piège rempli.");
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Demande refusée.");
        }

        var erreurs = ValidationDemande.Valider(demande);
        if (erreurs.Count > 0)
            return Results.ValidationProblem(erreurs);

        // Le calcul fait foi côté serveur (§7).
        var devis = CalculDevis.Calculer(demande.Quantites, demande.Profil, demande.Frequence);
        var (enregistree, jeton, codeSaisi) = await store.EnregistrerAsync(demande, devis, ct);
        log.LogInformation("Demande {Numero} enregistrée ({Total} €).", enregistree.Numero, enregistree.Total);

        // La demande est enregistrée : les envois ne peuvent plus faire échouer la réponse (§9.9).
        if (envois.Value.EnArrierePlan)
            await file.AjouterAsync(new EnvoiAFaire(enregistree.Id, jeton));
        else
            await notifications.EnvoyerAsync(enregistree, jeton, CancellationToken.None);

        return Results.Created((string?)null, new DemandeAcceptee(
            enregistree.Numero,
            enregistree.Total,
            devis.Lignes,
            devis.Mention,
            enregistree.CodeParrainPersonnel,
            parrainage.Lien(enregistree.CodeParrainPersonnel),
            codeSaisi switch
            {
                CodeParrainSaisi.Aucun => null,
                CodeParrainSaisi.Applique => "applique",
                _ => "ignore",
            }));
    }
}
