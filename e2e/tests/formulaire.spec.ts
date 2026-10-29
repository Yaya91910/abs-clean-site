import { expect, test } from '@playwright/test';
import { clientFictif } from './outils';

test('« Valider mon devis » sans élément affiche une aide', async ({ page }) => {
  await page.goto('/');
  const devis = page.locator('#devis');
  await devis.getByRole('button', { name: 'Valider mon devis' }).click();
  await expect(devis.getByText('Ajoutez au moins une vitre, baie ou velux pour valider.')).toBeVisible();
  await expect(page.getByRole('dialog')).toBeHidden();
});

test('la fenêtre se ferme avec Échap et avec « Annuler »', async ({ page }) => {
  await page.goto('/');
  const devis = page.locator('#devis');
  await devis.getByRole('button', { name: 'Ajouter : Velux' }).click();

  await devis.getByRole('button', { name: 'Valider mon devis' }).click();
  await expect(page.getByRole('dialog')).toBeVisible();
  await expect(page.getByRole('dialog').getByLabel('Nom', { exact: true })).toBeFocused();
  await page.keyboard.press('Escape');
  await expect(page.getByRole('dialog')).toBeHidden();

  await devis.getByRole('button', { name: 'Valider mon devis' }).click();
  await page.getByRole('dialog').getByRole('button', { name: 'Annuler' }).click();
  await expect(page.getByRole('dialog')).toBeHidden();
});

test('FAQ : une seule question ouverte à la fois', async ({ page }) => {
  await page.goto('/');
  const questions = page.locator('#faq').getByRole('button', { expanded: true });
  await expect(questions).toHaveCount(1);

  await page.getByRole('button', { name: /Dans quelles zones intervenez-vous/ }).click();
  await expect(page.getByRole('button', { name: /Dans quelles zones intervenez-vous/ })).toHaveAttribute('aria-expanded', 'true');
  await expect(questions).toHaveCount(1);
  await expect(page.getByText('ABS clean intervient en Essonne et en Île-de-France.')).toBeVisible();
});

test('le lien de parrainage préremplit et vérifie le code', async ({ page, request }) => {
  const parrain = clientFictif('Sophie');
  const reponse = await request.post('/api/demandes', {
    data: { nbVitres: 2, ...parrain, consentement: true },
  });
  expect(reponse.status()).toBe(201);
  const { codeParrain } = (await reponse.json()) as { codeParrain: string };

  await page.goto(`/devis?parrain=${codeParrain.toLowerCase()}`);
  const devis = page.locator('#devis');
  await expect(devis.getByLabel(/Code de parrainage/)).toHaveValue(codeParrain);
  await expect(devis.getByText('✓ Code reconnu : votre proche sera remercié.')).toBeVisible();
  await expect(devis).toBeInViewport();
});

test('un code de parrainage inconnu est signalé sans bloquer', async ({ page }) => {
  await page.goto('/');
  const champ = page.locator('#devis').getByLabel(/Code de parrainage/);
  await champ.fill('abs-inconnuzz');
  await champ.blur();
  await expect(page.getByText('Code inconnu : il ne sera pas pris en compte')).toBeVisible();
});
