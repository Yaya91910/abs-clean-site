import { ChangeDetectionStrategy, Component, DOCUMENT, ElementRef, inject, signal, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule } from '@angular/forms';
import { ChampAvecErreur, CHAMPS_TEXTE, CRENEAUX, creerFormulaireCoordonnees, messageErreur } from '../core/coordonnees';
import { Creneau, DemandeAcceptee, DemandesApi, ResultatEnvoi } from '../core/demandes-api';
import { ParrainageApi } from '../core/parrainage-api';
import { DevisStore } from '../core/devis-store';
import { Fleche } from '../shared/fleche';

/**
 * Fenêtre de coordonnées (CAHIER-DES-CHARGES.md §8) : <dialog> natif ouvert en modal,
 * ce qui rend le reste de la page inerte (focus piégé) et gère la touche Échap.
 */
@Component({
  selector: 'abs-devis-modal',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, Fleche],
  template: `
    <dialog #dialog class="modal-card" aria-labelledby="modal-title" (close)="apresFermeture()">
      <button type="button" class="modal-close" aria-label="Fermer" (click)="fermer()">×</button>

      @if (etat() !== 'envoyee') {
        <form class="modal-flow" [formGroup]="form" (ngSubmit)="envoyer()" novalidate>
          <div class="modal-head">
            <span class="kicker">Dernière étape</span>
            <h3 id="modal-title">Vos coordonnées</h3>
            <p>Estimation : <strong>{{ store.libelleEstimation() }}</strong>. Vous recevrez une copie du devis en PDF par e-mail.</p>
          </div>

          <div class="modal-scroll">
            <div class="modal-grid">
              @for (c of champsTexte; track c.nom) {
                <div class="mf" [class.full]="c.pleineLargeur">
                  <label [for]="'c-' + c.nom">{{ c.libelle }}</label>
                  @if (c.nom === 'commentaire') {
                    <textarea [id]="'c-' + c.nom" class="field left" rows="3" [formControlName]="c.nom" [placeholder]="c.exemple"
                      [attr.aria-invalid]="erreur(c.nom) ? true : null" [attr.aria-describedby]="erreur(c.nom) ? 'e-' + c.nom : null"></textarea>
                  } @else {
                    <input [id]="'c-' + c.nom" class="field left" [type]="c.type" [formControlName]="c.nom" [placeholder]="c.exemple"
                      [attr.autocomplete]="c.autocomplete" [attr.inputmode]="c.inputmode"
                      [attr.aria-invalid]="erreur(c.nom) ? true : null" [attr.aria-describedby]="erreur(c.nom) ? 'e-' + c.nom : null" />
                  }
                  @if (erreur(c.nom); as message) {
                    <span class="field-error" [id]="'e-' + c.nom">{{ message }}</span>
                  }
                </div>

                @if (c.nom === 'acces') {
                  <div class="mf full" role="group" aria-labelledby="creneaux-titre">
                    <span class="mf-label" id="creneaux-titre">Créneaux préférés</span>
                    <div class="creneaux">
                      @for (cr of creneaux; track cr.id) {
                        <button type="button" class="pill-btn petit" [attr.aria-pressed]="creneauChoisi(cr.id)" (click)="basculerCreneau(cr.id)">{{ cr.libelle }}</button>
                      }
                    </div>
                  </div>
                }
              }

              <!-- Champ piège (honeypot) : invisible pour un humain, rempli par les robots. -->
              <div class="visually-hidden" aria-hidden="true">
                <label for="c-site-web">Site web (ne pas remplir)</label>
                <input id="c-site-web" type="text" formControlName="siteWeb" tabindex="-1" autocomplete="off" />
              </div>
            </div>

            <div class="consent-bloc">
              <label class="consent">
                <input type="checkbox" formControlName="consentement"
                  [attr.aria-invalid]="erreur('consentement') ? true : null" [attr.aria-describedby]="erreur('consentement') ? 'e-consentement' : null" />
                <span>J'accepte que ABS clean utilise ces informations uniquement pour me recontacter au sujet de ce devis.</span>
              </label>
              @if (erreur('consentement'); as message) {
                <span class="field-error" id="e-consentement">{{ message }}</span>
              }
              <a class="lien-confidentialite" href="/confidentialite" target="_blank" rel="noopener">Politique de confidentialité (nouvel onglet)</a>
            </div>
          </div>

          <div class="modal-actions">
            @if (erreurGlobale(); as message) {
              <p class="field-error globale" role="alert">{{ message }}</p>
            }
            <button type="button" class="btn-cancel" (click)="fermer()">Annuler</button>
            <button type="submit" class="btn-devis calc envoyer" [disabled]="etat() === 'envoi'">
              <span class="label">{{ etat() === 'envoi' ? 'Envoi…' : 'Envoyer ma demande' }}</span><span class="arrow"><abs-fleche [taille]="22" /></span>
            </button>
          </div>
        </form>
      } @else {
        <div class="succes" role="status">
          <div class="succes-ico" aria-hidden="true">
            <svg width="34" height="34" viewBox="0 0 24 24" fill="none"><path d="M5 13l4 4L19 7" stroke="#FFFFFF" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round" /></svg>
          </div>
          <h3 id="modal-title">Demande envoyée !</h3>
          <p>Votre devis en PDF vient de partir dans votre boîte mail. ABS clean vous recontacte rapidement pour fixer l'intervention.</p>
          @if (acceptee(); as a) {
            <div class="ref-success">
              <div class="qr-box small"><img [src]="parrainageApi.qrSvg(a.codeParrain)" alt="QR code de votre lien de parrainage" width="80" height="80" /></div>
              <div class="ref-success-txt">
                <span class="ref-label">Votre code parrain</span>
                <span class="ref-code">{{ a.codeParrain }}</span>
                <span class="ref-offre">−10&nbsp;% sur votre prochaine intervention pour chaque proche qui devient client.</span>
              </div>
            </div>
            @if (a.codeParrainageSaisi === 'ignore') {
              <p class="note-code">Le code de parrainage saisi n'a pas pu être pris en compte.</p>
            }
          }
          <button type="button" class="btn-cancel" (click)="fermer()">Fermer</button>
        </div>
      }
    </dialog>
  `,
  styles: `
    .modal-card {
      width: calc(100% - 32px); max-width: 600px; max-height: none; margin: auto; padding: 28px; border: none; border-radius: 26px;
      background: #FFFFFF; color: var(--ink); box-shadow: 0 40px 80px -30px rgba(21, 52, 95, 0.55); overflow: visible;
    }
    .modal-card[open] { display: flex; flex-direction: column; align-items: center; animation: pop-in .25s ease; }
    .modal-card::backdrop { background: rgba(11, 29, 56, 0.42); backdrop-filter: blur(14px) saturate(120%); -webkit-backdrop-filter: blur(14px) saturate(120%); animation: fade-in .25s ease; }
    @keyframes fade-in { from { opacity: 0; } to { opacity: 1; } }
    @keyframes pop-in { from { opacity: 0; transform: translateY(14px) scale(0.98); } to { opacity: 1; transform: none; } }

    .modal-close { position: absolute; top: 14px; right: 14px; width: 38px; height: 38px; border-radius: 999px; border: none; background: #F0F4F9; color: var(--ink); font-size: 22px; line-height: 1; cursor: pointer; z-index: 1; }
    .modal-close:hover { background: #E2EAF4; }

    /* Titre et boutons toujours visibles : seuls les champs défilent. */
    .modal-flow { width: 100%; display: flex; flex-direction: column; align-items: center; gap: 18px; max-height: calc(100vh - 88px); max-height: calc(100dvh - 88px); }
    .modal-head { display: flex; flex-direction: column; align-items: center; gap: 6px; text-align: center; flex-shrink: 0; }
    .modal-head h3 { font-size: clamp(22px, 4vw, 28px); margin-top: 6px; }
    .modal-head p { font-size: 14px; color: var(--text-muted); margin: 0; max-width: 380px; line-height: 1.5; }
    .modal-head strong { color: var(--accent); }
    .modal-scroll {
      width: 100%; flex: 1 1 auto; min-height: 0; overflow-y: auto; display: flex; flex-direction: column; gap: 16px; padding: 4px 6px 28px 2px; scrollbar-width: thin;
      -webkit-mask-image: linear-gradient(to bottom, #000 calc(100% - 36px), transparent); mask-image: linear-gradient(to bottom, #000 calc(100% - 36px), transparent);
    }
    .modal-grid { width: 100%; display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; text-align: left; }
    .full { grid-column: 1 / -1; }
    .mf { display: flex; flex-direction: column; gap: 6px; min-width: 0; }
    .mf label, .mf-label { font-size: 13px; font-weight: 600; color: var(--ink); }
    textarea.field { padding: 12px 14px; min-height: 84px; resize: vertical; }
    .creneaux { display: flex; flex-wrap: wrap; gap: 8px; }
    .pill-btn.petit { min-height: 38px; padding: 0 16px; font-size: 13px; }
    .consent-bloc { display: flex; flex-direction: column; gap: 6px; text-align: left; }
    .consent { display: flex; align-items: flex-start; gap: 10px; width: 100%; font-size: 13px; color: var(--text-muted); line-height: 1.5; cursor: pointer; }
    .lien-confidentialite { font-size: 12px; padding-left: 28px; }
    .consent input { width: 18px; height: 18px; margin-top: 1px; accent-color: var(--accent); flex-shrink: 0; }

    .modal-actions { display: flex; flex-wrap: wrap-reverse; justify-content: center; gap: 12px; width: 100%; flex-shrink: 0; }
    .envoyer { flex: 1 1 240px; max-width: 320px; justify-content: space-between; }
    .globale { width: 100%; text-align: center; margin: 0; order: 1; }

    .succes { display: flex; flex-direction: column; align-items: center; gap: 14px; text-align: center; padding: 20px 4px; }
    .succes-ico { width: 72px; height: 72px; border-radius: 999px; background: linear-gradient(135deg, #3FD08F, var(--success)); display: flex; align-items: center; justify-content: center; box-shadow: 0 14px 30px -12px rgba(34, 181, 115, 0.7); }
    .succes h3 { font-size: 24px; }
    .succes p { font-size: 15px; color: var(--text-muted); margin: 0; max-width: 360px; line-height: 1.6; }
    .ref-success { display: flex; align-items: center; gap: 14px; width: 100%; padding: 14px; border-radius: 18px; background: rgba(47, 111, 214, 0.06); border: 1px solid rgba(47, 111, 214, 0.15); text-align: left; }
    .qr-box.small { width: 92px; height: 92px; padding: 6px; border-radius: 14px; flex-shrink: 0; box-shadow: 0 8px 18px -10px rgba(21, 52, 95, 0.4); border: 1px solid #E3E8F0; }
    .ref-success-txt { display: flex; flex-direction: column; gap: 4px; min-width: 0; }
    .ref-label { font-size: 12px; font-weight: 700; color: var(--accent-texte); text-transform: uppercase; letter-spacing: 0.08em; }
    .ref-code { font-size: 20px; font-weight: 800; color: var(--ink); letter-spacing: 0.04em; overflow-wrap: anywhere; }
    .ref-offre { font-size: 13px; color: var(--text-muted); line-height: 1.45; }
    .succes .note-code { font-size: 13px; }

    @media (max-width: 520px) {
      .modal-card { padding: 20px 16px; border-radius: 22px; width: calc(100% - 24px); }
      .modal-flow { max-height: calc(100vh - 72px); max-height: calc(100dvh - 72px); gap: 14px; }
    }
    @media (max-width: 420px) {
      .envoyer { flex: 1 1 100%; max-width: 100%; min-height: 56px; padding-left: 20px; }
      .envoyer .label { font-size: 16px; }
    }
    @media (max-width: 360px) { .modal-grid { grid-template-columns: 1fr; } }
  `,
})
export class DevisModal {
  protected readonly store = inject(DevisStore);
  protected readonly parrainageApi = inject(ParrainageApi);
  private readonly api = inject(DemandesApi);
  private readonly document = inject(DOCUMENT);
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  protected readonly etat = signal<'saisie' | 'envoi' | 'envoyee'>('saisie');
  protected readonly erreurGlobale = signal<string | null>(null);
  protected readonly acceptee = signal<DemandeAcceptee | null>(null);
  /** Vrai après une tentative d'envoi : toutes les erreurs s'affichent, même sur les champs non visités. */
  private readonly tentative = signal(false);

