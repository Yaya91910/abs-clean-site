import AxeBuilder from '@axe-core/playwright';
import { expect, Page, test } from '@playwright/test';
import { clientFictif, remplirCoordonnees } from './outils';

/**
 * Accessibilité (CLAUDE.md §14, contrastes AA) sur les états que Lighthouse ne voit pas :
 * fenêtre de coordonnées, erreurs de saisie, écran de succès, page du gérant.
 */
async function verifier(page: Page, zone?: string) {
  let analyse = new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']);
  if (zone) analyse = analyse.include(zone);
  const { violations } = await analyse.analyze();
  expect(violations.map((v) => `${v.id} : ${v.nodes.map((n) => n.target.join(' ')).join(', ')}`)).toEqual([]);
}

test.use({ viewport: { width: 390, height: 844 } });

test('page d’accueil', async ({ page }) => {
  await page.goto('/');
  await expect(page.locator('#tarifs').getByText('Baie vitrée')).toBeVisible();
  await verifier(page);
});

test('fenêtre de coordonnées : erreurs puis écran de succès', async ({ page }) => {
  await page.goto('/');
  const devis = page.locator('#devis');
  await devis.getByRole('button', { name: 'Ajouter : Vitres' }).click();
  await devis.getByRole('button', { name: 'Valider mon devis' }).click();
  const fenetre = page.getByRole('dialog');

  await fenetre.getByRole('button', { name: 'Envoyer ma demande' }).click();
  await expect(fenetre.getByText('Le nom est requis.')).toBeVisible();
  await verifier(page, 'dialog');

  await remplirCoordonnees(page, clientFictif());
  await fenetre.getByLabel(/J'accepte que ABS clean utilise ces informations/).check();
  await fenetre.getByRole('button', { name: 'Envoyer ma demande' }).click();
  await expect(fenetre.getByRole('heading', { name: 'Demande envoyée !' })).toBeVisible();
  await verifier(page, 'dialog');
});

for (const adresse of ['/mentions-legales', '/confidentialite', '/parrainage/conditions', '/admin/intervention?demande=1&jeton=x']) {
  test(`page ${adresse}`, async ({ page }) => {
    await page.goto(adresse);
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
    await verifier(page);
  });
}
