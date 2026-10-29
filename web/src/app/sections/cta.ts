import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-cta',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="section cta">
      <div class="band">
        <h2>Prêt pour des vitres impeccables ?</h2>
        <a href="#devis" class="btn-devis light"><span class="label">Obtenir mon devis gratuit</span></a>
      </div>
    </section>
  `,
  styles: `
    .cta { padding-top: 20px; }
    .band {
      width: 100%; max-width: 1000px; padding: 48px 24px; border-radius: 28px;
      display: flex; flex-direction: column; align-items: center; gap: 24px;
      background: linear-gradient(135deg, var(--ink) 0%, #1F55B5 100%);
    }
    h2 { font-size: clamp(28px, 4.5vw, 42px); color: #FFFFFF; max-width: 560px; line-height: 1.12; }
    .btn-devis.light { background: #FFFFFF; color: var(--ink); padding: 0 28px; }
  `,
})
export class Cta {}