  protected readonly creneaux = CRENEAUX;
  protected readonly champsTexte = CHAMPS_TEXTE;
  protected readonly form = creerFormulaireCoordonnees();
  /** Événements du formulaire (valeur, validité, champs visités, erreurs serveur) : la vue OnPush suit ce signal. */
  private readonly evenementsForm = toSignal(this.form.events);

  ouvrir(): void {
    this.preremplirLocalisation();
    this.etat.set('saisie');
    this.erreurGlobale.set(null);
    const dialog = this.dialog().nativeElement;
    if (typeof dialog.showModal === 'function') dialog.showModal();
    else dialog.setAttribute('open', '');
    this.document.documentElement.classList.add('modal-ouverte');
    this.focaliser('c-nom');
  }

  protected fermer(): void {
    const dialog = this.dialog().nativeElement;
    if (typeof dialog.close === 'function') dialog.close();
    else dialog.removeAttribute('open');
    this.apresFermeture();
  }

  /** Appelé aussi par l'événement « close » natif (touche Échap). */
  protected apresFermeture(): void {
    this.document.documentElement.classList.remove('modal-ouverte');
    if (this.etat() === 'envoyee') {
      this.form.reset();
      this.tentative.set(false);
      this.etat.set('saisie');
    }
  }

  protected erreur(champ: ChampAvecErreur): string | null {
    this.evenementsForm();
    const control = this.form.controls[champ];
    if (!control.errors || !(control.touched || this.tentative())) return null;
    return messageErreur(champ, control.errors);
  }

