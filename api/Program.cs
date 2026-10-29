using System.Text.Json;
using System.Text.Json.Serialization;
using AbsClean.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

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

app.MapGet("/api/health", () => TypedResults.Ok(new { status = "ok" }));
app.MapDevisEndpoints();

app.Run();
