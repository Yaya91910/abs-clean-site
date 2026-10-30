# ABS clean — Cahier des charges du site de devis

> Ce fichier est la référence du projet : à relire avant toute modification.
> Il décrit **quoi construire**, **comment** et **comment le vérifier**.
> Toute ligne marquée `À FOURNIR` doit être complétée par le gérant avant la mise en production — ne jamais inventer ces valeurs.

---

## 1. Contexte et objectif

ABS clean est une entreprise de nettoyage et d'entretien de surfaces vitrées, en **Essonne et Île-de-France**, pour particuliers, professionnels et commerces. Interventions **jusqu'à 2,50 m de hauteur**.

Le site doit :
1. présenter l'entreprise et donner envie de demander un devis ;
2. calculer une **estimation en direct** à partir du nombre de vitres, baies vitrées et velux ;
3. recueillir les coordonnées du client dans une **fenêtre modale** ;
4. générer un **devis PDF**, l'envoyer **par e-mail au client et au gérant**, et envoyer un **SMS au gérant** ;
5. enregistrer chaque demande (base pour un futur CRM) ;
6. gérer un **programme de parrainage** avec code personnel, lien et QR code partageables.

Échéance : **livraison le 12 novembre 2026**, développement du 2 au 12 novembre.

---

## 2. Règles de travail

- **Tester avant d'affirmer.** Ne jamais déclarer une fonctionnalité terminée sans l'avoir exécutée (tests automatiques + vérification manuelle décrite).
- **Aucun secret dans le code ni dans Git.** Clés d'API, mots de passe, numéros : uniquement en variables d'environnement. Fournir un `.env.example` sans valeurs réelles.
- **Ne rien inventer** sur l'entreprise (assurance, produits, délais, moyens de paiement, avis clients). Utiliser uniquement ce document.
- **Demander avant** toute action payante ou irréversible (achat de domaine, envoi de SMS réels en masse, suppression de données).
- **Petits commits** clairs, une fonctionnalité à la fois. Branche `main` toujours déployable.
- **Fidélité visuelle** : la maquette `maquette/maquette-reference.dc.html` est la référence pour les textes et les composants. L'ordre des sections et la palette ont été revus à la demande du gérant (voir §6 et §20 « Choix importants ») : ne pas revenir à la maquette sur ces points.

---

## 3. Stack technique

| Couche | Choix |
|---|---|
| Frontend | Angular (version LTS actuelle), composants *standalone*, `ChangeDetectionStrategy.OnPush`, Reactive Forms |
| Styles | CSS/SCSS natif avec variables CSS (pas de Bootstrap) |
| Backend | ASP.NET Core (version LTS actuelle), Minimal API |
| Données | EF Core + PostgreSQL en ligne (SQLite autorisé en local uniquement) |
| PDF | QuestPDF |
| E-mail | Service transactionnel (Brevo, Resend ou équivalent) — `À FOURNIR` par le gérant |
| SMS | Service SMS (Brevo, Twilio ou équivalent) — `À FOURNIR` par le gérant |
| Tests | xUnit (API), Playwright (parcours complet navigateur) |
| CI/CD | GitHub Actions |

Vérifier les versions et les conditions des offres gratuites au moment de l'installation.

---

## 4. Structure du dépôt

```
abs-clean-site/
├── CAHIER-DES-CHARGES.md
├── README.md                 # installation, lancement, déploiement
├── .env.example
├── maquette/maquette-reference.dc.html
├── assets/abs-clean-logo.svg
├── api/                      # ASP.NET Core
│   ├── Endpoints/
│   ├── Domain/               # calcul du devis, règles métier
│   ├── Infrastructure/       # EF Core, e-mail, SMS, PDF
│   └── Tests/
├── web/                      # Angular
│   └── src/app/sections/     # header, hero, a-propos, avant-apres, prestations,
│                             # tarifs, processus, faq, devis, cta, reseaux, footer
└── e2e/                      # Playwright
```

---

## 5. Charte graphique

Couleurs (extraites du logo) :

