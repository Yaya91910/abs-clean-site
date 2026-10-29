# ABS clean — Cahier des charges du site de devis

> Ce fichier est lu automatiquement par Claude Code à la racine du dépôt.
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

## 2. Règles de travail pour Claude Code

- **Tester avant d'affirmer.** Ne jamais déclarer une fonctionnalité terminée sans l'avoir exécutée (tests automatiques + vérification manuelle décrite).
- **Aucun secret dans le code ni dans Git.** Clés d'API, mots de passe, numéros : uniquement en variables d'environnement. Fournir un `.env.example` sans valeurs réelles.
- **Ne rien inventer** sur l'entreprise (assurance, produits, délais, moyens de paiement, avis clients). Utiliser uniquement ce document.
- **Demander avant** toute action payante ou irréversible (achat de domaine, envoi de SMS réels en masse, suppression de données).
- **Petits commits** clairs, une fonctionnalité à la fois. Branche `main` toujours déployable.
- **Fidélité visuelle** : la maquette `maquette/maquette-reference.dc.html` est la référence (couleurs, espacements, composants, textes).

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
├── CLAUDE.md
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

Ordre de la page (textes exacts dans la maquette) :

1. **En-tête collant** : logo, liens (À propos, Prestations, Tarifs, FAQ), bouton « Devis gratuit » avec badge « 2 MIN ». Sur mobile < 640 px : logo + bouton uniquement.
2. **Héros** : badge « Essonne · Île-de-France · Devis instantané », titre « La transparence, notre métier. », boutons « Obtenir mon devis gratuit » et « Découvrir nos prestations », photo, pastilles de confiance.
3. **À propos** : texte de présentation + Ponctualité, Disponibilité, Discrétion.
4. **Avant / Après** : visuel comparatif (photos réelles `À FOURNIR`).
5. **Prestations** : Particuliers, Commerces, Professionnels, Option (encadrements, rails, rebords).
6. **Tarifs** : 4 cartes (voir §7) + bandeau « Formules régulières ».
7. **Comment ça marche** : 3 étapes.
8. **FAQ** : accordéon, 8 questions, une seule ouverte à la fois, `aria-expanded`.
9. **Devis** : formulaire + estimation en direct (voir §7 et §8).
10. **Bandeau final** « Prêt pour des vitres impeccables ? ».
10 bis. **Parrainage** : offre −10 %, 3 étapes, boutons Copier le lien / WhatsApp / SMS / E-mail, carte avec code personnel et QR code (voir §9 bis).
11. **Réseaux sociaux** : TikTok, Instagram (liens `À FOURNIR`, ouverture dans un nouvel onglet, `rel="noopener noreferrer"`).
12. **Pied de page** : logo, zone, liens, contact (`À FOURNIR`), mentions légales, confidentialité.

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
- À chaque demande enregistrée, générer un **code unique** lisible : `ABS-` + prénom en majuscules sans accents (6 lettres max) + 1 à 2 caractères aléatoires, sans caractères ambigus (0/O, 1/I). Garantir l'unicité en base.
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
