import { ChangeDetectionStrategy, Component } from '@angular/core';
import { PageLegale } from './page-legale';

/**
 * Politique de confidentialité (RGPD, CLAUDE.md §13). Décrit ce que le site fait réellement ;
 * la durée de conservation et l'identité du responsable sont À FOURNIR / À DÉFINIR avec le gérant.
 */
@Component({
  selector: 'abs-confidentialite',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PageLegale],
  template: `
    <abs-page-legale titre="Politique de confidentialité">
      <h2>Responsable du traitement</h2>
      <p>ABS clean — <span class="a-fournir">À FOURNIR : raison sociale, adresse et e-mail de contact</span>.</p>

      <h2>Données collectées</h2>
      <p>Uniquement celles que vous saisissez dans le formulaire de devis :</p>
      <ul>
        <li>nom, prénom, téléphone, adresse e-mail ;</li>
        <li>adresse d'intervention, code postal, ville, informations d'accès (étage, digicode) ;</li>
        <li>créneaux préférés et commentaire, s'ils sont renseignés ;</li>
        <li>le détail de votre demande (nombre de vitres, baies vitrées et velux, profil, fréquence) ;</li>
        <li>le code de parrainage éventuellement saisi.</li>
      </ul>
      <p>
        Un code de parrainage personnel est créé pour chaque demande. Votre adresse IP est utilisée sur le moment pour limiter
        les envois abusifs ; elle n'est pas enregistrée avec votre demande.
      </p>

      <h2>Finalité et base légale</h2>
      <p>
        Ces informations servent <strong>uniquement à vous recontacter au sujet de votre devis</strong> : vous envoyer le devis en PDF,
        organiser l'intervention et, si vous avez été recommandé·e, gérer le programme de parrainage. Le traitement repose sur
        votre consentement, donné en cochant la case du formulaire, et sur les mesures préalables à un contrat que vous demandez.
        Aucune donnée n'est vendue ni utilisée à des fins publicitaires.
      </p>

      <h2>Destinataires</h2>
      <ul>
        <li>ABS clean, qui traite votre demande ;</li>
        <li>l'hébergeur du site et de la base de données : Render (serveurs à Francfort, Union européenne) ;</li>
        <li>le service d'envoi des e-mails et SMS : Brevo, qui envoie votre devis par e-mail et prévient ABS clean de votre demande ;</li>
        <li>si vous avez utilisé un code de parrainage : votre parrain est informé de votre prénom lorsque votre première intervention est réalisée.</li>
      </ul>

      <h2>Durée de conservation</h2>
      <p><span class="a-fournir">À DÉFINIR avec le gérant : durée de conservation des demandes (par exemple 3 ans après le dernier contact)</span></p>

      <h2>Vos droits</h2>
      <p>
        Vous pouvez à tout moment demander l'accès à vos données, leur rectification, leur effacement, la limitation du traitement,
        leur portabilité, vous opposer au traitement ou retirer votre consentement, en écrivant à
        <span class="a-fournir">À FOURNIR : adresse e-mail de contact</span>.
        Si vous estimez que vos droits ne sont pas respectés, vous pouvez adresser une réclamation à la CNIL (cnil.fr).
      </p>

      <h2>Cookies</h2>
      <p>
        Ce site n'utilise <strong>aucun cookie</strong> ni outil de mesure d'audience, et ne charge aucune ressource chez un tiers
        (les polices de caractères sont hébergées avec le site).
      </p>

      <h2>Sécurité</h2>
      <p>Le site est servi uniquement en HTTPS. Les données sont stockées dans une base de données hébergée dans l'Union européenne.</p>
    </abs-page-legale>
  `,
})
export class Confidentialite {}
