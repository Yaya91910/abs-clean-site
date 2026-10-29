import { expect, test } from '@playwright/test';
import { attendreSansDebordement } from './outils';

const PAGES = [
  { lien: 'Mentions légales', adresse: '/mentions-legales', titre: 'Mentions légales' },
  { lien: 'Confidentialité', adresse: '/confidentialite', titre: 'Politique de confidentialité' },
  { lien: 'Conditions du parrainage', adresse: '/parrainage/conditions', titre: 'Conditions du parrainage' },
];

for (const { lien, adresse, titre } of PAGES) {
  test(`pied de page → ${titre}`, async ({ page }) => {
    await page.goto('/');
    await page.getByRole('contentinfo').getByRole('link', { name: lien, exact: true }).click();
    await expect(page).toHaveURL(adresse);
    await expect(page.getByRole('heading', { level: 1 })).toHaveText(titre);
    await expect(page).toHaveTitle(`${titre} — ABS clean`);
  });
}

test.describe('340 px', () => {
  test.use({ viewport: { width: 340, height: 740 } });

  for (const { adresse } of PAGES) {
    test(`${adresse} sans débordement horizontal`, async ({ page }) => {
      await page.goto(adresse);
      await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
      await attendreSansDebordement(page);
    });
  }
});

test('la fenêtre de coordonnées renvoie vers la politique de confidentialité', async ({ page }) => {
  await page.goto('/');
  await page.locator('#devis').getByRole('button', { name: 'Ajouter : Vitres' }).click();
  await page.locator('#devis').getByRole('button', { name: 'Valider mon devis' }).click();
  await expect(page.getByRole('dialog').getByRole('link', { name: /Politique de confidentialité/ })).toHaveAttribute('href', '/confidentialite');
});

test('aucune ressource chargée chez un tiers (polices comprises)', async ({ page, baseURL }) => {
  const origine = new URL(baseURL!).origin;
  const tiers: string[] = [];
  page.on('request', (r) => {
    if (!r.url().startsWith(origine) && !r.url().startsWith('data:')) tiers.push(r.url());
  });
  await page.goto('/');
  await page.locator('#devis').scrollIntoViewIfNeeded();
  await page.waitForLoadState('networkidle');

  expect(tiers).toEqual([]);
  expect(await page.evaluate(() => document.fonts.check('16px "Inter Variable"'))).toBe(true);
});
