import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Buee } from '../shared/buee';
import { Fleche } from '../shared/fleche';

@Component({
  selector: 'abs-hero',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Fleche, Buee],
  template: `
    <section class="section hero">
      <div class="hero-grid">
        <div class="hero-texte">
          <div class="cta-badge"><span class="live-dot" aria-hidden="true"></span>Essonne · Île-de-France · Devis instantané</div>
          <h1>La transparence,<br /><span class="grad-text">notre métier.</span></h1>
          <p class="sub">Nettoyage et entretien des surfaces vitrées pour particuliers, professionnels et commerces.</p>
          <div class="actions">
            <a href="#devis" class="btn-devis"><span class="label">Obtenir mon devis gratuit</span><span class="arrow"><abs-fleche /></span></a>
            <a href="#prestations" class="btn-discover">
              Découvrir nos prestations
              <span class="chev" aria-hidden="true"><svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M6 9l6 6 6-6" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg></span>
            </a>
          </div>
          <ul class="trustrow">
            @for (atout of atouts; track atout) {
              <li class="trust-chip">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M5 13l4 4L19 7" stroke="var(--accent)" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" /></svg>{{ atout }}
              </li>
            }
          </ul>
        </div>

        <!-- Photo d'intervention : À FOURNIR (droits d'utilisation obligatoires). -->
        <div class="glass visuel-cadre">
          <div class="hero-visual">
            <svg width="36" height="36" viewBox="0 0 24 24" fill="none" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2" stroke="#9BB3CC" stroke-width="1.4" /><circle cx="8.5" cy="10" r="1.4" stroke="#9BB3CC" stroke-width="1.4" /><path d="M21 15l-5-4-4 3-3-2-6 5" stroke="#9BB3CC" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
            <span>[Photo : intervention ABS clean]</span>
            <abs-buee />
          </div>
          <div class="flottant haut" aria-hidden="true"><span class="live-dot"></span>Estimation en direct</div>
          <div class="flottant bas" aria-hidden="true">Jusqu'à <strong>2,50 m</strong> de hauteur</div>
        </div>
      </div>
    </section>
  `,
  styles: `
    .hero { padding-top: 64px; }
    /* Ordinateur : promesse et bouton à gauche, photo à droite, le tout visible sans défiler. Mobile : empilé et centré. */
    .hero-grid { width: 100%; max-width: 1120px; display: grid; grid-template-columns: minmax(0, 1fr); gap: 40px; align-items: center; }
    .hero-texte { display: flex; flex-direction: column; align-items: center; min-width: 0; }
    h1 { font-size: clamp(34px, 6.4vw, 64px); overflow-wrap: anywhere; line-height: 1.05; max-width: 860px; margin-top: 22px; }
    .sub { font-size: clamp(16px, 2vw, 19px); margin-top: 20px; }
    .actions { display: flex; flex-wrap: wrap; justify-content: center; align-items: center; gap: 12px; margin-top: 30px; }

    .btn-discover {
      display: inline-flex; align-items: center; gap: 10px; min-height: 56px; padding: 0 24px;
      border-radius: 999px; font-weight: 600; font-size: 16px; color: var(--ink);
      background: rgba(255, 255, 255, 0.85); border: 1px solid var(--trait);
      transition: transform .2s ease, box-shadow .2s ease, background .2s ease;
    }
    .btn-discover:hover { transform: translateY(-2px); background: #FFFFFF; box-shadow: 0 12px 24px -16px rgba(21, 52, 95, 0.35); }
    .chev { display: inline-flex; color: var(--accent); }
    @media (max-width: 400px) { .btn-discover { font-size: 15px; } }

    .visuel-cadre { position: relative; width: 100%; padding: 10px; }
    /* Pastilles de verre qui flottent doucement au-dessus de la photo. */
    .flottant {
      position: absolute; display: inline-flex; align-items: center; gap: 9px; padding: 11px 16px; border-radius: 16px;
      font-size: 13px; font-weight: 600; color: var(--ink); white-space: nowrap;
      background: rgba(255, 255, 255, 0.6); backdrop-filter: blur(16px) saturate(170%); -webkit-backdrop-filter: blur(16px) saturate(170%);
      border: 1px solid rgba(255, 255, 255, 0.95); box-shadow: 0 18px 40px -22px rgba(21, 52, 95, 0.45);
      animation: flotter 7s ease-in-out infinite alternate;
    }
    .flottant strong { font-size: 18px; font-weight: 800; color: var(--accent); letter-spacing: -0.02em; }
    .flottant.haut { top: 28px; left: -14px; }
    .flottant.bas { bottom: 30px; right: -14px; animation-delay: -3.5s; }
    @keyframes flotter { from { transform: translateY(0); } to { transform: translateY(-10px); } }
    /* Sur téléphone, pastilles immobiles : une surface floutée qui bouge oblige à recalculer le flou à chaque image. */
    @media (max-width: 719px) { .flottant { animation: none; } }
    @media (max-width: 520px) { .flottant.haut { left: 18px; } .flottant.bas { right: 18px; } }
    .hero-visual {
      position: relative; width: 100%; aspect-ratio: 4 / 3; border-radius: 18px; background: linear-gradient(165deg, #DCEBFA 0%, #B5D2F0 55%, #8DB7E6 100%); /* « ciel » derrière la vitre, en attendant la photo */
      display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 10px; font-size: 13px; color: var(--ink);
    }

    .trustrow { list-style: none; margin: 28px 0 0; padding: 0; display: flex; flex-wrap: wrap; justify-content: center; gap: 10px; }
    .trust-chip {
      display: inline-flex; align-items: center; gap: 7px; padding: 8px 14px; background: rgba(255, 255, 255, 0.7);
      border: 1px solid rgba(255, 255, 255, 0.95); border-radius: 999px; font-size: 13px; font-weight: 500; color: #3C4A5C;
    }

    @media (min-width: 960px) {
      .hero { padding-top: 72px; padding-bottom: 64px; }
      .hero-grid { grid-template-columns: minmax(0, 1.15fr) minmax(0, 0.85fr); gap: 56px; }
      .hero-texte { align-items: flex-start; text-align: left; }
      .actions, .trustrow { justify-content: flex-start; }
      .hero-visual { aspect-ratio: 4 / 5; }
    }
  `,
})
export class Hero {
  protected readonly atouts = ['Devis gratuit', 'Sans engagement', 'Matériel professionnel', 'Intervention rapide'];
}
