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
          <p class="sub">Des solutions adaptées à tous vos besoins.</p>
        </div>
        <div class="grid-2">
          <article class="glass card">
            <div class="icon-box" aria-hidden="true">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><path d="M3 11l9-7 9 7M5 10v9h14v-9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" /></svg>
            </div>
            <h3>Particuliers</h3>
            <p>Fenêtres, baies vitrées, vérandas, verrières, fenêtres de toit.</p>
          </article>
          <article class="glass card">
            <div class="icon-box" aria-hidden="true">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><path d="M4 9l1.2-5h13.6L20 9M4 9v11h16V9M4 9c0 1.4 1.1 2.5 2.7 2.5S9.3 10.4 9.3 9c0 1.4 1.2 2.5 2.7 2.5s2.7-1.1 2.7-2.5c0 1.4 1.1 2.5 2.6 2.5S20 10.4 20 9M10 20v-5h4v5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" /></svg>
            </div>
            <h3>Commerces</h3>
            <p>Vitrines, devantures vitrées et contrats d'entretien réguliers.</p>
          </article>
          <article class="glass card">
            <div class="icon-box" aria-hidden="true">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><rect x="5" y="3" width="14" height="18" rx="1.5" stroke="currentColor" stroke-width="1.5" /><path d="M9 7h2M13 7h2M9 11h2M13 11h2M9 15h2M13 15h2" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" /></svg>
            </div>
            <h3>Professionnels</h3>
            <p>Bureaux, locaux professionnels, restaurants, cabinets, agences, halls d'accueil…</p>
          </article>
          <article class="glass card">
            <div class="icon-box" aria-hidden="true">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><path d="M4 7h16M4 12h16M4 17h16" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" /><circle cx="8" cy="7" r="1.6" fill="currentColor" /><circle cx="15" cy="12" r="1.6" fill="currentColor" /><circle cx="10" cy="17" r="1.6" fill="currentColor" /></svg>
            </div>
            <h3>Option</h3>
            <p>Encadrements, rails et rebords.</p>
          </article>
        </div>
      </div>
    </section>
  `,
  styles: `.card h3 { font-size: 18px; } .card p { max-width: 300px; }`,
})
export class Prestations {}
