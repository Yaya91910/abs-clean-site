import { ChangeDetectionStrategy, Component } from '@angular/core';
import { PageLegale } from './page-legale';

/**
 * Conditions du parrainage (CAHIER-DES-CHARGES.md §9 bis) : règles par défaut du cahier des charges, À VALIDER par le gérant ;
 * les points non tranchés restent signalés.
 */
@Component({
  selector: 'abs-conditions-parrainage',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PageLegale],
  template: `
    <abs-page-legale titre="Conditions du parrainage">
      <p class="a-fournir">Conditions À VALIDER par ABS clean avant la mise en ligne.</p>

      <h2>Le principe</h2>
      <p>
        Chaque client reçoit, après sa demande de devis, un <strong>code de parrainage personnel</strong> (par exemple ABS-1765321),
        avec un lien et un QR code à partager.
      </p>

      <h2>La remise du parrain</h2>
      <ul>
        <li>Le parrain obtient <strong>−10 % sur sa prochaine intervention</strong> pour chaque proche recommandé qui devient client.</li>
        <li>Un proche devient client lorsque <strong>sa première intervention est réalisée</strong> ; le simple envoi d'une demande de devis ne suffit pas.</li>
        <li>La remise est activée par ABS clean après cette première intervention ; le parrain en est informé par e-mail.</li>
        <li>Cumul de plusieurs remises et plafond : <span class="a-fournir">À DÉFINIR</span></li>
        <li>Durée de validité d'une remise : <span class="a-fournir">À DÉFINIR</span></li>
      </ul>

      <h2>Le filleul</h2>
      <ul>
        <li>Le filleul saisit le code de son parrain dans le formulaire de devis, ou utilise son lien ou son QR code.</li>
        <li>Remise accordée au filleul : <span class="a-fournir">À DÉFINIR</span></li>
      </ul>

      <h2>Règles</h2>
      <ul>
        <li>On ne peut pas se parrainer soi-même : un code saisi avec le même téléphone, le même e-mail ou la même adresse que son parrain n'est pas pris en compte.</li>
        <li>Un client ne peut avoir qu'un seul parrain.</li>
        <li>Un code inconnu est ignoré, sans empêcher l'envoi de la demande de devis.</li>
        <li>ABS clean peut refuser un parrainage en cas d'abus.</li>
      </ul>
    </abs-page-legale>
  `,
})
export class ConditionsParrainage {}
