import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-processus',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Processus</span>
          <h2 class="title">Comment ça marche</h2>
        </div>
        <p class="a-venir">Les 3 étapes : contenu à intégrer depuis la maquette.</p>
      </div>
    </section>
  `,
})
export class Processus {}
