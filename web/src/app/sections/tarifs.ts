import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith } from 'rxjs';
import { GrilleTarifs, TarifsApi } from '../core/tarifs-api';

type EtatTarifs = { statut: 'chargement' } | { statut: 'ok'; grille: GrilleTarifs } | { statut: 'erreur' };

const LIBELLES_FREQUENCE: Record<string, string> = {
  ponctuel: 'Ponctuel',
  mensuel: 'Mensuel',
  trimestriel: 'Trimestriel',
};

@Component({
  selector: 'abs-tarifs',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section id="tarifs" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Tarifs</span>
          <h2 class="title">Des prix clairs et <span class="grad-text">transparents</span></h2>
        </div>

        @switch (etat().statut) {
          @case ('chargement') {
            <p class="a-venir">Chargement des tarifs…</p>
          }
          @case ('erreur') {
            <p class="a-venir" role="alert">Les tarifs sont momentanément indisponibles.</p>
          }
          @case ('ok') {
            @if (grille(); as g) {
              <div class="grid-4">
                @for (element of g.elements; track element.code) {
                  <article class="glass card price-card">
                    <h3>{{ element.libelle }}</h3>
                    <p class="price">
                      @if (element.aPartirDe) {
                        <span class="from">à partir de</span>
                      }
                      {{ element.prixUnitaire }} €
                    </p>
                  </article>
                }
              </div>
              <div class="band">
                <h3>Formules régulières</h3>
                <ul class="chips">
                  @for (f of g.frequences; track f.code) {
                    <li>{{ libelleFrequence(f.code) }}</li>
                  }
                </ul>
                @if (reductionsInconnues()) {
                  <p class="note">Réductions des formules régulières communiquées prochainement : aucune réduction n'est appliquée à l'estimation.</p>
                }
              </div>
              <p class="a-venir">{{ g.mentionEstimation }} · {{ g.mentionOption }}</p>
            }
          }
        }
      </div>
    </section>
  `,
  styles: `
    .price-card h3 { font-size: 18px; }
    .price { margin: 0; font-size: 30px; font-weight: 800; color: var(--accent); letter-spacing: -0.02em; display: flex; flex-direction: column; align-items: center; }
    .from { font-size: 12px; font-weight: 500; color: var(--text-muted); letter-spacing: 0; }
    .band { width: 100%; padding: 28px 24px; border-radius: 24px; background: linear-gradient(135deg, var(--ink), #1F55B5); color: #FFFFFF; display: flex; flex-direction: column; align-items: center; gap: 12px; }
    .band h3 { font-size: clamp(22px, 3vw, 28px); }
    .chips { list-style: none; margin: 0; padding: 0; display: flex; flex-wrap: wrap; justify-content: center; gap: 8px; }
    .chips li { padding: 7px 14px; border-radius: 999px; font-size: 13px; font-weight: 600; color: #DBEAFE; background: rgba(255, 255, 255, 0.1); border: 1px solid rgba(255, 255, 255, 0.18); }
    .note { margin: 0; font-size: 13px; color: rgba(255, 255, 255, 0.8); max-width: 480px; line-height: 1.5; }
  `,
})
export class Tarifs {
  protected readonly etat = toSignal(
    inject(TarifsApi).grille().pipe(
      map((grille): EtatTarifs => ({ statut: 'ok', grille })),
      startWith<EtatTarifs>({ statut: 'chargement' }),
      catchError(() => of<EtatTarifs>({ statut: 'erreur' })),
    ),
    { requireSync: true },
  );

  protected grille(): GrilleTarifs | null {
    const etat = this.etat();
    return etat.statut === 'ok' ? etat.grille : null;
  }

  protected reductionsInconnues(): boolean {
    return this.grille()?.frequences.some((f) => !f.reductionConnue) ?? false;
  }

  protected libelleFrequence(code: string): string {
    return LIBELLES_FREQUENCE[code] ?? code;
  }
}
