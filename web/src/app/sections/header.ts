import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="header">
      <a href="#" class="brand" aria-label="ABS clean, retour en haut de page">
        <img src="abs-clean-logo.svg" alt="ABS clean" width="97" height="48" />
      </a>
      <div class="right">
        <nav aria-label="Navigation principale" class="nav-links">
          <a href="#qui-sommes-nous">À propos</a>
          <a href="#prestations">Prestations</a>
          <a href="#tarifs">Tarifs</a>
          <a href="#faq">FAQ</a>
          <a href="#parrainage">Parrainage</a>
        </nav>
        <a href="#devis" class="head-cta"><span class="tag">2 min</span>Devis gratuit</a>
      </div>
    </header>
  `,
  styles: `
    .header {
      position: sticky; top: 0; z-index: 50; width: 100%;
      padding: 12px 20px 10px; display: flex; align-items: center; justify-content: space-between; gap: 14px;
      background: rgba(255, 255, 255, 0.88); backdrop-filter: blur(16px); -webkit-backdrop-filter: blur(16px);
      border-bottom: 1px solid rgba(21, 52, 95, 0.06);
    }
    :host { position: sticky; top: 0; z-index: 50; }
    .brand img { display: block; height: 48px; width: auto; }
    .right { display: flex; align-items: center; gap: 22px; }
    .nav-links { display: flex; gap: 18px; }
    .nav-links a { color: #3C4A5C; font-size: 13px; }
    .nav-links a:hover { color: var(--accent); }
    .head-cta {
      position: relative; display: inline-flex; align-items: center; min-height: 48px; padding: 0 22px;
      border-radius: 999px; color: #FFFFFF; font-weight: 800; font-size: 15px;
      background: linear-gradient(135deg, #4C9BE0 0%, var(--accent) 40%, #1A4791 100%);
      box-shadow: 0 14px 30px -10px rgba(15, 76, 149, 0.65);
    }
    .tag {
      position: absolute; top: -10px; right: 14px; padding: 2px 8px; border-radius: 999px;
      background: var(--success); color: #FFFFFF; font-size: 10px; font-weight: 700; letter-spacing: 0.04em; text-transform: uppercase;
    }
    @media (max-width: 639px) { .nav-links { display: none; } }
  `,
})
export class Header {}
