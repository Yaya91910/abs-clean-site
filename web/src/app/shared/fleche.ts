import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Flèche « → » des boutons devis. */
@Component({
  selector: 'abs-fleche',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg [attr.width]="taille()" [attr.height]="taille()" viewBox="0 0 24 24" fill="none" aria-hidden="true">
      <path d="M5 12h14M13 6l6 6-6 6" [style.stroke]="couleur()" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" />
    </svg>
  `,
  styles: `:host { display: inline-flex; }`,
})
export class Fleche {
  readonly taille = input(16);
  readonly couleur = input('var(--accent)');
}
