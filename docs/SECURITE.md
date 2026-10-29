# Sécurité

## Ce que fait le site

| Risque | Protection |
|---|---|
| Écoute, modification des échanges | HTTPS (certificat Render), HSTS 1 an : le navigateur refuse ensuite toute version non chiffrée |
| Secrets dans le code | Aucun : clés et numéros uniquement en variables d'environnement (`.env.example` sans valeurs) |
| Prix modifié dans le navigateur | Le total est toujours recalculé par l'API à partir des quantités |
| Robots, envois en masse | Champ piège (honeypot) ; 5 demandes / 10 min par IP ; 60 consultations du parrainage / min ; 300 requêtes / min par IP au total |
| Requêtes énormes | Corps limité à 64 Ko (413 au-delà) |
| Injection SQL | EF Core : requêtes paramétrées uniquement |
| Injection de code (XSS) | Angular échappe tout affichage ; saisies échappées dans les e-mails ; politique de contenu (CSP) : seuls les scripts du site s'exécutent |
| Retours à la ligne dans le nom ou la ville (objet d'e-mail, SMS) | Caractères de contrôle refusés dans les champs d'une ligne |
| Site affiché dans une page piège (clickjacking) | `frame-ancestors 'none'` et `X-Frame-Options: DENY` |
| Lien de validation du gérant | Jeton aléatoire de 256 bits, usage unique, seule son empreinte est stockée, placé après « # » (jamais dans les journaux du serveur) ; validation atomique (double clic sans effet) |
| Fuite de données par le parrainage | Les adresses /api/parrainage ne renvoient aucune donnée personnelle |
| Traçage des visiteurs | Aucun cookie, aucune ressource chez un tiers (police hébergée avec le site) |
| Serveur | Conteneur sans droits administrateur ; version du serveur non affichée |
| Dépendances vulnérables | Dependabot : mises à jour chaque semaine, vérifiées par la CI |

Dernier audit des dépendances (.NET, npm du site, npm des tests) : **aucune vulnérabilité connue** (29/10/2026).

## Ce qui dépend des comptes

À faire par le gérant et par toi, avant la mise en production :

- [ ] **Double authentification (2FA)** sur GitHub, Render, Brevo, le registrar du nom de domaine.
- [ ] **2FA sur la boîte mail du gérant** : le lien « Marquer l'intervention comme réalisée » y arrive ; qui lit cette boîte peut activer des remises.
- [ ] **Base PostgreSQL payante en production**, avec sauvegardes (la base gratuite de préproduction n'en a aucune).
- [ ] **Protéger la branche `main`** (GitHub → Settings → Branches) : fusion uniquement par PR avec la CI verte.
- [ ] **Alertes Dependabot** activées (GitHub → Settings → Code security).
- [ ] Ne jamais envoyer de clé d'API ou de mot de passe par e-mail ou dans un document : les saisir directement dans Render.
- [ ] Si une clé fuit : la régénérer immédiatement dans Brevo / Render (deploy hook) et mettre à jour la variable.

## Limites connues

- La limitation de débit est en mémoire : elle repart à zéro à chaque redémarrage et suppose une seule instance du service.
- La vraie IP des visiteurs derrière Render (`X-Forwarded-For`) est à vérifier une fois en ligne, sinon les limites par IP s'appliqueraient à tout le monde à la fois.
