# ABS clean — site de devis

Site vitrine et devis en ligne pour ABS clean (nettoyage de surfaces vitrées, Essonne et Île-de-France).
Le cahier des charges complet est dans [CAHIER-DES-CHARGES.md](CAHIER-DES-CHARGES.md) ; la référence visuelle dans [maquette/](maquette/).

| Dossier | Contenu |
|---|---|
| `api/` | API ASP.NET Core 10 (Minimal API) — `Domain/` calcul du devis, `Endpoints/`, `Infrastructure/`, `Tests/` (xUnit) |
| `web/` | Front Angular 22 — une section par composant dans `src/app/sections/` |
| `assets/` | Logo source (SVG / PNG) |
| `e2e/` | Tests de bout en bout Playwright |

## Prérequis

- [.NET SDK 10](https://dotnet.microsoft.com/download) (LTS)
- Node.js 22 LTS (ou 24) et npm

## Base de données

PostgreSQL en local comme en ligne. La chaîne de connexion de développement est dans `api/appsettings.Development.json`
(`Host=localhost;Database=abs_clean_dev`, utilisateur système) ; ailleurs, utiliser `ConnectionStrings__Default`.

```bash
createdb abs_clean_dev
```

```bash
dotnet tool restore && dotnet ef database update --project api
```

Après une modification du modèle (`api/Domain/Demande.cs`, `api/Infrastructure/AbsCleanDb.cs`) :

```bash
dotnet ef migrations add NomDeLaMigration --project api --output-dir Infrastructure/Migrations
```

Les tests de l'API n'ont pas besoin de PostgreSQL : ils utilisent SQLite en mémoire.

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

### Parcours complet (Playwright)

Les tests de bout en bout démarrent eux-mêmes une pile isolée : API sur le port 5288 avec sa propre base
`abs_clean_e2e` (créée et migrée au démarrage), site sur le port 4300. Ils couvrent le parcours de devis
complet à 1440, 820, 390 et 340 px (sans débordement horizontal), la FAQ, la fenêtre de coordonnées et le parrainage.

```bash
cd e2e && npm install && npx playwright install chromium
```

```bash
cd e2e && npx playwright test
```

Autre base : variable `E2E_CONNECTION_STRING`. En CI, une base PostgreSQL jetable est fournie par le job `e2e`.

## API

| Méthode | Route | État |
|---|---|---|
| GET | `/api/health` | ✅ |
| GET | `/api/tarifs` | ✅ |
| POST | `/api/devis/estimation` | ✅ |
| POST | `/api/demandes` | ✅ enregistrement, PDF, e-mails client et gérant, SMS gérant |
| GET | `/api/parrainage/{code}` et `/qr.svg` | à venir |
| POST | `/api/admin/demandes/{id}/intervention-realisee` | à venir |

Les erreurs sont renvoyées au format `ProblemDetails`. Le total est toujours recalculé côté serveur.

`POST /api/demandes` : validation champ par champ (§8), champ piège `siteWeb` (honeypot), limitation à
5 envois par IP et par fenêtre de 10 minutes (`RateLimit__Demandes__Limite`, `RateLimit__Demandes__FenetreMinutes`).
Derrière un proxy (Render, Azure…), configurer les en-têtes transférés au déploiement pour que la limite s'applique à l'IP réelle du client.

## Envois (PDF, e-mails, SMS)

Après chaque demande enregistrée, l'API génère le devis PDF (QuestPDF), envoie un e-mail au client et au gérant
(PDF joint) et un SMS au gérant, via Brevo. Chaque envoi est indépendant : un échec est journalisé et
enregistré (`EmailClientEnvoye`, `EmailGerantEnvoye`, `SmsEnvoye`) sans bloquer les autres.

**Mode test** : tant que `Email__ApiKey` / `Sms__ApiKey` sont vides, rien n'est envoyé ; les e-mails (HTML + PDF)
et les SMS sont écrits dans `api/boite-envoi/` (ignoré par Git). C'est le cas en développement.

QuestPDF est utilisé sous licence *Community*, gratuite pour les entreprises de moins d'un million de dollars
de chiffre d'affaires annuel.

## Configuration

`Database__MigrerAuDemarrage=true` applique les migrations EF Core au démarrage de l'API (préproduction, tests de bout en bout).

Toutes les clés et numéros passent par des variables d'environnement : voir [.env.example](.env.example).
Ne jamais commiter de secret.

## Intégration continue

GitHub Actions ([.github/workflows/ci.yml](.github/workflows/ci.yml)) compile et teste l'API et le front à chaque push sur `main` et sur chaque pull request.
Un job construit aussi l'image Docker de production.

## Déploiement

Image Docker unique (API + site) hébergée sur Render, région Francfort : préproduction déployée automatiquement
depuis `main` quand la CI est verte, production sur validation manuelle. Pas à pas : [docs/DEPLOIEMENT.md](docs/DEPLOIEMENT.md).
