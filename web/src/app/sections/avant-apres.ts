import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-avant-apres',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Résultat</span>
          <h2 class="title">Visible immédiatement</h2>
        </div>
        <p class="a-venir">Visuel avant / après : photos réelles à fournir.</p>
      </div>
    </section>
  `,
})
export class AvantApres {}
