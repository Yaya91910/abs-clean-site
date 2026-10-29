# Bilan de santé hebdomadaire

Instructions d'un bilan à faire chaque semaine (ou à la demande : « fais le bilan décrit dans docs/BILAN-HEBDO.md »).
Elles peuvent aussi servir de consigne à une tâche programmée (application Claude → tâches programmées, par exemple
chaque lundi à 9 h).

---

Tu fais le bilan de santé hebdomadaire du site ABS clean (site de devis de nettoyage de vitres). Rapport en français,
clair et court, pour le propriétaire du projet (pas forcément développeur).

**Règle absolue : bilan en lecture seule.** Ne fusionne, ne ferme, ne modifie, ne pousse et ne supprime rien
(ni PR, ni branche, ni fichier, ni réglage). Tu recommandes, l'utilisateur décide.

## Contexte

- Dépôt local : `~/code/abs-clean-site` (GitHub `Yaya91910/abs-clean-site`). Lire d'abord la section « 20. État d'avancement » de `CLAUDE.md`.
- Outils : GitHub CLI `~/.local/bin/gh` ; .NET 10 dans `~/.dotnet` ; Node 22 dans `~/.nvm/versions/node/v22.23.4/bin` (à mettre en tête du PATH : Node 16 est le défaut).
- Avant d'analyser : `git fetch --prune` (lecture seule) ; ne pas changer de branche si le dépôt a des modifications en cours.

## 1. Site en ligne

Chercher dans `CLAUDE.md` une ligne commençant par `Adresse en ligne :` suivie d'une adresse https (préproduction
et/ou production). S'il n'y en a pas : « Site pas encore en ligne », passer à l'étape 2. Sinon, pour chaque adresse :

- `GET /api/health` : attendu 200 et `"status":"ok"`. Alerter si `"envois"` indique `"test"` pour l'e-mail ou le SMS (rien n'est réellement envoyé) — alerte importante en production.
- `GET /` : 200, en-têtes `Content-Security-Policy`, `X-Frame-Options` et `Strict-Transport-Security` présents. `HEAD /` : 200.
- Certificat HTTPS : date d'expiration (alerte sous 21 jours).
- Temps de réponse de l'accueil (la préproduction gratuite peut mettre ~1 min à se réveiller : réessayer une fois avant de conclure à une panne).
- Lighthouse mobile sur l'accueil (`npx -y lighthouse@latest <adresse> --quiet --chrome-flags="--headless=new" --only-categories=performance,accessibility,best-practices,seo`) : objectif ≥ 90 partout. En cas d'erreur `NO_NAVSTART`, relancer une fois.

## 2. CI et pull requests

- Dernier passage du workflow « CI » sur `main` (`gh run list --workflow CI --branch main --limit 3`).
- PR ouvertes (`gh pr list --state open`), et pour chacune `gh pr checks <n>`. Une PR n'est prête que si les **4 jobs** sont au vert : API (.NET), Web (Angular), Image Docker, Parcours complet (Playwright).
- PR Dependabot : recommandation (fusionner / fermer / à regarder) selon les règles du projet — Node uniquement en LTS (jamais de version impaire) ; TypeScript suit la version exigée par Angular (`ng update`, pas seul) ; `@types/node` aligné sur Node 22. Pour un outil de test (xunit, Test.Sdk…), vérifier dans les journaux de la CI que les tests ont réellement tourné (« Passed: » proche de 122, pas 0).

## 3. Sécurité des dépendances

- `dotnet list AbsClean.slnx package --vulnerable --include-transitive`
- `npm audit` dans `web/` puis dans `e2e/`

## Format du rapport

- Une ligne de synthèse : 🟢 tout va bien / 🟠 à surveiller / 🔴 action nécessaire.
- Sections : Site en ligne · CI · PR en attente (recommandation pour chacune) · Vulnérabilités · Actions conseillées (courte liste, par ordre d'importance).
- Si une étape échoue (outil manquant, réseau), le dire clairement plutôt que deviner.
