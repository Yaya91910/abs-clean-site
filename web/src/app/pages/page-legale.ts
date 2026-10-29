import { ChangeDetectionStrategy, Component, inject, input, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Header } from '../sections/header';
import { Footer } from '../sections/footer';

/** Gabarit commun des pages légales : en-tête, carte de texte, pied de page. */
@Component({
  selector: 'abs-page-legale',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Header, Footer],
  template: `
    <div class="page-legale">
      <abs-header />
      <main class="section">
        <article class="glass texte">
          <a href="/" class="retour">← Retour au site</a>
          <h1>{{ titre() }}</h1>
          @if (miseAJour()) {
            <p class="maj">Dernière mise à jour : {{ miseAJour() }}</p>
          }
          <ng-content />
        </article>
      </main>
      <abs-footer />
    </div>
  `,
  styles: `
    .page-legale { min-height: 100vh; background: linear-gradient(180deg, #F4F8FF 0%, #EAF2FF 100%); }
    .texte { width: 100%; max-width: 820px; padding: clamp(24px, 5vw, 48px); text-align: left; line-height: 1.65; color: #3C4A5C; }
    .retour { display: inline-block; font-size: 14px; margin-bottom: 18px; }
    h1 { font-size: clamp(28px, 5vw, 38px); color: var(--ink); line-height: 1.15; }
    .maj { font-size: 13px; color: var(--text-soft); margin: 8px 0 0; }
    :host ::ng-deep h2 { font-size: 20px; color: var(--ink); margin: 32px 0 10px; letter-spacing: -0.02em; }
    :host ::ng-deep p, :host ::ng-deep li { font-size: 15px; overflow-wrap: anywhere; }
    :host ::ng-deep ul { padding-left: 20px; margin: 8px 0; }
    :host ::ng-deep dl { display: grid; grid-template-columns: minmax(0, 14em) minmax(0, 1fr); gap: 6px 16px; margin: 8px 0; }
    :host ::ng-deep dt { font-weight: 600; color: var(--ink); }
    :host ::ng-deep dd { margin: 0; overflow-wrap: anywhere; }
    @media (max-width: 520px) { :host ::ng-deep dl { grid-template-columns: 1fr; gap: 2px; } :host ::ng-deep dd { margin-bottom: 8px; } }
  `,
})
export class PageLegale implements OnInit {
  readonly titre = input.required<string>();
  readonly miseAJour = input<string>();
  private readonly title = inject(Title);

  ngOnInit(): void {
    this.title.setTitle(`${this.titre()} — ABS clean`);
  }
}
