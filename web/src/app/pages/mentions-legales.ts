import { ChangeDetectionStrategy, Component } from '@angular/core';
import { PageLegale } from './page-legale';

/**
 * Mentions légales (LCEN, art. 6). Les informations sur l'entreprise sont À FOURNIR par le gérant :
 * rien n'est inventé, chaque manque est signalé visiblement.
 */
@Component({
  selector: 'abs-mentions-legales',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PageLegale],
  template: `
    <abs-page-legale titre="Mentions légales">
      <h2>Éditeur du site</h2>
      <dl>
        <dt>Nom commercial</dt><dd>ABS clean</dd>
        <dt>Raison sociale et forme</dt><dd><span class="a-fournir">À FOURNIR : dénomination, forme juridique (EI, SASU…)</span></dd>
        <dt>Capital social</dt><dd><span class="a-fournir">À FOURNIR si société</span></dd>
        <dt>Adresse</dt><dd><span class="a-fournir">À FOURNIR : adresse du siège</span></dd>
        <dt>SIRET / RCS ou RNE</dt><dd><span class="a-fournir">À FOURNIR</span></dd>
        <dt>TVA intracommunautaire</dt><dd><span class="a-fournir">À FOURNIR, ou « TVA non applicable, art. 293 B du CGI »</span></dd>
        <dt>Téléphone</dt><dd><span class="a-fournir">À FOURNIR</span></dd>
        <dt>E-mail</dt><dd><span class="a-fournir">À FOURNIR</span></dd>
        <dt>Directeur de la publication</dt><dd><span class="a-fournir">À FOURNIR : nom du gérant</span></dd>
      </dl>

      <h2>Activité</h2>
      <p>Nettoyage et entretien des surfaces vitrées pour particuliers, professionnels et commerces, en Essonne et en Île-de-France, jusqu'à 2,50 m de hauteur.</p>

      <h2>Hébergement</h2>
      <dl>
        <dt>Hébergeur</dt><dd>Render Services, Inc. (render.com)</dd>
        <dt>Adresse</dt><dd><span class="a-fournir">À VÉRIFIER sur render.com au moment de la mise en ligne</span></dd>
        <dt>Lieu des serveurs</dt><dd>Francfort (Allemagne), Union européenne</dd>
      </dl>

      <h2>Propriété intellectuelle</h2>
      <p>
        Les textes, le logo et les éléments graphiques de ce site appartiennent à ABS clean, sauf mention contraire.
        Toute reproduction sans autorisation est interdite. Les photographies sont utilisées avec l'accord de leurs auteurs.
      </p>

      <h2>Données personnelles</h2>
      <p>Le traitement des informations envoyées par le formulaire de devis est décrit dans la <a href="/confidentialite">politique de confidentialité</a>.</p>
    </abs-page-legale>
  `,
})
export class MentionsLegales {}
