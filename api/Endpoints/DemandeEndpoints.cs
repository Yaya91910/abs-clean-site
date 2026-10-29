using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure;

namespace AbsClean.Api.Endpoints;

public static class DemandeEndpoints
{
    public const string PolitiqueDebit = "envoi-demande";

    public static IEndpointRouteBuilder MapDemandeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/demandes", async (NouvelleDemande demande, DemandeStore store, ILogger<NouvelleDemande> log, CancellationToken ct) =>
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

            // PDF, e-mails et SMS : étape suivante (§9, points 4 à 8).
            log.LogInformation("Demande {Numero} enregistrée ({Total} €).", enregistree.Numero, enregistree.Total);

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
