import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Logo ABS clean : réutilise le symbole SVG inline déclaré dans index.html. */
@Component({
  selector: 'abs-logo',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<svg viewBox="0 0 812.8 402.8" role="img" aria-label="ABS clean" [style.height.px]="hauteur()"><use href="#abs-logo" /></svg>`,
  styles: `:host { display: inline-flex; } svg { width: auto; aspect-ratio: 812.8 / 402.8; display: block; }`,
})
export class Logo {
  readonly hauteur = input(56);
}
