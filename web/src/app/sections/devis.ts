import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-devis',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section id="devis" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Devis</span>
          <h2 class="title">Votre devis en 2 minutes</h2>
        </div>
        <p class="a-venir">Formulaire, estimation en direct et fenêtre de coordonnées : à venir.</p>
      </div>
    </section>
  `,
})
export class Devis {}
