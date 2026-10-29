using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AbsClean.Api.Endpoints;
using AbsClean.Api.Infrastructure;
using AbsClean.Api.Infrastructure.Envoi;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<AbsCleanDb>(o => o.UseNpgsql(ConnexionPostgres.Normaliser(builder.Configuration.GetConnectionString("Default"))));

// Derrière le proxy de l'hébergeur (Render) : IP réelle du client (limitation de débit) et schéma HTTPS d'origine.
// ForwardLimit = 1 : seule l'entrée ajoutée par le proxy est prise en compte, pas celles envoyées par le client.
if (builder.Configuration.GetValue<bool>("Proxy:EntetesTransferees"))
{
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        o.ForwardLimit = 1;
        o.KnownIPNetworks.Clear();
        o.KnownProxies.Clear();
    });
}
builder.Services.AddScoped<DemandeStore>();

// PDF, e-mails et SMS (§9). Sans clé d'API : mode test, tout est écrit dans le dossier Envoi:Test:Dossier.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
builder.Services.Configure<EntrepriseOptions>(builder.Configuration.GetSection("Entreprise"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<SmsOptions>(builder.Configuration.GetSection("Sms"));
builder.Services.Configure<EnvoiTestOptions>(builder.Configuration.GetSection("Envoi:Test"));
builder.Services.AddSingleton(sp => new DevisPdf(sp.GetRequiredService<IOptions<EntrepriseOptions>>().Value));
builder.Services.AddScoped<NotificationsDemande>();

// Parrainage (§9 bis) : Site__Url sert de base aux liens de parrainage, aux QR codes et au lien de validation du gérant.
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection("Site"));
builder.Services.AddScoped<ServiceParrainage>();

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
    o.AddPolicy(ParrainageEndpoints.PolitiqueConsultation, http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "inconnue",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

// CORS limité au domaine du site (Cors__AllowedOrigin).
var origineAutorisee = builder.Configuration["Cors:AllowedOrigin"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    if (!string.IsNullOrWhiteSpace(origineAutorisee))
        p.WithOrigins(origineAutorisee).AllowAnyHeader().WithMethods("GET", "POST");
}));

var app = builder.Build();

// Applique les migrations au démarrage si demandé (préproduction, tests de bout en bout).
if (app.Configuration.GetValue<bool>("Database:MigrerAuDemarrage"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AbsCleanDb>().Database.Migrate();
}

if (!emailReel || !smsReel)
    app.Logger.LogWarning("Mode test des envois (e-mail réel : {Email}, SMS réel : {Sms}) : messages écrits dans un dossier local, rien n'est envoyé.", emailReel, smsReel);

if (app.Configuration.GetValue<bool>("Proxy:EntetesTransferees"))
    app.UseForwardedHeaders();

app.UseExceptionHandler();
app.UseStatusCodePages();
if (!app.Environment.IsDevelopment())
    app.UseHsts();

// Site Angular compilé (wwwroot, présent dans l'image Docker) : fichiers versionnés mis en cache, index.html jamais.
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = c => c.Context.Response.Headers.CacheControl =
        c.File.Name == "index.html" ? "no-cache" : "public, max-age=31536000, immutable",
});
// Routage explicitement après les fichiers statiques : sinon la route de repli (index.html) est choisie
// avant eux et le site ne reçoit jamais ses scripts ni ses images.
app.UseRouting();

app.UseCors();
app.UseRateLimiter();

app.MapGet("/api/health", () => TypedResults.Ok(new { status = "ok" }));
app.MapDevisEndpoints();
app.MapDemandeEndpoints();
app.MapParrainageEndpoints();

// Toute autre adresse hors /api (/devis, /admin/intervention…) affiche le site.
app.MapFallbackToFile("{*chemin:regex(^(?!api(/|$)).*$):nonfile}", "index.html", new StaticFileOptions
{
    OnPrepareResponse = c => c.Context.Response.Headers.CacheControl = "no-cache",
});

app.Run();
