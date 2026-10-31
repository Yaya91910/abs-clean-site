using AbsClean.Api.Domain;

namespace AbsClean.Api.Endpoints;

/// <summary>Corps de POST /api/devis/estimation. Aucun champ de prix : le total est toujours recalculé.</summary>
public sealed record EstimationRequete(
    Profil Profil = Profil.Particulier,
    Frequence Frequence = Frequence.Ponctuel,
    int NbVitres = 0,
    int NbBaies = 0,
    int NbVelux = 0,
    int NbVitrines = 0);

public static class DevisEndpoints
{
    public static IEndpointRouteBuilder MapDevisEndpoints(this IEndpointRouteBuilder app)
    {
        // Grille identique pour tous : gardée 5 minutes par le navigateur (visites rapprochées, retour sur la page).
        app.MapGet("/api/tarifs", (HttpContext http) =>
        {
            http.Response.Headers.CacheControl = "public, max-age=300";
            return TypedResults.Ok(new
            {
                elements = Tarifs.Elements,
                frequences = Enum.GetValues<Frequence>().Select(f => new
                {
                    code = f,
                    reductionPourcent = Tarifs.ReductionPourcent(f),
                    reductionConnue = Tarifs.ReductionPourcent(f) is not null,
                }),
                profils = Enum.GetValues<Profil>(),
                quantiteMin = Tarifs.QuantiteMin,
                quantiteMax = Tarifs.QuantiteMax,
                mentionEstimation = Tarifs.MentionEstimation,
                mentionOption = Tarifs.MentionOption,
            });
        });

        app.MapPost("/api/devis/estimation", (EstimationRequete requete) =>
        {
            var quantites = new QuantitesDevis(requete.NbVitres, requete.NbBaies, requete.NbVelux, requete.NbVitrines);
            var erreurs = CalculDevis.Valider(quantites);
            return erreurs.Count > 0
                ? Results.ValidationProblem(erreurs)
                : Results.Ok(CalculDevis.Calculer(quantites, requete.Profil, requete.Frequence));
        });

        return app;
    }
}
