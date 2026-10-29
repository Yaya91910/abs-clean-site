import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-a-propos',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section id="qui-sommes-nous" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">À propos</span>
          <h2 class="title">Qui sommes-nous ?</h2>
          <p class="sub">
            ABS clean est spécialisée dans le nettoyage et l'entretien des surfaces vitrées. Une priorité : un résultat
            impeccable et un service fiable, jusqu'à 2,50 m de hauteur.
          </p>
        </div>
        <div class="grid-3">
          @for (valeur of valeurs; track valeur) {
            <article class="glass card"><h3>{{ valeur }}</h3></article>
          }
        </div>
      </div>
    </section>
  `,
  styles: `h3 { font-size: 17px; }`,
})
export class APropos {
  protected readonly valeurs = ['Ponctualité', 'Disponibilité', 'Discrétion'];
}
