import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Logo } from '../shared/logo';

@Component({
  selector: 'abs-footer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Logo],
  template: `
    <footer class="footer">
      <abs-logo [hauteur]="72" />
      <p class="zone">Essonne — nettoyage et entretien des surfaces vitrées, jusqu'à 2,50 m de hauteur.</p>
      <nav aria-label="Liens du pied de page" class="liens">
        <a href="#qui-sommes-nous">À propos</a>
        <a href="#prestations">Prestations</a>
        <a href="#tarifs">Tarifs</a>
        <a href="#faq">FAQ</a>
        <a href="#parrainage">Parrainage</a>
        <a href="#devis">Devis</a>
      </nav>
      <!-- Téléphone et e-mail : À FOURNIR par le gérant (CAHIER-DES-CHARGES.md §18). -->
      <p class="contact">[Téléphone] · [Adresse e-mail]</p>
      <!-- Mentions légales et politique de confidentialité : pages à créer (contenus À FOURNIR). -->
      <p class="copy">© {{ annee }} ABS clean. Tous droits réservés.</p>
    </footer>
  `,
  styles: `
    .footer { width: 100%; padding: 36px 20px 40px; display: flex; flex-direction: column; align-items: center; gap: 20px; text-align: center; }
    p { margin: 0; }
    .zone { font-size: 13px; color: #6B7A8D; max-width: 400px; line-height: 1.5; }
    .liens { display: flex; flex-wrap: wrap; justify-content: center; gap: 10px 22px; }
    .liens a, .contact { font-size: 13px; color: #4A5A6E; }
    .liens a:hover { color: var(--accent); }
    .copy { font-size: 12px; color: #66778C; }
  `,
})
export class Footer {
  protected readonly annee = new Date().getFullYear();
}
