import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-prestations',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section id="prestations" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Prestations</span>
          <h2 class="title">Nos prestations</h2>
        </div>
        <p class="a-venir">Particuliers, Commerces, Professionnels, Option : contenu à intégrer depuis la maquette.</p>
      </div>
    </section>
  `,
})
export class Prestations {}
