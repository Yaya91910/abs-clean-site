import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'abs-footer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <footer class="footer">
      <img src="abs-clean-logo.svg" alt="ABS clean" width="97" height="48" />
      <p>Nettoyage de surfaces vitrées · Essonne et Île-de-France</p>
      <!-- Contact (téléphone, e-mail, SIRET, adresse) : À FOURNIR par le gérant. -->
      <!-- Liens Mentions légales / Confidentialité : pages à créer (contenus À FOURNIR). -->
      <p class="copy">© {{ annee }} ABS clean</p>
    </footer>
  `,
  styles: `
    .footer { padding: 40px 20px 32px; display: flex; flex-direction: column; align-items: center; gap: 8px; text-align: center; color: var(--text-muted); font-size: 14px; }
    img { height: 48px; width: auto; }
    p { margin: 0; }
    .copy { font-size: 12px; }
  `,
})
export class Footer {
  protected readonly annee = new Date().getFullYear();
}
