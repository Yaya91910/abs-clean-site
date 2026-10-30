import { ChangeDetectionStrategy, Component } from '@angular/core';

/** Bandeau qui défile en continu sous le héros (surfaces traitées, clients, zone). Immobile si les animations sont réduites. */
@Component({
  selector: 'abs-defilant',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="bandeau">
      <div class="piste">
        @for (copie of [0, 1]; track copie) {
          <ul [attr.aria-hidden]="copie === 1 ? 'true' : null" [class.copie]="copie === 1">
            @for (mot of mots; track mot) {
              <li><span class="point" aria-hidden="true"></span>{{ mot }}</li>
            }
          </ul>
        }
      </div>
    </div>
  `,
  styles: `
    :host { display: block; }
    .bandeau {
      position: relative; overflow: hidden; padding: 18px 0;
      border-block: 1px solid rgba(255, 255, 255, 0.9);
      background: rgba(255, 255, 255, 0.45); backdrop-filter: blur(12px) saturate(150%); -webkit-backdrop-filter: blur(12px) saturate(150%);
      -webkit-mask-image: linear-gradient(90deg, transparent, #000 10%, #000 90%, transparent);
      mask-image: linear-gradient(90deg, transparent, #000 10%, #000 90%, transparent);
    }
    .piste { display: flex; width: max-content; animation: defiler 42s linear infinite; }
    .bandeau:hover .piste { animation-play-state: paused; }
    ul { list-style: none; margin: 0; padding: 0; display: flex; }
    li { display: inline-flex; align-items: center; gap: 14px; padding: 0 22px; font-size: 15px; font-weight: 600; color: var(--ink); white-space: nowrap; letter-spacing: -0.01em; }
    .point { width: 6px; height: 6px; border-radius: 999px; background: var(--accent2); }
    @keyframes defiler { from { transform: translateX(0); } to { transform: translateX(-50%); } }
    @media (prefers-reduced-motion: reduce) {
      .piste { width: auto; justify-content: center; }
      ul { flex-wrap: wrap; justify-content: center; row-gap: 8px; }
      .copie { display: none; }
    }
  `,
})
export class Defilant {
  protected readonly mots = [
    'Vitres', 'Baies vitrées', 'Velux', 'Vitrines', 'Vérandas', 'Verrières',
    'Particuliers', 'Commerces', 'Professionnels', "Jusqu'à 2,50 m", 'Essonne', 'Île-de-France',
  ];
}
