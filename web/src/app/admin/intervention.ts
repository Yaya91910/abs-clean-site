import { ChangeDetectionStrategy, Component, DOCUMENT, inject, signal } from '@angular/core';
import { ParrainageApi, ResultatValidation } from '../core/parrainage-api';
import { Logo } from '../shared/logo';

/**
 * Page ouverte depuis le lien de l'e-mail du gérant : /admin/intervention?demande=ID&jeton=…
 * La validation exige un clic (POST) : un simple aperçu du lien par la messagerie ne déclenche rien.
 */
@Component({
  selector: 'abs-admin-intervention',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Logo],
  template: `
    <main class="page-admin">
      <div class="glass carte">
        <abs-logo [hauteur]="56" />
        <h1>Intervention réalisée</h1>
        @switch (etat()) {
          @case ('a-confirmer') {
            <p>Confirmez que la première intervention pour cette demande de devis a bien été réalisée.</p>
            <p class="aide">Si ce client a été parrainé, la remise de −10&nbsp;% de son parrain sera activée et il sera prévenu par e-mail.</p>
            <button type="button" class="btn-devis calc" (click)="confirmer()"><span class="label">Confirmer l'intervention</span></button>
          }
          @case ('envoi') {
            <p role="status">Enregistrement…</p>
          }
          @case ('validee') {
            <p class="ok" role="status">✓ C'est enregistré. Merci !</p>
          }
          @case ('deja-validee') {
            <p role="status">Cette intervention avait déjà été confirmée.</p>
          }
          @case ('lien-invalide') {
            <p role="alert">Ce lien n'est pas valable. Utilisez le lien reçu dans l'e-mail de la demande.</p>
          }
          @default {
            <p role="alert">L'enregistrement n'a pas abouti. Réessayez dans un instant.</p>
            <button type="button" class="btn-cancel" (click)="confirmer()">Réessayer</button>
          }
        }
        <a href="/">Retour au site</a>
      </div>
    </main>
  `,
  styles: `
    .page-admin { min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 24px 16px; background: linear-gradient(180deg, #F4F8FF 0%, #EAF2FF 100%); }
    .carte { width: 100%; max-width: 480px; padding: clamp(24px, 6vw, 40px); display: flex; flex-direction: column; align-items: center; gap: 16px; text-align: center; }
    h1 { font-size: 26px; }
    p { margin: 0; line-height: 1.6; color: var(--text-muted); }
    .aide { font-size: 13px; }
    .ok { color: #157A4E; font-weight: 600; }
    a { font-size: 14px; }
  `,
})
export class AdminIntervention {
  private readonly api = inject(ParrainageApi);
  private readonly parametres = new URLSearchParams(inject(DOCUMENT).location.search);
  private readonly demande = this.parametres.get('demande') ?? '';
  private readonly jeton = this.parametres.get('jeton') ?? '';

  protected readonly etat = signal<'a-confirmer' | 'envoi' | ResultatValidation>(this.demande && this.jeton ? 'a-confirmer' : 'lien-invalide');

  protected confirmer(): void {
    this.etat.set('envoi');
    this.api.validerIntervention(this.demande, this.jeton).subscribe((resultat) => this.etat.set(resultat));
  }
}
