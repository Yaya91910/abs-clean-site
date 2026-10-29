# ABS clean — site de devis

Site vitrine et devis en ligne pour ABS clean (nettoyage de surfaces vitrées, Essonne et Île-de-France).
Le cahier des charges complet est dans [CLAUDE.md](CLAUDE.md) ; la référence visuelle dans [maquette/](maquette/).

| Dossier | Contenu |
|---|---|
| `api/` | API ASP.NET Core 10 (Minimal API) — `Domain/` calcul du devis, `Endpoints/`, `Infrastructure/`, `Tests/` (xUnit) |
| `web/` | Front Angular 22 — une section par composant dans `src/app/sections/` |
| `assets/` | Logo source (SVG / PNG) |
| `e2e/` | Tests Playwright (à venir) |

## Prérequis

- [.NET SDK 10](https://dotnet.microsoft.com/download) (LTS)
- Node.js 22 LTS (ou 24) et npm

## Lancer en local

Dans deux terminaux :

```bash
# API sur http://localhost:5188
dotnet run --project api --launch-profile http
```

```bash
# Front sur http://localhost:4200 (les appels /api sont redirigés vers l'API, cf. web/proxy.conf.json)
cd web && npm install && npm start
```

## Tests

```bash
dotnet test AbsClean.slnx
```

```bash
cd web && npx ng test --watch=false
```

## API

| Méthode | Route | État |
|---|---|---|
| GET | `/api/health` | ✅ |
| GET | `/api/tarifs` | ✅ |
| POST | `/api/devis/estimation` | ✅ |
| POST | `/api/demandes` | à venir |
| GET | `/api/parrainage/{code}` et `/qr.svg` | à venir |
| POST | `/api/admin/demandes/{id}/intervention-realisee` | à venir |

Les erreurs sont renvoyées au format `ProblemDetails`. Le total est toujours recalculé côté serveur.

## Configuration

Toutes les clés et numéros passent par des variables d'environnement : voir [.env.example](.env.example).
Ne jamais commiter de secret.

## Intégration continue

GitHub Actions ([.github/workflows/ci.yml](.github/workflows/ci.yml)) compile et teste l'API et le front à chaque push sur `main` et sur chaque pull request.
Le déploiement (préproduction puis production) sera ajouté une fois l'hébergement choisi.
