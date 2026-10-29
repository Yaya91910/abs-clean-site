import { ChangeDetectionStrategy, Component } from '@angular/core';

/** Visuel comparatif illustratif, en attendant les photos réelles (À FOURNIR). */
@Component({
  selector: 'abs-avant-apres',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Résultat</span>
          <h2 class="title">Visible immédiatement</h2>
        </div>
        <div class="glass cadre" role="img" aria-label="Comparaison avant / après le nettoyage d'une vitre">
          <div class="volets">
            <div class="avant">
              @for (opacite of carreauxSales; track $index) {
                <div class="carreau" [style.opacity]="opacite"></div>
              }
              <span class="etiquette sombre">Avant</span>
            </div>
            <div class="apres">
              <div class="reflet large"></div>
              <div class="reflet fin"></div>
              <span class="etiquette accent">Après</span>
            </div>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: `
    .cadre { width: 100%; padding: 10px; }
    .volets { display: flex; flex-wrap: wrap; gap: 6px; border-radius: 18px; overflow: hidden; }
    .avant, .apres { flex: 1 1 280px; min-height: 220px; position: relative; }
    .avant { display: grid; grid-template-columns: repeat(6, minmax(0, 1fr)); grid-template-rows: repeat(4, minmax(0, 1fr)); gap: 3px; padding: 3px; background: #CBD3DD; }
    .carreau { background: #9AA6B5; }
    .apres { background: linear-gradient(135deg, #EEF4FF, #FFFFFF); overflow: hidden; }
    .reflet { position: absolute; height: 140%; top: -20%; transform: rotate(18deg); }
    .reflet.large { width: 60px; left: 28%; background: rgba(255, 255, 255, 0.85); }
    .reflet.fin { width: 40px; left: 54%; background: rgba(255, 255, 255, 0.6); }
    .etiquette { position: absolute; top: 14px; left: 14px; padding: 6px 14px; color: #FFFFFF; border-radius: 999px; font-size: 12px; font-weight: 600; }
    .etiquette.sombre { background: rgba(21, 52, 95, 0.8); }
    .etiquette.accent { background: var(--accent); }
  `,
})
export class AvantApres {
  protected readonly carreauxSales = Array.from({ length: 24 }, (_, i) => 0.25 + ((i * 37) % 60) / 100);
}
