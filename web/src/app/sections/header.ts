import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Fleche } from '../shared/fleche';
import { Logo } from '../shared/logo';

@Component({
  selector: 'abs-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Logo, Fleche],
  template: `
    <header class="header">
      <a href="/" class="brand" aria-label="ABS clean, retour en haut de page"><abs-logo [hauteur]="56" /></a>
      <div class="right">
        <nav aria-label="Navigation principale" class="nav-links">
          <a href="/#qui-sommes-nous">À propos</a>
          <a href="/#prestations">Prestations</a>
          <a href="/#tarifs">Tarifs</a>
          <a href="/#faq">FAQ</a>
          <a href="/#parrainage">Parrainage</a>
        </nav>
        <a href="/#devis" class="head-cta">
          <span class="tag">2 min</span>Devis gratuit<span class="arrow"><abs-fleche [taille]="18" /></span>
        </a>
      </div>
    </header>
  `,
  styles: `
    :host { position: sticky; top: 0; z-index: 50; display: block; }
    .header {
      width: 100%; padding: 12px 20px 10px; display: flex; align-items: center; justify-content: space-between; gap: 14px;
      background: rgba(255, 255, 255, 0.88); backdrop-filter: blur(16px); -webkit-backdrop-filter: blur(16px);
      border-bottom: 1px solid rgba(21, 52, 95, 0.06);
    }
    .brand { display: inline-flex; align-items: center; flex-shrink: 0; }
    .right { display: flex; align-items: center; gap: 22px; }
    .nav-links { display: flex; align-items: center; gap: 26px; }
    .nav-links a { color: #3C4A5C; font-size: 13px; white-space: nowrap; }
    .nav-links a:hover { color: var(--accent); }
    .head-cta {
      position: relative; display: inline-flex; align-items: center; gap: 12px; min-height: 52px; padding: 0 6px 0 22px;
      border-radius: 999px; color: #FFFFFF; font-weight: 800; font-size: 16px; letter-spacing: -0.01em;
      background: linear-gradient(135deg, #4C9BE0 0%, var(--accent) 40%, #1A4791 100%);
      box-shadow: 0 14px 30px -10px rgba(15, 76, 149, 0.65), 0 0 0 4px rgba(47, 111, 214, 0.14), inset 0 1px 0 rgba(255, 255, 255, 0.35);
      text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2); transition: transform .2s ease, box-shadow .2s ease; white-space: nowrap;
    }
    .head-cta:hover { transform: translateY(-2px) scale(1.04); }
    .head-cta::before { content: ""; position: absolute; inset: -5px; border-radius: 999px; border: 2px solid var(--accent); opacity: 0; animation: cta-ping 2.4s ease-out infinite; pointer-events: none; }
    .tag {
      position: absolute; top: -10px; right: 14px; padding: 2px 8px; border-radius: 999px; background: #157A4E; color: #FFFFFF; /* vert de la charte assombri : contraste AA du texte blanc */
      font-size: 10px; font-weight: 700; letter-spacing: 0.04em; text-transform: uppercase; text-shadow: none; box-shadow: 0 4px 10px -4px rgba(34, 181, 115, 0.7);
    }
    .arrow { width: 40px; height: 40px; border-radius: 999px; background: #FFFFFF; display: flex; align-items: center; justify-content: center; box-shadow: 0 4px 10px -4px rgba(0, 0, 0, 0.25); transition: transform .2s ease; }
    .head-cta:hover .arrow { transform: translateX(3px); }
    @media (max-width: 900px) { .right { gap: 16px; } .nav-links { gap: 16px; } }
    /* Maquette : menu masqué sous 640 px ; masqué dès 720 px pour que liens et bouton ne passent pas sur deux lignes. */
    @media (max-width: 720px) { .nav-links { display: none; } }
    @media (max-width: 400px) {
      abs-logo ::ng-deep svg { height: 44px !important; }
      .head-cta { min-height: 46px; padding-left: 16px; font-size: 14px; gap: 9px; }
      .arrow { width: 34px; height: 34px; }
    }
  `,
})
export class Header {}
