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
          <a href="#devis" class="btn-devis hero-final"><span class="label">Obtenir mon devis gratuit</span><span class="arrow"><abs-fleche [taille]="22" /></span></a>
        </span>
      </div>
    </section>
  `,
  styles: `
    .cta { padding-top: 20px; }
    .band {
      position: relative; overflow: hidden; width: 100%; max-width: 1000px; border-radius: 28px; padding: clamp(44px, 7vw, 72px) 24px;
      display: flex; flex-direction: column; align-items: center; gap: 24px;
      background: linear-gradient(135deg, var(--brume) 0%, var(--brume2) 100%); border: 1px solid #FFFFFF;
      box-shadow: 0 24px 50px -34px rgba(21, 52, 95, 0.35);
    }
    .blob { position: absolute; top: -90px; right: -70px; width: 280px; height: 280px; border-radius: 50%; background: #FFFFFF; opacity: 0.7; filter: blur(50px); pointer-events: none; }
    h2 { position: relative; font-size: clamp(28px, 4.5vw, 42px); color: var(--ink); max-width: 560px; line-height: 1.12; }
    p { position: relative; font-size: 16px; color: var(--text-muted); margin: 0; max-width: 420px; }
    .final-wrap { position: relative; display: inline-flex; margin-top: 6px; }
    .btn-devis.hero-final { min-height: 64px; padding: 0 10px 0 30px; gap: 16px; }
    .btn-devis.hero-final .label { font-size: 18px; font-weight: 700; }
    .btn-devis.hero-final .arrow { width: 46px; height: 46px; }
    @media (max-width: 400px) { .btn-devis.hero-final { min-height: 58px; padding-left: 22px; } .btn-devis.hero-final .label { font-size: 16px; } .btn-devis.hero-final .arrow { width: 42px; height: 42px; } }
  `,
})
export class Cta {}
