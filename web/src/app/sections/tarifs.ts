import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith } from 'rxjs';
import { ElementTarif, GrilleTarifs, TarifsApi } from '../core/tarifs-api';
import { Fleche } from '../shared/fleche';

type EtatTarifs = { statut: 'chargement' } | { statut: 'ok'; grille: GrilleTarifs } | { statut: 'erreur' };

/** Textes d'affichage par élément ; les prix, eux, viennent de l'API. */
const PRESENTATION: Record<string, { unite: string; note: string }> = {
  vitre: { unite: 'par vitre', note: 'Fenêtres standard' },
  baie: { unite: 'selon la taille', note: 'Baies et portes-fenêtres' },
  velux: { unite: 'par velux', note: 'Fenêtres de toit' },
  vitrine: { unite: 'selon la taille', note: 'Commerces et devantures' },
};

@Component({
  selector: 'abs-tarifs',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Fleche],
  template: `
    <section id="tarifs" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">Tarifs</span>
          <h2 class="title">Des prix clairs et <span class="grad-text">transparents</span></h2>
          <p class="sub">Un tarif simple par élément. Votre estimation exacte s'affiche en direct dans le formulaire de devis.</p>
        </div>

        @switch (etat().statut) {
          @case ('chargement') {
            <p class="info">Chargement des tarifs…</p>
          }
          @case ('erreur') {
            <p class="info" role="alert">Les tarifs sont momentanément indisponibles.</p>
          }
          @case ('ok') {
            <div class="price-grid">
              @for (element of elements(); track element.code) {
                <article class="glass price-card">
                  <div class="price-top">
                    <div class="icon-box" aria-hidden="true">
                      @switch (element.code) {
                        @case ('vitre') {
                          <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><rect x="5" y="3" width="14" height="18" rx="1.5" stroke="currentColor" stroke-width="1.5" /><path d="M12 3v18M5 12h14" stroke="currentColor" stroke-width="1.5" /></svg>
                        }
                        @case ('baie') {
                          <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><rect x="2.5" y="5" width="19" height="14" rx="1.5" stroke="currentColor" stroke-width="1.5" /><path d="M12 5v14" stroke="currentColor" stroke-width="1.5" /><path d="M9 11v2M15 11v2" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" /></svg>
                        }
                        @case ('velux') {
                          <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><path d="M3 17l9-11 9 11" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round" /><rect x="8.5" y="11" width="7" height="5" rx="0.8" stroke="currentColor" stroke-width="1.5" /></svg>
                        }
                        @default {
                          <svg width="24" height="24" viewBox="0 0 24 24" fill="none"><path d="M4 9l1.2-5h13.6L20 9M4 9v11h16V9M4 9c0 1.4 1.1 2.5 2.7 2.5S9.3 10.4 9.3 9c0 1.4 1.2 2.5 2.7 2.5s2.7-1.1 2.7-2.5c0 1.4 1.1 2.5 2.6 2.5S20 10.4 20 9" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" /><rect x="7" y="14" width="10" height="4" rx="0.8" stroke="currentColor" stroke-width="1.5" /></svg>
                        }
                      }
                    </div>
                    <h3 class="price-name">{{ element.libelle }}</h3>
                  </div>
                  <p class="price-mid">
                    <span class="price-pre">{{ element.aPartirDe ? 'à partir de' : "l'unité" }}</span>
                    <span class="price-amount">{{ element.prixUnitaire }}<span class="price-cur">€</span></span>
                    <span class="price-unit">{{ presentation(element).unite }}</span>
                  </p>
                  <span class="price-note">{{ presentation(element).note }}</span>
                </article>
              }
            </div>
          }
        }

        <div class="price-band">
          <div class="price-band-glow" aria-hidden="true"></div>
          <div class="price-band-text">
            <span class="price-band-kicker">Formules régulières</span>
            <h3 class="price-band-title">Plus vous passez souvent, <br />moins vous payez.</h3>
            <div class="price-chips">
              <span class="price-chip">Ponctuel</span>
              <span class="price-chip hot">Mensuel · réduit</span>
              <span class="price-chip hot">Trimestriel · réduit</span>
            </div>
          </div>
          <div class="price-band-cta">
            <a href="#devis" class="btn-devis light"><span class="label">Calculer mon prix</span><span class="arrow"><abs-fleche [taille]="18" couleur="#FFFFFF" /></span></a>
            <span class="price-band-small">Gratuit · sans engagement · option encadrements, rails et rebords sur devis</span>
          </div>
        </div>
      </div>
    </section>
  `,
  styles: `
    .container { gap: 32px; }
    .info { font-size: 14px; color: var(--text-muted); }

    .price-grid { width: 100%; display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 16px; }
    @media (max-width: 900px) { .price-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
    @media (max-width: 360px) { .price-grid { grid-template-columns: 1fr; } }
    .price-card { position: relative; overflow: hidden; padding: 26px 18px 22px; display: flex; flex-direction: column; align-items: center; gap: 16px; transition: transform .25s ease, box-shadow .25s ease; }
    .price-card::before { content: ""; position: absolute; top: 0; left: 0; right: 0; height: 4px; background: linear-gradient(90deg, var(--accent2), var(--accent)); }
    .price-card::after { content: ""; position: absolute; top: -60px; right: -60px; width: 140px; height: 140px; border-radius: 50%; background: radial-gradient(circle, rgba(76, 155, 224, 0.28), transparent 70%); pointer-events: none; transition: transform .4s ease; }
    .price-card:hover { transform: translateY(-6px); box-shadow: 0 34px 60px -30px rgba(21, 52, 95, 0.45), 0 0 0 2px rgba(47, 111, 214, 0.25); }
    .price-card:hover::after { transform: scale(1.5); }
    .price-top { display: flex; flex-direction: column; align-items: center; gap: 10px; }
    .price-name { font-size: 16px; font-weight: 700; letter-spacing: normal; }
    .price-mid { margin: 0; display: flex; flex-direction: column; align-items: center; gap: 2px; }
    .price-pre { font-size: 12px; font-weight: 600; color: var(--text-soft); text-transform: uppercase; letter-spacing: 0.08em; }
    .price-amount { font-size: clamp(42px, 6vw, 54px); font-weight: 800; color: var(--accent); background: linear-gradient(135deg, var(--accent), var(--accent2)); -webkit-background-clip: text; background-clip: text; -webkit-text-fill-color: transparent; letter-spacing: -0.04em; line-height: 1; display: inline-flex; align-items: flex-start; }
    .price-cur { font-size: 0.45em; margin: 0.18em 0 0 3px; letter-spacing: 0; }
    .price-unit { font-size: 13px; color: var(--text-muted); margin-top: 4px; }
    .price-note { font-size: 12px; font-weight: 500; color: var(--ink); padding: 6px 12px; border-radius: 999px; background: rgba(47, 111, 214, 0.08); }

    .price-band { position: relative; overflow: hidden; width: 100%; border-radius: 26px; padding: clamp(26px, 4vw, 38px) clamp(22px, 4vw, 42px); background: linear-gradient(135deg, var(--ink) 0%, #183A6A 60%, var(--accent) 130%); display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 24px; text-align: left; box-shadow: 0 30px 60px -30px rgba(21, 52, 95, 0.6); }
    .price-band-glow { position: absolute; top: -90px; right: 18%; width: 260px; height: 260px; border-radius: 50%; background: radial-gradient(circle, rgba(76, 155, 224, 0.55), transparent 70%); pointer-events: none; }
    .price-band-text { position: relative; display: flex; flex-direction: column; gap: 10px; flex: 1 1 340px; min-width: 0; }
    .price-band-kicker { font-size: 12px; font-weight: 700; letter-spacing: 0.1em; text-transform: uppercase; color: #A9D2F5; }
    .price-band-title { font-size: clamp(22px, 3vw, 28px); line-height: 1.2; color: #FFFFFF; }
    .price-chips { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 6px; }
    .price-chip { padding: 7px 14px; border-radius: 999px; font-size: 13px; font-weight: 600; color: #DBEAFE; background: rgba(255, 255, 255, 0.1); border: 1px solid rgba(255, 255, 255, 0.18); }
    .price-chip.hot { color: var(--ink); background: linear-gradient(135deg, #A9D2F5, #CFE4F8); border-color: transparent; }
    .price-band-cta { position: relative; display: flex; flex-direction: column; align-items: center; gap: 10px; flex: 0 1 auto; }
    .price-band-small { font-size: 12px; color: rgba(255, 255, 255, 0.7); text-align: center; max-width: 260px; line-height: 1.5; }
    @media (max-width: 480px) { .price-chip { padding: 6px 11px; font-size: 12px; } .price-chips { gap: 6px; } .price-note { font-size: 11px; padding: 5px 10px; } .price-card { padding: 24px 12px 18px; } }
    @media (max-width: 640px) { .price-band { text-align: center; justify-content: center; } .price-band-text { align-items: center; } .price-chips { justify-content: center; } .price-band-title br { display: none; } }
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

  protected elements(): ElementTarif[] {
    const etat = this.etat();
    return etat.statut === 'ok' ? etat.grille.elements : [];
  }

  protected presentation(element: ElementTarif) {
    return PRESENTATION[element.code] ?? { unite: '', note: '' };
  }
}
