import { expect, test } from '@playwright/test';
import { attendreSansDebordement, clientFictif, remplirCoordonnees } from './outils';

/** Parcours complet (CLAUDE.md §15) aux quatre largeurs exigées. */
for (const largeur of [1440, 820, 390, 340]) {
  test.describe(`${largeur} px`, () => {
    test.use({ viewport: { width: largeur, height: largeur > 800 ? 900 : 760 } });

    test('demande de devis de bout en bout', async ({ page }) => {
      await page.goto('/');
      await expect(page.getByRole('heading', { level: 1 })).toContainText('La transparence');
      await expect(page.locator('#tarifs').getByText('Baie vitrée')).toBeVisible();
      await attendreSansDebordement(page);

      // Compteurs et estimation en direct.
      const devis = page.locator('#devis');
      await devis.scrollIntoViewIfNeeded();
      await devis.getByRole('button', { name: 'Mensuel' }).click();
      await devis.getByRole('button', { name: 'Ajouter : Vitres' }).click({ clickCount: 3 });
      await devis.getByRole('button', { name: 'Retirer : Vitres' }).click();
      await devis.getByRole('button', { name: 'Ajouter : Baies vitrées' }).click();
      await devis.getByRole('button', { name: 'Ajouter : Velux' }).click();
      await expect(devis.locator('.estimate-val')).toHaveText('À partir de 105 €');
      await expect(devis.getByText("Formule Mensuel : la réduction n'est pas encore incluse")).toBeVisible();
      await devis.getByLabel('Localisation').fill('91000');

      // Fenêtre de coordonnées : centrée, titre et bouton d'envoi visibles.
      await devis.getByRole('button', { name: 'Valider mon devis' }).click();
      const fenetre = page.getByRole('dialog');
      await expect(fenetre).toBeVisible();
      await expect(fenetre.getByRole('heading', { name: 'Vos coordonnées' })).toBeInViewport();
      await expect(fenetre.getByRole('button', { name: 'Envoyer ma demande' })).toBeInViewport();
      await expect(fenetre.getByLabel('Code postal')).toHaveValue('91000');
      await attendreSansDebordement(page);

      // Validation : envoi à vide refusé, erreurs champ par champ.
      await fenetre.getByRole('button', { name: 'Envoyer ma demande' }).click();
      await expect(fenetre.getByText('Le nom est requis.')).toBeVisible();
      await expect(fenetre.getByText('Votre accord est nécessaire')).toBeVisible();

      // Mauvais numéro, puis correction.
      const client = clientFictif();
      await remplirCoordonnees(page, client);
      await fenetre.getByLabel('Téléphone').fill('08 12 34 56 78');
      await fenetre.getByRole('button', { name: 'Samedi' }).click();
      await fenetre.getByLabel(/J'accepte que ABS clean utilise ces informations/).check();
      await fenetre.getByRole('button', { name: 'Envoyer ma demande' }).click();
      await expect(fenetre.getByText('Numéro de téléphone français invalide')).toBeVisible();
      await fenetre.getByLabel('Téléphone').fill(client.telephone);
      await fenetre.getByRole('button', { name: 'Envoyer ma demande' }).click();

      // Écran de succès avec le code parrain et son QR code.
      await expect(fenetre.getByRole('heading', { name: 'Demande envoyée !' })).toBeVisible();
      await expect(fenetre.getByText('Votre devis en PDF vient de partir dans votre boîte mail.')).toBeVisible();
      await expect(fenetre.locator('.ref-code')).toHaveText(/^ABS-[1-9][0-9]{6}$/);
      const qr = fenetre.getByAltText('QR code de votre lien de parrainage');
      await expect(qr).toBeVisible();
      await expect.poll(() => qr.evaluate((img: HTMLImageElement) => img.naturalWidth), { message: 'QR code chargé' }).toBeGreaterThan(0);
      await attendreSansDebordement(page);

      await fenetre.getByRole('button', { name: 'Fermer' }).last().click();
      await expect(fenetre).toBeHidden();
    });
  });
}