  protected creneauChoisi(id: Creneau): boolean {
    this.evenementsForm();
    return this.form.controls.creneaux.value.includes(id);
  }

  protected basculerCreneau(id: Creneau): void {
    const actuels = this.form.controls.creneaux.value;
    this.form.controls.creneaux.setValue(actuels.includes(id) ? actuels.filter((c) => c !== id) : [...actuels, id]);
  }

  protected envoyer(): void {
    if (this.etat() === 'envoi') return;
    this.tentative.set(true);
    this.erreurGlobale.set(null);
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      this.focaliserPremiereErreur();
      return;
    }

    const v = this.form.getRawValue();
    const q = this.store.quantites();
    this.etat.set('envoi');
    this.api
      .envoyer({
        profil: this.store.profil(),
        frequence: this.store.frequence(),
        nbVitres: q.vitre,
        nbBaies: q.baie,
        nbVelux: q.velux,
        ...v,
        codeParrainage: this.store.codeParrain(),
      })
      .subscribe((resultat) => this.traiter(resultat));
  }

  private traiter(resultat: ResultatEnvoi): void {
    switch (resultat.statut) {
      case 'ok':
        this.acceptee.set(resultat);
        this.etat.set('envoyee');
        this.focaliser('modal-title');
        return;
      case 'invalide':
        this.etat.set('saisie');
        for (const [champ, messages] of Object.entries(resultat.erreurs)) {
          const control = this.form.get(champ);
          if (control) control.setErrors({ serveur: messages[0] });
          else this.erreurGlobale.set(messages[0]);
        }
        this.focaliserPremiereErreur();
        return;
      case 'trop-de-demandes':
        this.etat.set('saisie');
        this.erreurGlobale.set('Trop de demandes envoyées depuis votre connexion. Réessayez dans quelques minutes.');
        return;
      default:
        this.etat.set('saisie');
        this.erreurGlobale.set("L'envoi n'a pas abouti. Vérifiez votre connexion et réessayez dans un instant.");
    }
  }

  private preremplirLocalisation(): void {
    const localisation = this.store.localisation().trim();
    if (!localisation) return;
    const { codePostal, ville } = this.form.controls;
    if (/^\d{5}$/.test(localisation)) {
      if (!codePostal.value) codePostal.setValue(localisation);
    } else if (!ville.value) {
      ville.setValue(localisation);
    }
  }

  private focaliserPremiereErreur(): void {
    const premier = Object.keys(this.form.controls).find((c) => this.form.get(c)?.invalid);
    if (premier === 'consentement') this.dialog().nativeElement.querySelector<HTMLElement>('.consent input')?.focus();
    else if (premier) this.focaliser('c-' + premier);
  }

  private focaliser(id: string): void {
    setTimeout(() => {
      const el = this.dialog().nativeElement.querySelector<HTMLElement>('#' + id);
      if (el && !el.hasAttribute('tabindex') && !/^(INPUT|TEXTAREA|BUTTON)$/.test(el.tagName)) el.setAttribute('tabindex', '-1');
      el?.focus();
    });
  }
}