| Variable | Valeur | Usage |
|---|---|---|
| `--ink` | `#15345F` | titres, bandeaux sombres (navy du « b » et de « clean ») |
| `--accent` | `#2F6FD6` | boutons, prix, accents (bleu vif du « s ») |
| `--accent2` | `#4C9BE0` | halos, dégradés, touches secondaires (bleu clair du « a ») |
| texte secondaire | `#5B6B80` | paragraphes |
| succès | `#22B573` | point « en direct », confirmation |

- Police : **Inter** (300 à 800), titres en 700 avec `letter-spacing: -0.03em`.
- Logo : `assets/abs-clean-logo.svg` (vectoriel), en-tête et pied de page.
- Fond de page : dégradé très clair + trame de points + halos radiaux animés lentement (désactivés si `prefers-reduced-motion`).
- Cartes « verre » : fond blanc 72 % + `backdrop-filter: blur(14px)`, rayon 24 px.
- Boutons devis : pilule en dégradé `--accent` → `#1F55B5`, pastille-flèche blanche, reflet animé.
- **Leçon de la maquette :** les couleurs doivent passer par des variables CSS réellement appliquées — vérifier visuellement chaque composant (un fond manquant rend un texte blanc invisible).

---

## 6. Sections et contenus

Ordre de la page (textes exacts dans la maquette ; ordre revu pour la conversion, voir §20) :

1. **En-tête collant** : logo, liens (Prestations, Tarifs, À propos, FAQ, Parrainage), bouton « Devis gratuit » avec badge « 2 MIN ». Sur mobile < 720 px : logo + bouton uniquement.
2. **Héros** : badge « Essonne · Île-de-France · Devis instantané », titre « La transparence, notre métier. », boutons « Obtenir mon devis gratuit » et « Découvrir nos prestations », pastilles de confiance, photo (à droite sur ordinateur).
3. **Comment ça marche** : 3 étapes.
4. **Prestations** : Particuliers, Commerces, Professionnels, Option (encadrements, rails, rebords).
5. **Tarifs** : 4 cartes (voir §7) + bandeau « Formules régulières ».
6. **Devis** : formulaire + estimation en direct (voir §7 et §8) ; sur ordinateur, récapitulatif collé à droite.
7. **Avant / Après** : visuel comparatif (photos réelles `À FOURNIR`).
8. **À propos** : texte de présentation + Ponctualité, Disponibilité, Discrétion.
9. **FAQ** : accordéon, 8 questions, une seule ouverte à la fois, `aria-expanded`.
10. **Parrainage** : offre −10 %, 3 étapes, boutons Copier le lien / WhatsApp / SMS / E-mail, carte avec code personnel et QR code (voir §9 bis).
11. **Bandeau final** « Prêt pour des vitres impeccables ? ».
12. **Réseaux sociaux** : TikTok, Instagram (liens `À FOURNIR`, ouverture dans un nouvel onglet, `rel="noopener noreferrer"`).
13. **Pied de page** : logo, zone, liens, contact (`À FOURNIR`), mentions légales, confidentialité.

Photos : `À FOURNIR` (droits d'utilisation obligatoires — pas d'images de banque sans licence).

---

## 7. Règles de calcul du devis

Tarifs de base (TTC/HT : `À FOURNIR`) :

| Élément | Prix |
|---|---|
| Vitre | 20 € l'unité |
| Baie vitrée | à partir de 40 € |
| Velux | 25 € l'unité |
| Vitrine | à partir de 50 € (professionnels / commerces) |

- Estimation = `vitres × 20 + baies × 40 + velux × 25` (+ vitrines si ajoutées au formulaire).
- Affichage : « À partir de X € ». Mention : « Estimation confirmée après échange ».
- Fréquences : Ponctuel, Mensuel, Trimestriel. **Pourcentages de réduction : `À FOURNIR`** — tant qu'ils ne sont pas connus, aucune réduction n'est appliquée et l'interface l'indique.
- Profil Particulier / Professionnel : aucune différence de prix connue à ce jour (`À CONFIRMER`).
- Option encadrements, rails et rebords : « sur devis ».
- **Le calcul fait foi côté serveur.** Le prix affiché côté client est indicatif ; l'API recalcule à partir des quantités et ignore tout prix envoyé par le navigateur.
- Bornes : quantités entières de 0 à 99 ; au moins un élément > 0 pour valider.

