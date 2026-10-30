import { ChangeDetectionStrategy, Component, DestroyRef, DOCUMENT, ElementRef, afterNextRender, computed, inject, signal, viewChild } from '@angular/core';
import { CodeQuantite, DevisStore, FREQUENCES, PROFILS } from '../core/devis-store';
import { Fleche } from '../shared/fleche';
import { Montant } from '../shared/montant';
import { DevisModal } from './devis-modal';

@Component({
  selector: 'abs-devis',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Fleche, Montant, DevisModal],
  template: `
    <section id="devis" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Devis</span>
          <h2 class="title">Votre devis en 2 minutes</h2>
          <p class="sub">Choisissez vos options, votre estimation s'affiche aussitôt.</p>
        </div>

        <div class="glass formulaire">
          <div class="options">
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
              <input id="code-parrain" class="field code" type="text" placeholder="ex. ABS-1765321" autocomplete="off" aria-describedby="code-parrain-aide"
                [value]="store.codeParrain()" (change)="store.definirCodeParrain($any($event.target).value)" />
              <span id="code-parrain-aide" class="aide">Un proche vous a recommandé ? Saisissez son code : il sera remercié.</span>
              @switch (store.codeReconnu()) {
                @case (true) {
                  <span class="aide code-ok" role="status">✓ Code reconnu : votre proche sera remercié.</span>
                }
                @case (false) {
                  <span class="aide" role="status">Code inconnu : il ne sera pas pris en compte, mais vous pouvez envoyer votre demande.</span>
                }
              }
            </div>
          </div>

          <!-- Récapitulatif : collé à droite sur ordinateur, l'estimation reste sous les yeux pendant les choix. -->
          <div class="resume">
            <div class="estimate" aria-live="polite" #estimation>
              <span class="estimate-titre">Votre estimation</span>
              <span class="estimate-val"><abs-montant [valeur]="store.estimation()" /></span>
              <span class="visually-hidden">{{ store.libelleEstimation() }}</span>
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
            @if (store.repris()) {
              <p class="aide repris">Vos choix de la dernière visite ont été gardés. <button type="button" class="lien" (click)="store.reinitialiser()">Tout remettre à zéro</button></p>
            }
            <ul class="rassurance">
              <li>Devis gratuit</li>
              <li>Sans engagement</li>
              <li>Devis PDF envoyé par e-mail</li>
            </ul>
          </div>
        </div>
      </div>
      <!-- Téléphone : le récapitulatif est sous les compteurs ; cette barre garde l'estimation sous le pouce. -->
      <div class="barre" role="region" aria-label="Estimation en cours" [hidden]="!barreVisible()">
        <span class="barre-montant"><abs-montant [valeur]="store.estimation()" /></span>
        <button type="button" class="barre-btn" (click)="valider()">Continuer<abs-fleche [taille]="18" couleur="#FFFFFF" /></button>
      </div>
      <abs-devis-modal />
    </section>
  `,
  styles: `
    .formulaire { width: 100%; max-width: 640px; padding: clamp(22px, 5vw, 40px); display: flex; flex-direction: column; align-items: center; gap: 26px; }
    .options, .resume { width: 100%; display: flex; flex-direction: column; align-items: center; gap: 26px; }
    .resume { gap: 16px; }
    @media (min-width: 900px) {
      .formulaire { max-width: 980px; display: grid; grid-template-columns: minmax(0, 1.3fr) minmax(0, 1fr); align-items: start; gap: 36px; }
      .resume { position: sticky; top: 96px; padding: 24px; border-radius: 20px; background: #FFFFFF; border: 1px solid var(--trait); box-shadow: 0 18px 40px -30px rgba(21, 52, 95, 0.35); }
      .resume .estimate { padding: 22px 16px; }
    }
    .rassurance { list-style: none; margin: 0; padding: 0; display: flex; flex-wrap: wrap; justify-content: center; gap: 6px 14px; font-size: 13px; color: var(--text-muted); }
    .rassurance li { display: inline-flex; align-items: center; gap: 6px; }
    .rassurance li::before { content: "✓"; color: #157A4E; font-weight: 700; }
    .groupe { display: flex; flex-direction: column; align-items: center; gap: 12px; }

    .qty-grid { width: 100%; display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px; }
    .qty-box { display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .qty-box .field-label { font-size: 13px; }
    .stepper { display: flex; align-items: center; justify-content: space-between; width: 100%; max-width: 150px; height: 48px; padding: 4px; background: rgba(255, 255, 255, 0.9); border: 1px solid #D7E0EA; border-radius: 999px; }
    .step-btn { width: 38px; height: 38px; border-radius: 999px; border: none; background: #EFF6FF; color: var(--ink); font-size: 20px; font-weight: 600; line-height: 1; cursor: pointer; font-family: inherit; transition: background .15s ease, transform .15s ease; }
    .step-btn:hover { background: #DBEAFE; transform: scale(1.06); }
    .step-btn.plus { background: var(--accent); color: #FFFFFF; }
    .step-btn.plus:hover { background: var(--accent-fonce); }
    .step-val { min-width: 28px; text-align: center; font-size: 17px; font-weight: 700; color: var(--ink); }
    @media (max-width: 480px) { .qty-grid { grid-template-columns: 1fr; } .stepper { max-width: 200px; } }

    .champ { width: 100%; display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .champ .field { max-width: 320px; }
    .field.code { text-transform: uppercase; letter-spacing: 0.06em; }
    .facultatif { font-weight: 500; color: var(--text-soft); }
    .code-ok { color: #157A4E; font-weight: 600; }
    .aide { font-size: 12px; color: var(--text-soft); max-width: 380px; line-height: 1.5; }

    .estimate { width: 100%; padding: 20px; border-radius: 18px; background: var(--brume); border: 1px solid var(--brume2); display: flex; flex-direction: column; align-items: center; gap: 6px; }
    .estimate-titre { font-size: 13px; color: var(--text-muted); font-weight: 500; }
    .estimate .aide { color: var(--text-muted); } /* fond teinté : gris plus foncé pour le contraste AA */
    .estimate-val { font-size: clamp(24px, 7vw, 40px); font-weight: 800; color: var(--accent); letter-spacing: -0.02em; white-space: nowrap; }

    .valider { width: 100%; max-width: 380px; justify-content: space-between; }
    .repris { text-align: center; margin: 0; }
    .lien { border: none; background: none; padding: 0; font: inherit; color: var(--accent); text-decoration: underline; cursor: pointer; }

    .barre {
      position: fixed; z-index: 40; left: 12px; right: 12px; bottom: calc(12px + env(safe-area-inset-bottom)); display: flex; align-items: center; justify-content: space-between; gap: 12px;
      padding: 8px 8px 8px 18px; border-radius: 999px; background: rgba(255, 255, 255, 0.92); border: 1px solid var(--trait);
      box-shadow: 0 18px 40px -18px rgba(21, 52, 95, 0.45); animation: barre-entree .3s ease;
    }
    .barre[hidden] { display: none; }
    .barre-montant { font-weight: 800; font-size: 17px; color: var(--accent); white-space: nowrap; }
    .barre-btn { display: inline-flex; align-items: center; gap: 8px; min-height: 44px; padding: 0 16px 0 20px; border: none; border-radius: 999px; font: inherit; font-weight: 700; color: #FFFFFF; background: var(--accent); cursor: pointer; }
    @keyframes barre-entree { from { transform: translateY(120%); } }
    @media (min-width: 900px) { .barre { display: none; } }
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

  private readonly estimation = viewChild.required<ElementRef<HTMLElement>>('estimation');
  private readonly sectionVisible = signal(false);
  private readonly resumeVisible = signal(true);
  /** Barre du bas (téléphone) : dans la section devis, éléments choisis, estimation hors de l'écran. */
  protected readonly barreVisible = computed(() => this.sectionVisible() && !this.resumeVisible() && this.store.nbElements() > 0);

  constructor() {
    const hote = inject<ElementRef<HTMLElement>>(ElementRef).nativeElement;
    const fenetre = inject(DOCUMENT).defaultView;
    const destruction = inject(DestroyRef);
    afterNextRender(() => {
      if (!fenetre || !('IntersectionObserver' in fenetre)) return;
      const section = new IntersectionObserver(([entree]) => this.sectionVisible.set(entree.isIntersecting));
      // L'estimation compte comme visible quand on en voit au moins la moitié (pas un simple liseré en bas d'écran).
      const estimation = new IntersectionObserver(([entree]) => this.resumeVisible.set(entree.isIntersecting), { threshold: 0.5 });
      section.observe(hote.querySelector('section')!);
      estimation.observe(this.estimation().nativeElement);
      destruction.onDestroy(() => {
        section.disconnect();
        estimation.disconnect();
      });
    });
  }

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
