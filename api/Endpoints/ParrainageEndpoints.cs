using AbsClean.Api.Domain;
using AbsClean.Api.Infrastructure;

namespace AbsClean.Api.Endpoints;

public static class ParrainageEndpoints
{
    public const string PolitiqueConsultation = "consultation-parrainage";

    public static IEndpointRouteBuilder MapParrainageEndpoints(this IEndpointRouteBuilder app)
    {
        // Vérifie qu'un code existe, sans exposer de données personnelles.
        app.MapGet("/api/parrainage/{code}", async (string code, ServiceParrainage service, CancellationToken ct) =>
            CodeParrain.Normaliser(code) is { } c && await service.ExisteAsync(c, ct)
                ? Results.Ok(new { code = c, valide = true })
                : Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Code de parrainage inconnu."))
            .RequireRateLimiting(PolitiqueConsultation);

        app.MapGet("/api/parrainage/{code}/qr.svg", async (string code, ServiceParrainage service, HttpContext http, CancellationToken ct) =>
        {
            if (CodeParrain.Normaliser(code) is not { } c || !await service.ExisteAsync(c, ct))
                return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Code de parrainage inconnu.");
            http.Response.Headers.CacheControl = "public, max-age=86400";
            return Results.Text(service.QrSvg(c), "image/svg+xml");
        }).RequireRateLimiting(PolitiqueConsultation);

        app.MapGet("/api/parrainage/{code}/qr.png", async (string code, ServiceParrainage service, HttpContext http, CancellationToken ct) =>
        {
            if (CodeParrain.Normaliser(code) is not { } c || !await service.ExisteAsync(c, ct))
                return Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Code de parrainage inconnu.");
            http.Response.Headers.CacheControl = "public, max-age=86400";
            return Results.File(service.QrPng(c), "image/png");
        }).RequireRateLimiting(PolitiqueConsultation);

        // Validation par le gérant via le lien à usage unique de son e-mail (la page du site appelle ce POST).
        app.MapPost("/api/admin/demandes/{id:int}/intervention-realisee", async (int id, ValidationIntervention corps, ServiceParrainage service, CancellationToken ct) =>
            await service.ValiderInterventionAsync(id, corps.Jeton, ct) switch
            {
                ResultatValidation.Validee => Results.Ok(new { valide = true }),
                ResultatValidation.DejaValidee => Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Intervention déjà confirmée pour cette demande."),
                ResultatValidation.Introuvable or ResultatValidation.JetonInvalide or _ =>
                    Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Lien de validation invalide ou expiré."),
            }).RequireRateLimiting(PolitiqueConsultation);

        return app;
    }
}

public sealed record ValidationIntervention(string? Jeton);