---

## 8. Formulaire de coordonnées (fenêtre modale)

S'ouvre au clic sur « Valider mon devis ». Plein écran, **site flouté derrière**, carte **centrée sur tous les écrans** ; si l'écran est trop petit, seuls les champs défilent, le titre et le bouton d'envoi restent visibles. Fermeture : bouton ×, « Annuler », touche Échap. Focus piégé dans la fenêtre.

| Champ | Règle |
|---|---|
| Nom, Prénom | requis, 1–60 caractères |
| Téléphone | requis, format français (06/07/01–05/09, espaces tolérés) |
| E-mail | requis, format valide |
| Adresse d'intervention | requise |
| Code postal | requis, 5 chiffres |
| Ville | requise |
| Accès (étage, digicode) | facultatif |
| Créneaux préférés | multi-choix : Semaine · matin, Semaine · après-midi, Samedi |
| Commentaire | facultatif, 1000 caractères max |
| Code de parrainage | facultatif, normalisé en majuscules, vérifié côté serveur (code inconnu : ignoré, sans bloquer l'envoi) |
| Consentement RGPD | **case obligatoire**, texte : « J'accepte que ABS clean utilise ces informations uniquement pour me recontacter au sujet de ce devis. » |

Anti-abus : champ piège invisible (*honeypot*) + limitation de débit par IP sur l'endpoint d'envoi.

Écran de succès : affiche aussi **le code parrain personnel du client** et son QR code. « Demande envoyée ! Votre devis en PDF vient de partir dans votre boîte mail. ABS clean vous recontacte rapidement pour fixer l'intervention. »

---

## 9. Ce qui se passe après l'envoi

1. Validation serveur (mêmes règles que §8) → erreurs renvoyées champ par champ.
2. Recalcul du devis côté serveur.
3. Enregistrement de la demande (statut `nouvelle`).
4. Génération du **PDF** (QuestPDF) : logo, numéro de devis unique (ex. `ABS-2026-0001`), date, coordonnées client, détail des lignes, total « à partir de », mention d'estimation, coordonnées ABS clean (`À FOURNIR` : SIRET, adresse, téléphone, e-mail).
5. **E-mail au client** avec le PDF en pièce jointe.
6. **E-mail au gérant** avec le PDF et toutes les coordonnées.
7. **SMS au gérant** : `Nouveau devis ABS : {Prénom} {Nom}, {Ville}, à partir de {total} € — {téléphone}` (≤ 160 caractères).
8. Chaque envoi est indépendant : si le SMS échoue, les e-mails partent quand même ; l'échec est journalisé et la demande reste enregistrée.
9. Réponse au navigateur uniquement après l'enregistrement réussi.

---

## 9 bis. Programme de parrainage

**Offre (énoncée par le gérant) :** −10 % pour chaque nouveau client recommandé.

**Règles retenues par défaut — `À VALIDER` par le gérant avant la mise en ligne :**
- Le **parrain** obtient −10 % sur sa prochaine intervention pour chaque filleul devenu client.
- Un filleul compte comme « nouveau client » quand **sa première intervention est réalisée** (pas au simple envoi du devis).
- Le filleul bénéficie-t-il aussi d'une remise ? `À DÉFINIR`.
- Cumul des remises (ex. 3 filleuls = −30 %) et plafond : `À DÉFINIR`. Durée de validité d'une remise : `À DÉFINIR`.
- Les conditions complètes doivent figurer sur une page « Conditions du parrainage », liée depuis la section.

**Fonctionnement technique :**
- À chaque demande enregistrée, générer un **code unique** lisible : `ABS-` + 7 chiffres aléatoires, le premier non nul (ex. `ABS-1765321`). Garantir l'unicité en base. À la saisie, « 1765321 » ou « ABS1765321 » sont complétés en `ABS-1765321` ; les anciens codes (ex. `ABS-MARIE7`) restent valables.
- Lien de partage : `https://{domaine}/devis?parrain={CODE}`. À l'ouverture, le champ « Code de parrainage » est prérempli.
- **QR code** généré côté serveur (bibliothèque QRCoder en .NET), en SVG, niveau de correction M, intégré : à l'écran de succès, dans le **PDF du devis** et dans l'e-mail au client.
- Partage : copier le lien (Clipboard API), WhatsApp (`https://wa.me/?text=`), SMS (`sms:?&body=`), e-mail (`mailto:`), et `navigator.share` sur mobile si disponible.
- **Anti-fraude** : refuser l'auto-parrainage (même téléphone, e-mail ou adresse que le parrain) ; un client ne peut avoir qu'un seul parrain ; un code inconnu est ignoré sans bloquer la demande.
- **Validation par le gérant** : un filleul ne déclenche la remise qu'après confirmation de son intervention. Prévoir un lien sécurisé dans l'e-mail au gérant (« Marquer l'intervention comme réalisée ») ou une mini page d'administration protégée par mot de passe.
- Quand la remise est activée : e-mail au parrain « Votre proche {Prénom} est devenu client : −10 % sur votre prochaine intervention ».

**Données :**
- `Demande` : ajouter `CodeParrainPersonnel` (unique), `CodeParrainUtilise` (nullable), `ParrainId` (nullable).
- `Parrainage` : Id, ParrainId, FilleulId, CreeLe, Statut (`en_attente` / `valide` / `refuse`), ValideLe, RemiseUtiliseeLe.

**Tests supplémentaires :** génération et unicité des codes ; auto-parrainage refusé ; code inconnu ignoré ; lien `?parrain=` qui préremplit le champ ; QR code décodable renvoyant le bon lien ; remise activée uniquement après validation du gérant.

---

## 10. API

| Méthode | Route | Rôle |
|---|---|---|
| GET | `/api/tarifs` | tarifs et formules (source unique pour le front) |
| POST | `/api/devis/estimation` | quantités + profil + fréquence → total |
| POST | `/api/demandes` | demande complète → enregistrement, PDF, e-mails, SMS |
| GET | `/api/parrainage/{code}` | vérifie qu'un code existe (sans exposer de données personnelles) |
| GET | `/api/parrainage/{code}/qr.svg` | QR code du lien de parrainage |
| POST | `/api/admin/demandes/{id}/intervention-realisee` | validation par le gérant (lien sécurisé à usage unique ou authentification) |
| GET | `/api/health` | supervision |

CORS limité au domaine du site. Réponses d'erreur au format `ProblemDetails`.

---

## 11. Modèle de données

`Demande` : Id, Numero, CreeLe, Profil, Frequence, NbVitres, NbBaies, NbVelux, Total, Nom, Prenom, Telephone, Email, Adresse, CodePostal, Ville, Acces, Creneaux, Commentaire, ConsentementLe, Statut, EmailClientEnvoye, EmailGerantEnvoye, SmsEnvoye.

---

## 12. Variables d'environnement (`.env.example`)

```
ConnectionStrings__Default=
Email__ApiKey=
Email__From=
Email__GerantTo=
Sms__ApiKey=
Sms__Sender=
Sms__GerantNumero=
Cors__AllowedOrigin=
```

---

## 13. RGPD et obligations légales

- Pages **Mentions légales** et **Politique de confidentialité** (contenus `À FOURNIR` / à valider par le gérant).
- Données utilisées uniquement pour recontacter le client au sujet du devis.
- Durée de conservation : `À DÉFINIR` avec le gérant, affichée dans la politique de confidentialité.
- HTTPS obligatoire. Aucun cookie de suivi sans consentement.

---

## 14. Performance et accessibilité

- Lighthouse (mobile) : **≥ 90** en Performance, Accessibilité, Bonnes pratiques, SEO.
- Images en WebP/AVIF, dimensions fixées, chargement différé hors écran.
- Logo en SVG inline.
- Navigation clavier complète, contrastes AA, `aria-*` sur l'accordéon et la modale.
- Animations désactivées avec `prefers-reduced-motion`.
- Balises `title`, `meta description` et Open Graph (aperçu lors d'un partage).

---

## 15. Tests à écrire et à faire passer

**Automatiques**
- Unitaires : calcul du devis (0 élément refusé, combinaisons, bornes 0–99, prix envoyé par le client ignoré).
- Intégration API : validation de chaque champ, consentement manquant refusé, honeypot rempli rejeté.
- Playwright : parcours complet → compteurs, estimation, validation, modale, envoi, écran de succès ; à 1440, 820, 390 et 340 px de large ; aucun débordement horizontal.

**Manuels en préproduction**
- Un vrai devis de bout en bout : PDF reçu par le client, e-mail et SMS reçus par le gérant.
- Sur le téléphone du gérant (iPhone et Android) et sur ordinateur.
- Cas d'erreur : champs vides, mauvais numéro, consentement non coché, coupure du service SMS.

---

## 16. Déploiement

- **Préproduction** puis **production**, chacune avec ses variables d'environnement.
- GitHub Actions : build + tests à chaque push ; déploiement automatique de `main` en préproduction ; production sur validation manuelle.
- Hébergement envisagé : API sur Render ou Azure App Service, front sur Azure Static Web Apps, Netlify ou Vercel, base PostgreSQL hébergée. Vérifier les conditions au moment du choix.
- Nom de domaine + HTTPS : `À FOURNIR` (achat validé par le gérant).

---

## 17. Planning

| Dates | Livrable |
|---|---|
| 2 nov. | Dépôt, structure, CI, squelettes API et Angular |
| 3–4 nov. | Calcul du devis + tests, endpoints, base de données |
| 4–6 nov. | Sections Angular fidèles à la maquette, responsive |
| 7–8 nov. | PDF, e-mails, SMS en mode test, codes et QR de parrainage |
| 9–10 nov. | Préproduction en ligne, tests Playwright |
| 10–11 nov. | Tests réels sur appareils, corrections, pages légales |
| 12 nov. | Mise en production, validation avec le gérant |

---

## 18. Points ouverts (à fournir par le gérant)

- [ ] Pourcentages de réduction Mensuel / Trimestriel
- [ ] Tarifs HT ou TTC, statut TVA
- [ ] Téléphone, e-mail, SIRET, adresse pour le PDF et le pied de page
- [ ] Identifiants TikTok et Instagram
- [ ] Photos (héros, avant/après) avec droits d'utilisation
- [ ] Comptes e-mail transactionnel et SMS
- [ ] Nom de domaine
- [ ] Contenu des mentions légales et durée de conservation des données
- [ ] Règles du parrainage : remise du filleul, cumul, plafond, durée de validité, texte des conditions
- [ ] Validation du logo vectorisé (`assets/abs-clean-logo.svg`, redessiné d'après une photo)

---

## 19. Définition de « terminé »

Le site est terminé quand : tous les tests passent en CI ; un devis réel a été reçu (PDF client, e-mail et SMS gérant) en préproduction puis en production ; le rendu est validé sur iPhone, Android et ordinateur ; Lighthouse mobile ≥ 90 ; les pages légales sont en ligne ; et le gérant a validé le site.

---

## 20. État d'avancement (au 29 octobre 2026)

> Section à tenir à jour à chaque étape importante : c'est le point de départ de chaque nouvelle session de travail.

Adresse en ligne : aucune (site pas encore en ligne)

> Dès la mise en ligne, remplacer la ligne ci-dessus par une ligne par environnement, au format
> `Adresse en ligne : <adresse https> (préproduction)` puis `… (production)`.

Bilan de santé hebdomadaire : consignes dans `docs/BILAN-HEBDO.md` (« fais le bilan décrit dans docs/BILAN-HEBDO.md »). Il lit la ligne ci-dessus pour vérifier le site en ligne.

### Fait (fusionné dans `main`, CI verte)

| PR | Contenu |
|---|---|
| #1 | Squelettes API (.NET 10) et web (Angular 22), calcul du devis, CI |
| #2 | `POST /api/demandes` : PostgreSQL (EF Core), numéro `ABS-AAAA-NNNN`, validation §8, honeypot, limite par IP |
| #3 | Sections fidèles à la maquette, estimation en direct, fenêtre de coordonnées (`<dialog>`) |
| #4 | Devis PDF (QuestPDF), e-mails client et gérant, SMS gérant (Brevo ou mode test) |
| #5 | Parrainage : codes, QR (QRCoder), anti-fraude, lien de validation du gérant, page `/admin/intervention` |
| #6 | Tests Playwright (parcours complet à 1440 / 820 / 390 / 340 px) et job CI |
| #7 | Image Docker unique (l'API sert le site), `render.yaml` (Render, Francfort), déploiement production manuel |
| #8 | Pages légales (`/mentions-legales`, `/confidentialite`, `/parrainage/conditions`), police Inter hébergée |
| #9 | Accessibilité AA (contrastes, contrôle axe) — Lighthouse mobile 98–100 |
| #10 | Envois en arrière-plan + reprise au démarrage, alerte mode test, 4 bugs corrigés, refactoring |
| #11 | Sécurité : CSP et en-têtes, limite 64 Ko, caractères de contrôle, jeton du gérant après `#`, Dependabot |
| #15, #18, #19, #20 | Mises à jour Dependabot triées, règles d'exclusion, coverlet retiré |
| #21, #22 | Cette section d'état, bilan de santé hebdomadaire (`docs/BILAN-HEBDO.md`) |
| #23 | Fiche client : numéro `CLI-00001` sans doublon, deux PDF distincts (devis client / fiche d'intervention gérant), bandeau « Client existant », accueil « Bon retour » sur le site |

Tests : API 128 (xUnit), site 22 (Vitest), Playwright 23 (dont accessibilité axe). Lighthouse mobile : 100 / 100 / 100 / 100 sur la version de production.

### Reste à faire

1. **Informations du gérant** (§18) : liste à remplir envoyée sous forme de PDF (`docs/ABS-clean-informations.pdf`, **non versionné** : la version remplie contient ses coordonnées, ne jamais la committer). À intégrer à réception : prix et réductions (`api/Domain/Tarifs.cs` + réponse FAQ « Comment est calculé le prix ? » + tests), coordonnées (`Entreprise__*`, pied de page, mentions légales), réseaux (`web/src/app/sections/reseaux.ts`), règles du parrainage (page conditions), durée de conservation (confidentialité ; purge automatique à ajouter), photos (héros, avant/après, en WebP/AVIF).
2. **Préproduction** : compte Render + Blueprint (`docs/DEPLOIEMENT.md`). Puis vérifier en ligne : vraie IP derrière Render (`X-Forwarded-For`, sinon la limite par IP s'applique à tous), vrais e-mails et SMS (Brevo), aperçu de partage, iPhone (Safari) et Android.
3. **Production** : offres payantes (base avec sauvegardes) et nom de domaine, avec l'accord du gérant ; secret `RENDER_DEPLOY_HOOK_PRODUCTION`, environnement GitHub `production` avec relecteur.
4. **Comptes** : liste de `docs/SECURITE.md` (2FA partout, branche `main` protégée).
5. **Base locale** `abs_clean_dev` : migration `Clients` (#23) pas encore appliquée (sandbox) — `dotnet ef database update --project api`.
6. Piste à décider : **code parrain par client** plutôt que par demande (aujourd'hui un client fidèle reçoit un nouveau code à chaque devis).
7. Pistes non demandées : écran d'administration (liste des demandes, remise utilisée `RemiseUtiliseeLe`), purge des anciennes demandes, tiret « — » du SMS (2 SMS facturés en Unicode, « - » tiendrait en 1).

### Choix importants (ne pas défaire sans raison)

- **Le calcul fait foi côté serveur** ; le front n'envoie que des quantités.
- **Client** (`Client`, numéro `CLI-00001`) : reconnu par son e-mail (casse ignorée) **ou** son téléphone pour éviter les doublons ; mais l'accueil « Bon retour » et les coordonnées directes d'ABS clean ne sont donnés au navigateur que si e-mail **et** téléphone correspondent (sinon on révélerait si une adresse est cliente).
- **Code parrain `ABS-` + 7 chiffres** (ex. `ABS-1765321`, demandé par le gérant) : aucune donnée personnelle dans le code ; les anciens codes `ABS-PRENOM7` restent valables.
- **Deux exemplaires PDF** (`DevisPdf.DocumentClient` / `DocumentGerant`, données dans `FicheDevis`) ; les coordonnées d'ABS clean n'y apparaissent que si elles sont renseignées.
- **Une seule image Docker** : l'API sert le site (même domaine, pas de CORS). `PageIndex` remplace `__SITE_URL__` par `Site__Url` et pose la CSP, avec l'empreinte SHA-256 du script intégré par Angular calculée au démarrage.
- **Envois en arrière-plan** (`FileEnvois` + `EnvoisEnArrierePlan`), indépendants les uns des autres ; `Envois__EnArrierePlan=false` dans les tests.
- **Mode test des envois** sans clé Brevo : tout est écrit dans `api/boite-envoi/` (ignoré par Git).
- **Lien du gérant** : jeton de 256 bits après `#`, empreinte seule en base, validation atomique (`ExecuteUpdate`).
- **Front sans routeur** : `app.ts` choisit la page selon l'adresse ; passer au routeur Angular si les pages se multiplient.
- **Node 22 LTS** partout (CI, image, tests) ; TypeScript suit Angular (`ng update`) : Dependabot ignore leurs versions majeures.
- **Page réorganisée et palette adoucie** (demande du gérant) : promesse → 3 étapes → prestations → tarifs → devis, la réassurance (avant/après, à propos, FAQ) vient après ; bleu `--accent` `#3D6CB5` (au lieu de `#2F6FD6`), bandeaux clairs `--brume` au lieu du bleu nuit, reflets et halos plus discrets. Vérifier le contraste AA (axe) de tout petit texte posé sur `--brume`.
- **Deux gris et le vert du badge assombris** par rapport à la maquette, pour le contraste AA (§14).

### Lancer et tester (sur ce Mac)

Outils installés en espace utilisateur : .NET 10 dans `~/.dotnet`, Node 22 via nvm (Node 16 reste le défaut du shell), GitHub CLI dans `~/.local/bin/gh`, PostgreSQL 14 local (base `abs_clean_dev`). Docker n'est pas installé : l'image est construite par la CI.

```bash
~/.dotnet/dotnet run --project api --launch-profile http
```

```bash
cd web && PATH="$HOME/.nvm/versions/node/v22.23.4/bin:$PATH" npx ng serve
```

Site sur http://localhost:4200 (API sur 5188 via le proxy). Tests : `dotnet test` (vérifier le **code de sortie**, pas seulement la ligne « Réussi »), `npx ng test --watch=false` dans `web/`, `npx playwright test` dans `e2e/` (pile isolée : API 5288, base `abs_clean_e2e`, site 4300).

### Pièges connus

- `ng serve` peut rester bloqué sur une ancienne version après un changement de branche : le relancer.
- Les tests d'intégration utilisent un fichier SQLite temporaire par fabrique (une connexion par opération) : une connexion SQLite partagée fait échouer au hasard les tests où l'arrière-plan et le test lisent la base en même temps.
- La CSP n'existe que sur la version servie par l'API (pas sur `ng serve`) : la vérifier sur une version de production (API publiée + site dans `wwwroot`).
- Avant de fusionner une PR : les **4 jobs** de la CI au vert (API, Web, Image Docker, Playwright) ; pour une mise à jour d'outil de test, vérifier dans les journaux que les 122 tests ont bien tourné.
- `api/appsettings.Development.json` contient une adresse et un numéro de gérant **fictifs** pour le mode test : ne jamais y mettre de vraies valeurs.
- `dotnet ef` utilise `AbsCleanDbConception` (fabrique de conception) : il ne démarre pas l'API ; la variable `ConnectionStrings__Default` prime pour appliquer une migration.
- **Sandbox strict** (ports locaux, PostgreSQL, certificats et réseau bloqués) : compiler avec `dotnet build -m:1 -nodeReuse:false -p:UseSharedCompilation=false` ; `dotnet test`, `ng build`, `gh` et la base locale ne marchent pas → laisser la CI tester, ouvrir la PR depuis le lien GitHub, fusionner avec Git (`git merge --no-ff` puis `git push origin main`) une fois les 4 jobs verts. Aperçu des PDF : petit programme hors dépôt appelant `DevisPdf` (images via `GenerateImages`), paquets restaurés avec `--source ~/.nuget/packages`.
