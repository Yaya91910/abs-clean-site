using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AbsClean.Api.Endpoints;
using AbsClean.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<AbsCleanDb>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<DemandeStore>();

// Limitation de débit par IP sur l'envoi des demandes (anti-abus, §8).
var limite = builder.Configuration.GetValue("RateLimit:Demandes:Limite", 5);
var fenetre = TimeSpan.FromMinutes(builder.Configuration.GetValue("RateLimit:Demandes:FenetreMinutes", 10));
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(DemandeEndpoints.PolitiqueDebit, http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "inconnue",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = limite, Window = fenetre, QueueLimit = 0 }));
});

// CORS limité au domaine du site (Cors__AllowedOrigin).
var origineAutorisee = builder.Configuration["Cors:AllowedOrigin"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    if (!string.IsNullOrWhiteSpace(origineAutorisee))
        p.WithOrigins(origineAutorisee).AllowAnyHeader().WithMethods("GET", "POST");
}));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseRateLimiter();

app.MapGet("/api/health", () => TypedResults.Ok(new { status = "ok" }));
app.MapDevisEndpoints();
app.MapDemandeEndpoints();

app.Run();
