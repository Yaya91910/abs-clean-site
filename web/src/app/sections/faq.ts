import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { Fleche } from '../shared/fleche';

/** Accordéon : une seule question ouverte à la fois (CAHIER-DES-CHARGES.md §6). */
@Component({
  selector: 'abs-faq',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Fleche],
  template: `
    <section id="faq" class="section">
      <div class="container">
        <div class="head">
          <span class="kicker">FAQ</span>
          <h2 class="title">Vos questions, nos réponses</h2>
          <p class="sub">Tout ce qu'il faut savoir avant de confier vos vitres à ABS clean.</p>
        </div>
        <div class="faq-list">
          @for (item of questions; track item.q; let i = $index) {
            <div class="glass faq-item" [class.ouvert]="ouverte() === i">
              <h3>
                <button type="button" class="faq-q" [id]="'faq-q-' + i" [attr.aria-expanded]="ouverte() === i" [attr.aria-controls]="'faq-r-' + i" (click)="basculer(i)">
                  <span class="faq-num" aria-hidden="true">{{ (i + 1).toString().padStart(2, '0') }}</span>
                  <span class="faq-qtext">{{ item.q }}</span>
                  <span class="faq-chev" aria-hidden="true">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M6 9l6 6 6-6" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round" /></svg>
                  </span>
                </button>
              </h3>
              <div class="faq-a" [id]="'faq-r-' + i" role="region" [attr.aria-labelledby]="'faq-q-' + i" [attr.inert]="ouverte() === i ? null : ''">
                <div class="faq-a-inner"><p>{{ item.a }}</p></div>
              </div>
            </div>
          }
        </div>
        <div class="faq-cta">
          <span>Une autre question ? Le plus simple, c'est d'essayer :</span>
          <a href="#devis" class="btn-devis"><span class="label">Calculer mon devis gratuit</span><span class="arrow"><abs-fleche /></span></a>
        </div>
      </div>
    </section>
  `,
  styles: `
    .faq-list { width: 100%; max-width: 760px; display: flex; flex-direction: column; gap: 12px; }
    .faq-item { border-radius: 20px; overflow: hidden; transition: box-shadow .2s ease; text-align: left; }
    .faq-item:hover { box-shadow: 0 26px 52px -30px rgba(21, 52, 95, 0.38); }
    .faq-item.ouvert { box-shadow: 0 0 0 2px rgba(61, 108, 181, 0.35), 0 26px 52px -30px rgba(21, 52, 95, 0.4); }
    h3 { font-size: inherit; letter-spacing: normal; }
    .faq-q { width: 100%; display: flex; align-items: center; gap: 14px; padding: 18px 18px 18px 20px; background: transparent; border: none; cursor: pointer; font-family: inherit; text-align: left; color: var(--ink); }
    .faq-num { width: 34px; height: 34px; border-radius: 11px; background: linear-gradient(135deg, var(--accent2), var(--accent)); color: #FFFFFF; font-size: 13px; font-weight: 700; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .faq-qtext { flex: 1; font-size: 16px; font-weight: 600; line-height: 1.4; }
    .faq-chev { width: 34px; height: 34px; border-radius: 999px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; background: #EFF6FF; color: var(--accent); transition: transform .25s ease, background .25s ease; }
    .ouvert .faq-chev { transform: rotate(180deg); background: var(--accent); color: #FFFFFF; }
    .faq-a { display: grid; grid-template-rows: 0fr; transition: grid-template-rows .3s ease; }
    .ouvert .faq-a { grid-template-rows: 1fr; }
    .faq-a-inner { overflow: hidden; }
    .faq-a-inner p { margin: 0; padding: 0 22px 20px 68px; font-size: 15px; line-height: 1.65; color: #4A5A6E; }
    @media (max-width: 520px) { .faq-q { padding: 16px 14px; gap: 11px; } .faq-qtext { font-size: 15px; } .faq-a-inner p { padding: 0 18px 18px 18px; font-size: 14px; } }
    .faq-cta { display: flex; flex-direction: column; align-items: center; gap: 14px; margin-top: 6px; font-size: 15px; color: var(--text-muted); }
  `,
})
export class Faq {
  protected readonly ouverte = signal<number>(0);

  protected readonly questions = [
    { q: 'Le devis est-il vraiment gratuit ?', a: 'Oui, totalement gratuit et sans engagement. Vous obtenez votre estimation en direct sur le site, puis votre devis détaillé en PDF par e-mail. Vous décidez ensuite, sans aucune pression.' },
    { q: 'Dans quelles zones intervenez-vous ?', a: 'ABS clean intervient en Essonne et en Île-de-France. Indiquez votre ville dans le formulaire de devis : nous vous confirmons rapidement notre passage.' },
    { q: 'Comment est calculé le prix ?', a: "Le tarif dépend du nombre de vitres, de baies vitrées et de velux, de votre profil et de la fréquence choisie : à partir de 20 € la vitre, 40 € la baie vitrée et 25 € le velux. Pas de surprise : l'estimation est confirmée avec vous avant l'intervention." },
    { q: 'Proposez-vous des formules régulières ?', a: "Oui. En plus des interventions ponctuelles, nous proposons des passages mensuels ou trimestriels, avec des réductions selon la formule. Vos vitres restent impeccables toute l'année, sans y penser." },
    { q: 'Intervenez-vous pour les professionnels ?', a: "Absolument : bureaux, locaux professionnels, restaurants, cabinets, agences, halls d'accueil, vitrines et devantures. Une vitrine propre, c'est une meilleure image auprès de vos clients." },
    { q: "Jusqu'à quelle hauteur intervenez-vous ?", a: "Nous intervenons sur toutes les surfaces vitrées jusqu'à 2,50 m de hauteur : fenêtres, baies vitrées, vérandas, verrières et fenêtres de toit." },
    { q: 'Nettoyez-vous aussi les encadrements et les rails ?', a: "Oui, en option : encadrements, rails et rebords. C'est ce qui fait la différence entre une vitre lavée et une fenêtre vraiment impeccable." },
    { q: 'Pourquoi choisir ABS clean ?', a: 'Pour trois engagements : ponctualité, disponibilité et discrétion. Notre priorité est simple : un résultat impeccable et un service fiable, à chaque intervention.' },
  ];

  protected basculer(i: number): void {
    this.ouverte.update((actuelle) => (actuelle === i ? -1 : i));
  }
}
