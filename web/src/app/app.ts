import { ChangeDetectionStrategy, Component, DOCUMENT, afterNextRender, inject } from '@angular/core';
import { Header } from './sections/header';
import { Hero } from './sections/hero';
import { APropos } from './sections/a-propos';
import { AvantApres } from './sections/avant-apres';
import { Prestations } from './sections/prestations';
import { Tarifs } from './sections/tarifs';
import { Processus } from './sections/processus';
import { Faq } from './sections/faq';
import { Devis } from './sections/devis';
import { Cta } from './sections/cta';
import { Parrainage } from './sections/parrainage';
import { Reseaux } from './sections/reseaux';
import { Footer } from './sections/footer';
import { AdminIntervention } from './admin/intervention';
import { TarifsApi } from './core/tarifs-api';

@Component({
  selector: 'abs-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Header, Hero, APropos, AvantApres, Prestations, Tarifs, Processus, Faq, Devis, Cta, Parrainage, Reseaux, Footer, AdminIntervention],
  styleUrl: './app.scss',
  template: `
    @if (pageAdmin) {
      <abs-admin-intervention />
    } @else {
      <div class="page">
        <div class="dots" aria-hidden="true"></div>
        <div class="halo a" aria-hidden="true"></div>
        <div class="halo b" aria-hidden="true"></div>
        <div class="halo c" aria-hidden="true"></div>

        <div class="content">
          <abs-header />
          <main>
            <abs-hero />
            <abs-a-propos />
            <abs-avant-apres />
            <abs-prestations />
            <abs-tarifs />
            <abs-processus />
            <abs-faq />
            <abs-devis />
            <abs-cta />
            <abs-parrainage />
            <abs-reseaux />
          </main>
          <abs-footer />
        </div>
      </div>
    }
  `,
})
export class App {
  private readonly document = inject(DOCUMENT);
  /** Lien de validation du gérant (e-mail) : page dédiée, hors du site vitrine. */
  protected readonly pageAdmin = this.document.location.pathname === '/admin/intervention';

  constructor() {
    // Lien de parrainage https://{domaine}/devis?parrain=CODE : on amène directement au formulaire,
    // une fois les tarifs affichés (sinon leur arrivée repousse le formulaire plus bas).
    if (this.document.location.pathname !== '/devis') return;
    const tarifs = inject(TarifsApi);
    afterNextRender(() => {
      const defiler = () => requestAnimationFrame(() => this.document.getElementById('devis')?.scrollIntoView());
      tarifs.grille().subscribe({ next: defiler, error: defiler });
    });
  }
}
