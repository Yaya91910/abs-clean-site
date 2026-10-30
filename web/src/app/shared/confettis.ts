import { ChangeDetectionStrategy, Component } from '@angular/core';

/**
 * Petite gerbe de confettis aux couleurs du logo, lancée une fois à l'affichage (écran de succès du devis).
 * Décoratif ; invisible si l'utilisateur a demandé à réduire les animations (elles sont alors coupées et les confettis restent transparents).
 */
@Component({
  selector: 'abs-confettis',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true' },
  template: `
    @for (i of morceaux; track i) {
      <i [style.--i]="i"></i>
    }
  `,
  styles: `
    :host { position: absolute; left: 50%; top: 50%; width: 0; height: 0; pointer-events: none; }
    i {
      position: absolute; left: -4px; top: -6px; width: 8px; height: 12px; border-radius: 2px; opacity: 0;
      background: var(--accent); --angle: calc(var(--i) * 22.5deg);
      animation: gerbe 1.3s cubic-bezier(.2, .7, .3, 1) forwards; animation-delay: calc(var(--i) * 12ms);
    }
    i:nth-child(4n + 1) { background: var(--accent2); }
    i:nth-child(4n + 2) { background: var(--success); width: 7px; height: 7px; border-radius: 999px; }
    i:nth-child(4n + 3) { background: var(--ink); }
    @keyframes gerbe {
      0% { opacity: 1; transform: rotate(var(--angle)) translateY(0) rotate(0); }
      70% { opacity: 1; }
      100% { opacity: 0; transform: rotate(var(--angle)) translateY(-120px) rotate(320deg); }
    }
  `,
})
export class Confettis {
  protected readonly morceaux = Array.from({ length: 16 }, (_, i) => i);
}
