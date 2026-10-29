import { ChangeDetectionStrategy, Component } from '@angular/core';
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

@Component({
  selector: 'abs-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Header, Hero, APropos, AvantApres, Prestations, Tarifs, Processus, Faq, Devis, Cta, Parrainage, Reseaux, Footer],
  styleUrl: './app.scss',
  template: `
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
  `,
})
export class App {}
