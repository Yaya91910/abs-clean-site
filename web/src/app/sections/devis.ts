import { ChangeDetectionStrategy, Component, inject, signal, viewChild } from '@angular/core';
import { CodeQuantite, DevisStore, FREQUENCES, PROFILS } from '../core/devis-store';
import { Fleche } from '../shared/fleche';
import { DevisModal } from './devis-modal';

@Component({
  selector: 'abs-devis',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Fleche, DevisModal],
  template: `
    <section id="devis" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Devis</span>
          <h2 class="title">Votre devis en 2 minutes</h2>
          <p class="sub">Choisissez vos options, votre estimation s'affiche aussitôt.</p>
        </div>

        <div class="glass formulaire">
          <div class="groupe" role="group" aria-labelledby="devis-profil">
            <span id="devis-profil" class="field-label">Vous êtes</span>
            <div class="pills">
              @for (p of profils; track p.id) {
                <button type="button" class="pill-btn" [attr.aria-pressed]="store.profil() === p.id" (click)="store.profil.set(p.id)">{{ p.libelle }}</button>
              }
            </div>
          </div>

          <div class="groupe" role="group" aria-labelledby="devis-frequence">
            <span id="devis-frequence" class="field-label">Fréquence</span>
            <div class="pills">
              @for (f of frequences; track f.id) {
                <button type="button" class="pill-btn" [attr.aria-pressed]="store.frequence() === f.id" (click)="store.frequence.set(f.id)">{{ f.libelle }}</button>
              }
            </div>
          </div>

          <div class="qty-grid">
            @for (c of compteurs; track c.code) {
              <div class="qty-box" role="group" [attr.aria-labelledby]="'qty-' + c.code">
                <span class="field-label" [id]="'qty-' + c.code">{{ c.libelle }}</span>
                <div class="stepper">
                  <button type="button" class="step-btn" [attr.aria-label]="'Retirer : ' + c.libelle" (click)="ajuster(c.code, -1)">−</button>
                  <span class="step-val" aria-live="polite">{{ store.quantites()[c.code] }}</span>
                  <button type="button" class="step-btn plus" [attr.aria-label]="'Ajouter : ' + c.libelle" (click)="ajuster(c.code, 1)">+</button>
                </div>
              </div>
            }
          </div>

          <div class="champ">
            <label for="localisation" class="field-label">Localisation</label>
            <input id="localisation" class="field" type="text" placeholder="Ville ou code postal" autocomplete="address-level2"
              [value]="store.localisation()" (input)="store.localisation.set($any($event.target).value)" />
          </div>

          <div class="champ">
            <label for="code-parrain" class="field-label">Code de parrainage <span class="facultatif">(facultatif)</span></label>
            <input id="code-parrain" class="field code" type="text" placeholder="ex. ABS-MARIE7" autocomplete="off" aria-describedby="code-parrain-aide"
              [value]="store.codeParrain()" (change)="store.definirCodeParrain($any($event.target).value)" />
            <span id="code-parrain-aide" class="aide">Un proche vous a recommandé ? Saisissez son code : il sera remercié.</span>
          </div>

          <div class="estimate" aria-live="polite">
            <span class="estimate-titre">Votre estimation</span>
            <span class="estimate-val">{{ store.libelleEstimation() }}</span>
            <span class="aide">
              {{ store.libelleProfil() }} · {{ store.libelleFrequence() }} — tarif de base, réduction selon la formule. Estimation confirmée après échange.
            </span>
            @if (store.reductionNonIncluse()) {
              <span class="aide">Formule {{ store.libelleFrequence() }} : la réduction n'est pas encore incluse, elle vous sera confirmée lors de l'échange.</span>
            }
          </div>

          <button type="button" class="btn-devis calc valider" (click)="valider()">
            <span class="label">Valider mon devis</span><span class="arrow"><abs-fleche [taille]="22" /></span>
          </button>
          @if (afficherAide()) {
            <span class="alerte" role="alert">Ajoutez au moins une vitre, baie ou velux pour valider.</span>
          }
        </div>
      </div>
      <abs-devis-modal />
    </section>
  `,
  styles: `
    .formulaire { width: 100%; max-width: 640px; padding: clamp(22px, 5vw, 40px); display: flex; flex-direction: column; align-items: center; gap: 26px; }
    .groupe { display: flex; flex-direction: column; align-items: center; gap: 12px; }

    .qty-grid { width: 100%; display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px; }
    .qty-box { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .qty-box .field-label { font-size: 13px; }
    .stepper { display: flex; align-items: center; justify-content: space-between; width: 100%; max-width: 150px; height: 48px; padding: 4px; background: rgba(255, 255, 255, 0.9); border: 1px solid #D7E0EA; border-radius: 999px; }
    .step-btn { width: 38px; height: 38px; border-radius: 999px; border: none; background: #EFF6FF; color: var(--ink); font-size: 20px; font-weight: 600; line-height: 1; cursor: pointer; font-family: inherit; transition: background .15s ease, transform .15s ease; }
    .step-btn:hover { background: #DBEAFE; transform: scale(1.06); }
    .step-btn.plus { background: var(--accent); color: #FFFFFF; }
    .step-btn.plus:hover { background: #1F55B5; }
    .step-val { min-width: 28px; text-align: center; font-size: 17px; font-weight: 700; color: var(--ink); }
    @media (max-width: 480px) { .qty-grid { grid-template-columns: 1fr; } .stepper { max-width: 200px; } }

    .champ { width: 100%; display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .champ .field { max-width: 320px; }
    .field.code { text-transform: uppercase; letter-spacing: 0.06em; }
    .facultatif { font-weight: 500; color: var(--text-soft); }
    .aide { font-size: 12px; color: var(--text-soft); max-width: 380px; line-height: 1.5; }

    .estimate { width: 100%; padding: 20px; border-radius: 18px; background: linear-gradient(135deg, rgba(76, 155, 224, 0.16), rgba(125, 182, 234, 0.14)); border: 1px solid rgba(76, 155, 224, 0.35); display: flex; flex-direction: column; align-items: center; gap: 6px; }
    .estimate-titre { font-size: 13px; color: var(--text-muted); font-weight: 500; }
    .estimate-val { font-size: clamp(24px, 7vw, 40px); font-weight: 800; color: var(--accent); letter-spacing: -0.02em; white-space: nowrap; }

    .valider { width: 100%; max-width: 380px; justify-content: space-between; }
    .alerte { font-size: 13px; color: var(--danger); font-weight: 500; }
  `,
})
export class Devis {
  protected readonly store = inject(DevisStore);
  private readonly modal = viewChild.required(DevisModal);

  protected readonly profils = PROFILS;
  protected readonly frequences = FREQUENCES;
  protected readonly compteurs: { code: CodeQuantite; libelle: string }[] = [
    { code: 'vitre', libelle: 'Vitres' },
    { code: 'baie', libelle: 'Baies vitrées' },
    { code: 'velux', libelle: 'Velux' },
  ];

  protected readonly afficherAide = signal(false);

  protected ajuster(code: CodeQuantite, delta: number): void {
    this.store.ajuster(code, delta);
    this.afficherAide.set(false);
  }

  protected valider(): void {
    if (this.store.nbElements() === 0) {
      this.afficherAide.set(true);
      return;
    }
    this.afficherAide.set(false);
    this.modal().ouvrir();
  }
}
