import { ChangeDetectionStrategy, Component, DOCUMENT, computed, effect, inject, input, signal, untracked } from '@angular/core';

const DUREE_MS = 450;

/**
 * Estimation « À partir de X € » dont le chiffre défile jusqu'à sa nouvelle valeur, avec un léger rebond.
 * Visuel seulement (aria-hidden) : le texte définitif est annoncé à part, sans les valeurs intermédiaires.
 */
@Component({
  selector: 'abs-montant',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true', '[class.rebond-a]': 'rebonds() % 2 === 1', '[class.rebond-b]': 'rebonds() > 0 && rebonds() % 2 === 0' },
  template: `{{ texte() }}`,
  styles: `
    :host { display: inline-block; font-variant-numeric: tabular-nums; }
    :host(.rebond-a) { animation: rebond-a .35s ease; }
    :host(.rebond-b) { animation: rebond-b .35s ease; }
    @keyframes rebond-a { 40% { transform: scale(1.08); } }
    @keyframes rebond-b { 40% { transform: scale(1.08); } }
  `,
})
export class Montant {
  /** Montant en euros, ou null si rien n'est encore choisi (« — € »). */
  readonly valeur = input<number | null>(null);

  private readonly affiche = signal<number | null>(null);
  protected readonly rebonds = signal(0);
  protected readonly texte = computed(() => {
    const v = this.affiche();
    return v === null ? '— €' : `À partir de ${v} €`;
  });

  private readonly fenetre = inject(DOCUMENT).defaultView;
  private animation = 0;
  private filet = 0;

  constructor() {
    effect(() => {
      const cible = this.valeur();
      untracked(() => this.animer(cible));
    });
  }

  private animer(cible: number | null): void {
    const depart = this.affiche();
    const fenetre = this.fenetre;
    // Toute animation en cours (et son filet) est remplacée par la nouvelle.
    if (fenetre?.cancelAnimationFrame) fenetre.cancelAnimationFrame(this.animation);
    fenetre?.clearTimeout(this.filet);
    if (cible === depart) return;
    if (depart !== null && cible !== null) this.rebonds.update((n) => n + 1);

    const sobre = !fenetre?.requestAnimationFrame || fenetre.matchMedia?.('(prefers-reduced-motion: reduce)').matches;
    if (cible === null || depart === null || sobre) {
      this.affiche.set(cible);
      return;
    }
    const debut = fenetre.performance.now();
    const etape = (maintenant: number) => {
      const t = Math.min(1, (maintenant - debut) / DUREE_MS);
      const doux = 1 - (1 - t) ** 3;
      this.affiche.set(Math.round(depart + (cible - depart) * doux));
      if (t < 1) this.animation = fenetre.requestAnimationFrame(etape);
    };
    this.animation = fenetre.requestAnimationFrame(etape);
    // Filet : si les images sont suspendues (onglet en arrière-plan), la valeur finale s'affiche quand même.
    this.filet = fenetre.setTimeout(() => {
      fenetre.cancelAnimationFrame(this.animation);
      this.affiche.set(cible);
    }, DUREE_MS + 150);
  }
}
