import { ChangeDetectionStrategy, Component, DOCUMENT, afterNextRender, inject } from '@angular/core';
import { Header } from './sections/header';
import { Hero } from './sections/hero';
import { Defilant } from './sections/defilant';
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
import { activerEffets } from './shared/effets';
import { MentionsLegales } from './pages/mentions-legales';
import { Confidentialite } from './pages/confidentialite';
import { ConditionsParrainage } from './pages/conditions-parrainage';

/** Pages hors du site vitrine : pas d'effets d'animation. */
const PAGES_A_PART = ['/admin/intervention', '/mentions-legales', '/confidentialite', '/parrainage/conditions'];

@Component({
  selector: 'abs-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Header, Hero, Defilant, APropos, AvantApres, Prestations, Tarifs, Processus, Faq, Devis, Cta, Parrainage, Reseaux, Footer, AdminIntervention, MentionsLegales, Confidentialite, ConditionsParrainage],
  styleUrl: './app.scss',
  template: `
    @switch (page) {
      @case ('/admin/intervention') {
        <abs-admin-intervention />
      }
      @case ('/mentions-legales') {
        <abs-mentions-legales />
      }
      @case ('/confidentialite') {
        <abs-confidentialite />
      }
      @case ('/parrainage/conditions') {
        <abs-conditions-parrainage />
      }
      @default {
        <div class="page">
          <div class="dots" aria-hidden="true"></div>
          <div class="halo a" aria-hidden="true"></div>
          <div class="halo b" aria-hidden="true"></div>
          <div class="halo c" aria-hidden="true"></div>

          <div class="content">
            <abs-header />
            <main>
              <!-- Ordre pensé pour la conversion : promesse → 3 étapes → offre → prix → devis, puis réassurance. -->
              <abs-hero />
              <abs-defilant />
              <abs-processus />
              <abs-prestations />
              <abs-tarifs />
              <abs-devis />
              <abs-avant-apres />
              <abs-a-propos />
              <abs-faq />
              <abs-parrainage />
              <abs-cta />
              <abs-reseaux />
            </main>
            <abs-footer />
          </div>
        </div>
      }
    }
  `,
})
export class App {
  private readonly document = inject(DOCUMENT);
  /** Pages hors du site vitrine : validation du gérant (lien de son e-mail) et pages légales. */
  protected readonly page = this.document.location.pathname.replace(/\/+$/, '') || '/';

  constructor() {
    if (!PAGES_A_PART.includes(this.page)) afterNextRender(() => activerEffets(this.document));

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
