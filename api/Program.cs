using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AbsClean.Api.Endpoints;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<AbsCleanDb>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<DemandeStore>();

// PDF, e-mails et SMS (§9). Sans clé d'API : mode test, tout est écrit dans le dossier Envoi:Test:Dossier.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
builder.Services.Configure<EntrepriseOptions>(builder.Configuration.GetSection("Entreprise"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<SmsOptions>(builder.Configuration.GetSection("Sms"));
builder.Services.Configure<EnvoiTestOptions>(builder.Configuration.GetSection("Envoi:Test"));
builder.Services.AddSingleton(sp => new DevisPdf(sp.GetRequiredService<IOptions<EntrepriseOptions>>().Value));
builder.Services.AddScoped<NotificationsDemande>();

var emailReel = !string.IsNullOrWhiteSpace(builder.Configuration["Email:ApiKey"]);
var smsReel = !string.IsNullOrWhiteSpace(builder.Configuration["Sms:ApiKey"]);
if (emailReel) builder.Services.AddHttpClient<IEnvoiEmail, BrevoEmail>(c => c.Timeout = TimeSpan.FromSeconds(15));
else builder.Services.AddSingleton<IEnvoiEmail, DossierEmail>();
if (smsReel) builder.Services.AddHttpClient<IEnvoiSms, BrevoSms>(c => c.Timeout = TimeSpan.FromSeconds(15));
else builder.Services.AddSingleton<IEnvoiSms, DossierSms>();

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

if (!emailReel || !smsReel)
    app.Logger.LogWarning("Mode test des envois (e-mail réel : {Email}, SMS réel : {Sms}) : messages écrits dans un dossier local, rien n'est envoyé.", emailReel, smsReel);

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseRateLimiter();

app.MapGet("/api/health", () => TypedResults.Ok(new { status = "ok" }));
app.MapDevisEndpoints();
app.MapDemandeEndpoints();

app.Run();
