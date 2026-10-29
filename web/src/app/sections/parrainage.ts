import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-parrainage',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section id="parrainage" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Parrainage</span>
          <h2 class="title">Parrainez vos proches, <br /><span class="grad-text">économisez.</span></h2>
        </div>
        <p class="a-venir">Offre −10 %, partage du lien et QR code : à venir.</p>
      </div>
    </section>
  `,
})
export class Parrainage {}
