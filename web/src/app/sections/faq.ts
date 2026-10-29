import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-faq',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section id="faq" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">FAQ</span>
          <h2 class="title">Vos questions, nos réponses</h2>
        </div>
        <p class="a-venir">Accordéon de 8 questions : à intégrer depuis la maquette.</p>
      </div>
    </section>
  `,
})
export class Faq {}
