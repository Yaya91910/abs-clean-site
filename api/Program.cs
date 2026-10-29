using AbsClean.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder
    .AjouterSecurite()
    .AjouterApi()
    .AjouterDonnees()
    .AjouterEnvois()
    .AjouterAntiAbus()
    .AjouterHebergement();

var app = builder.Build();

app.MigrerSiDemande()
    .SignalerModeEnvois()
    .UtiliserEntetesSecurite()
    .UtiliserChaine()
    .MapperRoutes();

app.Run();

/// <summary>Point d'entrée, exposé pour les tests d'intégration (WebApplicationFactory).</summary>
public partial class Program;
