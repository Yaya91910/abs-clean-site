using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;

namespace AbsClean.Api.Endpoints;

public static class DemandeEndpoints
{
    public const string PolitiqueDebit = "envoi-demande";

    public static IEndpointRouteBuilder MapDemandeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/demandes", async (NouvelleDemande demande, DemandeStore store, NotificationsDemande notifications, ILogger<NouvelleDemande> log, CancellationToken ct) =>
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
            var enregistree = await store.EnregistrerAsync(demande, devis, ct);

            log.LogInformation("Demande {Numero} enregistrée ({Total} €).", enregistree.Numero, enregistree.Total);

            // La demande est enregistrée : les envois ne doivent plus pouvoir faire échouer la réponse,
            // ni être interrompus si le navigateur se déconnecte entre-temps.
            await notifications.EnvoyerAsync(enregistree, CancellationToken.None);

            return Results.Created((string?)null, new
            {
                numero = enregistree.Numero,
                total = enregistree.Total,
                lignes = devis.Lignes,
                mention = devis.Mention,
            });
        })
        .RequireRateLimiting(PolitiqueDebit);

        return app;
    }
}
