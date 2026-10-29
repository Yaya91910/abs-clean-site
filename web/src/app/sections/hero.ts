import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-hero',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="section hero">
      <div class="container">
        <div class="cta-badge"><span class="live-dot" aria-hidden="true"></span>Essonne · Île-de-France · Devis instantané</div>
        <h1>La transparence,<br /><span class="grad-text">notre métier.</span></h1>
        <p class="sub">Nettoyage et entretien des surfaces vitrées pour particuliers, professionnels et commerces.</p>
        <div class="actions">
          <a href="#devis" class="btn-devis">
            <span class="label">Obtenir mon devis gratuit</span>
            <span class="arrow" aria-hidden="true">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M5 12h14M13 6l6 6-6 6" stroke="var(--accent)" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" /></svg>
            </span>
          </a>
          <a href="#prestations" class="btn-ghost">Découvrir nos prestations</a>
        </div>
        <!-- Photo du héros et pastilles de confiance : À FOURNIR (photos avec droits d'utilisation). -->
      </div>
    </section>
  `,
  styles: `
    .hero { padding-top: 72px; }
    .container { gap: 0; }
    h1 { font-size: clamp(34px, 7vw, 68px); overflow-wrap: anywhere; line-height: 1.05; max-width: 860px; margin-top: 22px; }
    .sub { font-size: clamp(16px, 2vw, 19px); margin-top: 20px; }
    .actions { display: flex; flex-wrap: wrap; justify-content: center; align-items: center; gap: 14px; margin-top: 30px; }
  `,
})
export class Hero {}
