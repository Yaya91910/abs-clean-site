import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Fleche } from '../shared/fleche';

@Component({
  selector: 'abs-hero',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Fleche],
  template: `
    <section class="section hero">
      <div class="container">
        <div class="cta-badge"><span class="live-dot" aria-hidden="true"></span>Essonne · Île-de-France · Devis instantané</div>
        <h1>La transparence,<br /><span class="grad-text">notre métier.</span></h1>
        <p class="sub">Nettoyage et entretien des surfaces vitrées pour particuliers, professionnels et commerces.</p>
        <div class="actions">
          <a href="#devis" class="btn-devis"><span class="label">Obtenir mon devis gratuit</span><span class="arrow"><abs-fleche /></span></a>
          <a href="#prestations" class="btn-discover">
            <span class="ico" aria-hidden="true">
              <svg width="18" height="18" viewBox="0 0 24 24"><rect x="3" y="3" width="7" height="7" rx="2" fill="var(--accent)" /><rect x="14" y="3" width="7" height="7" rx="2" fill="var(--accent2)" /><rect x="3" y="14" width="7" height="7" rx="2" fill="var(--accent2)" /><rect x="14" y="14" width="7" height="7" rx="2" fill="var(--accent)" /></svg>
            </span>
            Découvrir nos prestations
            <span class="chev" aria-hidden="true"><svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M6 9l6 6 6-6" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg></span>
          </a>
        </div>

        <!-- Photo d'intervention : À FOURNIR (droits d'utilisation obligatoires). -->
        <div class="glass visuel-cadre">
          <div class="hero-visual">
            <svg width="36" height="36" viewBox="0 0 24 24" fill="none" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2" stroke="#9BB3CC" stroke-width="1.4" /><circle cx="8.5" cy="10" r="1.4" stroke="#9BB3CC" stroke-width="1.4" /><path d="M21 15l-5-4-4 3-3-2-6 5" stroke="#9BB3CC" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
            <span>[Photo : intervention ABS clean]</span>
          </div>
        </div>

        <ul class="trustrow">
          @for (atout of atouts; track atout) {
            <li class="trust-chip">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true"><path d="M5 13l4 4L19 7" stroke="var(--accent)" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" /></svg>{{ atout }}
            </li>
          }
        </ul>
      </div>
    </section>
  `,
  styles: `
    .hero { padding-top: 72px; }
    .container { gap: 0; }
    h1 { font-size: clamp(34px, 7vw, 68px); overflow-wrap: anywhere; line-height: 1.05; max-width: 860px; margin-top: 22px; }
    .sub { font-size: clamp(16px, 2vw, 19px); margin-top: 20px; }
    .actions { display: flex; flex-wrap: wrap; justify-content: center; align-items: center; gap: 14px; margin-top: 30px; }

    .btn-discover {
      position: relative; display: inline-flex; align-items: center; gap: 12px; min-height: 56px; padding: 0 22px 0 8px;
      border-radius: 999px; font-weight: 600; font-size: 16px; color: var(--ink); border: 2px solid transparent;
      background: linear-gradient(120deg, #EEF4FF 0%, #F0F9FF 50%, #EAF3FC 100%) padding-box,
                  linear-gradient(120deg, #93C5FD, #CFE4F8, #CFE4F8, #93C5FD) border-box;
      background-size: 100% 100%, 300% 100%; animation: border-flow 6s linear infinite;
      box-shadow: 0 12px 26px -14px rgba(76, 155, 224, 0.6); transition: transform .2s ease, box-shadow .2s ease;
    }
    .btn-discover:hover { transform: translateY(-2px); box-shadow: 0 16px 32px -12px rgba(76, 155, 224, 0.75), 0 0 0 7px rgba(147, 197, 253, 0.25); }
    .ico { width: 40px; height: 40px; border-radius: 999px; background: #FFFFFF; box-shadow: 0 4px 10px -4px rgba(47, 111, 214, 0.35); display: flex; align-items: center; justify-content: center; flex-shrink: 0; transition: transform .3s ease; }
    .btn-discover:hover .ico { transform: rotate(90deg); }
    .chev { display: inline-flex; color: var(--accent); animation: chev-bounce 1.8s ease-in-out infinite; }
    @keyframes border-flow { 0% { background-position: 0 0, 0% 50%; } 100% { background-position: 0 0, 300% 50%; } }
    @keyframes chev-bounce { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(4px); } }
    @media (max-width: 400px) { .btn-discover { font-size: 15px; } }

    .visuel-cadre { width: 100%; max-width: 1000px; margin-top: 48px; padding: 10px; }
    .hero-visual {
      width: 100%; aspect-ratio: 21 / 9; border-radius: 18px; background: linear-gradient(135deg, #EFF6FF, #F5F9FF);
      display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 10px; font-size: 13px; color: var(--text-soft);
    }
    @media (max-width: 680px) { .hero-visual { aspect-ratio: 4 / 3; } }

    .trustrow { list-style: none; margin: 28px 0 0; padding: 0; display: flex; flex-wrap: wrap; justify-content: center; gap: 12px; }
    .trust-chip {
      display: inline-flex; align-items: center; gap: 7px; padding: 8px 14px; background: rgba(255, 255, 255, 0.65);
      border: 1px solid rgba(255, 255, 255, 0.95); border-radius: 999px; font-size: 13px; font-weight: 500; color: #3C4A5C;
    }
  `,
})
export class Hero {
  protected readonly atouts = ['Devis gratuit', 'Sans engagement', 'Matériel professionnel', 'Intervention rapide'];
}
