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
          <article class="glass card">
            <div class="icon-box" aria-hidden="true">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><circle cx="12" cy="12" r="8" stroke="currentColor" stroke-width="1.5" /><path d="M12 8v4l3 2" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" /></svg>
            </div>
            <h3>Ponctualité</h3>
            <p>À l'heure, à chaque intervention.</p>
          </article>
          <article class="glass card">
            <div class="icon-box" aria-hidden="true">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><rect x="4" y="5" width="16" height="15" rx="2" stroke="currentColor" stroke-width="1.5" /><path d="M4 10h16M8 3v4M16 3v4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" /></svg>
            </div>
            <h3>Disponibilité</h3>
            <p>Des créneaux adaptés à votre rythme.</p>
          </article>
          <article class="glass card">
            <div class="icon-box" aria-hidden="true">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><path d="M12 3l7 3v6c0 4.5-3 7.5-7 9-4-1.5-7-4.5-7-9V6l7-3z" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round" /><path d="M9 12l2 2 4-4" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" /></svg>
            </div>
            <h3>Discrétion</h3>
            <p>Un travail soigné, sans vous déranger.</p>
          </article>
        </div>
      </div>
    </section>
  `,
})
export class APropos {}
