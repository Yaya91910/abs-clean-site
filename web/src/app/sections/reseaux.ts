import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-reseaux',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Réseaux sociaux</span>
          <h2 class="title">Rejoignez-nous sur les réseaux</h2>
        </div>
        <p class="a-venir">Liens TikTok et Instagram : à fournir par le gérant.</p>
      </div>
    </section>
  `,
})
export class Reseaux {}
