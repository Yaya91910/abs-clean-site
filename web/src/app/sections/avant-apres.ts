import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

/**
 * Comparaison avant / après à faire glisser (souris, doigt ou flèches du clavier).
 * Visuel illustratif, en attendant les photos réelles (À FOURNIR).
 */
@Component({
  selector: 'abs-avant-apres',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Résultat</span>
          <h2 class="title">Visible immédiatement</h2>
          <p class="sub">Faites glisser le curseur pour comparer.</p>
        </div>
        <div class="glass cadre">
          <div class="comparaison" [style.--pos]="position() + '%'">
            <div class="calque avant" aria-hidden="true">
              @for (opacite of carreauxSales; track $index) {
                <div class="carreau" [style.opacity]="opacite"></div>
              }
            </div>
            <div class="calque apres" aria-hidden="true">
              <div class="reflet large"></div>
              <div class="reflet fin"></div>
            </div>
            <span class="etiquette sombre" aria-hidden="true">Avant</span>
            <span class="etiquette accent" aria-hidden="true">Après</span>
            <div class="poignee" aria-hidden="true">
              <span class="bouton">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none"><path d="M9 6l-6 6 6 6M15 6l6 6-6 6" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" /></svg>
              </span>
            </div>
            <input type="range" min="0" max="100" step="1" [value]="position()" (input)="position.set(+$any($event.target).value)"
              aria-label="Comparer avant et après le nettoyage : part de la vitre nettoyée" [attr.aria-valuetext]="position() + ' % nettoyé'" />
          </div>
        </div>
      </div>
    </section>
  `,
  styles: `
    .cadre { width: 100%; padding: 10px; }
    .comparaison { --pos: 50%; position: relative; height: clamp(240px, 42vw, 420px); border-radius: 18px; overflow: hidden; user-select: none; touch-action: pan-y; }
    .calque { position: absolute; inset: 0; }
    .avant { display: grid; grid-template-columns: repeat(8, minmax(0, 1fr)); grid-template-rows: repeat(5, minmax(0, 1fr)); gap: 3px; padding: 3px; background: #CBD3DD; }
    .carreau { background: #9AA6B5; }
    /* La partie « après » recouvre la partie « avant » à gauche du curseur. */
    .apres { background: linear-gradient(135deg, #EEF4FF, #FFFFFF); clip-path: inset(0 calc(100% - var(--pos)) 0 0); }
    .reflet { position: absolute; height: 140%; top: -20%; transform: rotate(18deg); }
    .reflet.large { width: 70px; left: 22%; background: rgba(255, 255, 255, 0.9); }
    .reflet.fin { width: 40px; left: 48%; background: rgba(255, 255, 255, 0.65); }
    .etiquette { position: absolute; top: 14px; padding: 6px 14px; color: #FFFFFF; border-radius: 999px; font-size: 12px; font-weight: 600; backdrop-filter: blur(8px); -webkit-backdrop-filter: blur(8px); }
    .etiquette.accent { left: 14px; background: var(--accent); }
    .etiquette.sombre { right: 14px; background: rgba(21, 52, 95, 0.8); }
    .poignee { position: absolute; top: 0; bottom: 0; left: var(--pos); width: 3px; margin-left: -1.5px; background: #FFFFFF; box-shadow: 0 0 18px rgba(21, 52, 95, 0.35); pointer-events: none; }
    .bouton {
      position: absolute; top: 50%; left: 50%; width: 48px; height: 48px; margin: -24px 0 0 -24px; border-radius: 999px;
      display: flex; align-items: center; justify-content: center; color: var(--accent);
      background: rgba(255, 255, 255, 0.75); backdrop-filter: blur(10px) saturate(160%); -webkit-backdrop-filter: blur(10px) saturate(160%);
      border: 1px solid #FFFFFF; box-shadow: 0 10px 26px -10px rgba(21, 52, 95, 0.5); transition: transform .2s ease;
    }
    .comparaison:hover .bouton { transform: scale(1.08); }
    /* Curseur natif invisible par-dessus tout : souris, doigt et clavier gratuits et accessibles. */
    input { position: absolute; inset: 0; width: 100%; height: 100%; margin: 0; opacity: 0; cursor: ew-resize; }
    .comparaison:has(input:focus-visible) .bouton { outline: 3px solid var(--accent); outline-offset: 3px; }
  `,
})
export class AvantApres {
  protected readonly carreauxSales = Array.from({ length: 40 }, (_, i) => 0.25 + ((i * 37) % 60) / 100);
  protected readonly position = signal(50);
}
