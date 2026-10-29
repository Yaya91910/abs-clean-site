# Déploiement — préproduction puis production (Render)

L'image Docker unique (`Dockerfile` à la racine) contient l'API et le site : l'API sert le site Angular,
tout est sur le même domaine (pas de CORS, liens et QR codes de parrainage cohérents).

## 1. Préproduction (offre gratuite)

> Offre gratuite Render : le site se met en veille après 15 min sans visite (environ 1 min pour se réveiller),
> 750 h d'exécution par mois, et la base PostgreSQL gratuite **expire 30 jours après sa création** puis est
> **supprimée 14 jours plus tard**, sans sauvegarde. Suffisant pour tester jusqu'au 12 novembre ; à ne pas utiliser en production.
> Vérifier ces conditions sur render.com au moment de la création.

1. Créer un compte sur [render.com](https://render.com) (connexion avec GitHub) et autoriser l'accès au dépôt `abs-clean-site`.
2. **New → Blueprint**, choisir le dépôt : Render lit `render.yaml` et propose de créer
   - le service web `abs-clean-preprod` (Docker, Francfort, gratuit),
   - la base `abs-clean-db-preprod` (PostgreSQL 17, Francfort, gratuite).
3. Render demande les variables marquées `sync: false` :
   - `Site__Url` : l'adresse du service, `https://abs-clean-preprod.onrender.com` (à corriger après création si Render a choisi un autre nom) ;
   - Brevo (`Email__*`, `Sms__*`) et coordonnées (`Entreprise__*`) : **laisser vide** tant que le gérant ne les a pas fournies → envois en mode test, rien ne part.
4. Valider. Le premier déploiement construit l'image (quelques minutes), applique les migrations (`Database__MigrerAuDemarrage=true`) puis démarre.
5. Vérifier : `https://…onrender.com/api/health` répond `{"status":"ok"}`, puis faire un devis complet sur le site.

Ensuite, **chaque fusion dans `main` est déployée automatiquement, seulement si la CI GitHub est verte** (`autoDeployTrigger: checksPass`).

### Envois réels en préproduction

Une fois le compte Brevo créé par le gérant (e-mail transactionnel + crédits SMS, expéditeur vérifié), renseigner dans
Render → service → *Environment* : `Email__ApiKey`, `Email__From`, `Email__GerantTo`, `Sms__ApiKey`, `Sms__Sender`
(11 caractères alphanumériques max), `Sms__GerantNumero`. Le service redémarre : les envois partent pour de vrai.
Tant qu'une clé manque, les journaux du service affichent « Mode test des envois ».

## 2. Production (validation manuelle)

À faire avec l'accord du gérant (offres payantes, nom de domaine) :

1. Dans Render, créer un service web Docker `abs-clean-prod` et une base PostgreSQL **payante** (sauvegardes), région Francfort,
   avec les mêmes variables que la préproduction (valeurs de production), et **désactiver l'auto-déploiement**.
2. Ajouter le nom de domaine (Render → service → *Custom Domains*) ; le certificat HTTPS est automatique. Mettre `Site__Url` à jour.
3. Copier l'URL de *Deploy Hook* du service de production (Settings) dans GitHub → Settings → Secrets → Actions :
   `RENDER_DEPLOY_HOOK_PRODUCTION`.
4. GitHub → Settings → Environments → `production` : ajouter le gérant (ou toi) comme *Required reviewer*.
5. Pour livrer : GitHub → Actions → **Déploiement production** → *Run workflow*, puis approuver.

## Paramètres de l'image

| Variable | Rôle |
|---|---|
| `ConnectionStrings__Default` | Base PostgreSQL ; accepte aussi l'URL `postgresql://…` fournie par Render |
| `Database__MigrerAuDemarrage` | `true` : applique les migrations au démarrage |
| `Proxy__EntetesTransferees` | `true` derrière Render : IP réelle du client (limitation de débit) et HTTPS d'origine |
| `PORT` / `ASPNETCORE_HTTP_PORTS` | Port d'écoute (8080) |
| `Site__Url` | Adresse publique : liens de parrainage, QR codes, lien de validation du gérant |
| `Email__*`, `Sms__*`, `Entreprise__*` | Brevo et coordonnées (voir `.env.example`) |
