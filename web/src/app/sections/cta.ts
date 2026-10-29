import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Fleche } from '../shared/fleche';

@Component({
  selector: 'abs-cta',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Fleche],
  template: `
    <section class="section cta">
      <div class="band">
        <div class="blob" aria-hidden="true"></div>
        <h2>Prêt pour des vitres impeccables ?</h2>
        <p>Votre tarif s'affiche en moins de 2 minutes, sans engagement.</p>
        <span class="final-wrap">
          <a href="#devis" class="btn-devis hero-final"><span class="label">Obtenir mon devis gratuit</span><span class="arrow"><abs-fleche [taille]="22" couleur="#FFFFFF" /></span></a>
        </span>
      </div>
    </section>
  `,
  styles: `
    .cta { padding-top: 20px; }
    .band {
      position: relative; overflow: hidden; width: 100%; max-width: 1000px; border-radius: 28px; padding: clamp(44px, 7vw, 72px) 24px;
      display: flex; flex-direction: column; align-items: center; gap: 24px;
      background: linear-gradient(135deg, var(--ink) 0%, #183A6A 60%, var(--accent) 130%);
    }
    .blob { position: absolute; top: -80px; right: -60px; width: 260px; height: 260px; border-radius: 50%; background: var(--accent2); opacity: 0.35; filter: blur(60px); pointer-events: none; }
    h2 { position: relative; font-size: clamp(28px, 4.5vw, 42px); color: #FFFFFF; max-width: 560px; line-height: 1.12; }
    p { position: relative; font-size: 16px; color: rgba(255, 255, 255, 0.75); margin: 0; max-width: 420px; }
    .final-wrap { position: relative; display: inline-flex; margin-top: 6px; }
    .final-wrap::before { content: ""; position: absolute; inset: -7px; border-radius: 999px; border: 2px solid rgba(255, 255, 255, 0.7); animation: cta-ping 2.4s ease-out infinite; pointer-events: none; }
    .btn-devis.hero-final { min-height: 66px; padding: 0 10px 0 32px; gap: 16px; background: linear-gradient(135deg, #FFFFFF 0%, #E3EFFB 100%); color: var(--ink); box-shadow: 0 0 0 6px rgba(255, 255, 255, 0.14), 0 0 46px rgba(76, 155, 224, 0.65), 0 20px 36px -14px rgba(0, 0, 0, 0.45); }
    .btn-devis.hero-final:hover { transform: translateY(-3px) scale(1.03); }
    .btn-devis.hero-final .label { font-size: 19px; font-weight: 800; letter-spacing: -0.01em; color: var(--ink); }
    .btn-devis.hero-final .arrow { width: 48px; height: 48px; background: linear-gradient(135deg, var(--accent2), var(--accent)); box-shadow: 0 6px 14px -6px var(--accent-shadow); }
    .btn-devis.hero-final::after { background: linear-gradient(100deg, transparent, rgba(47, 111, 214, 0.22), transparent); }
    @media (max-width: 400px) { .btn-devis.hero-final { min-height: 58px; padding-left: 22px; } .btn-devis.hero-final .label { font-size: 16px; } .btn-devis.hero-final .arrow { width: 42px; height: 42px; } }
  `,
})
export class Cta {}
