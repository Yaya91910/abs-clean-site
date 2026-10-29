import { Page, expect } from '@playwright/test';

/** Coordonnées d'un client fictif, uniques à chaque appel (téléphone, e-mail, adresse). */
export function clientFictif(prenom = 'Léa') {
  const n = `${Date.now()}${Math.floor(Math.random() * 1000)}`.slice(-8);
  return {
    nom: 'Testeur',
    prenom,
    telephone: `07 ${n.slice(0, 2)} ${n.slice(2, 4)} ${n.slice(4, 6)} ${n.slice(6, 8)}`,
    email: `e2e-${n}@example.test`,
    adresse: `${n} rue des Tests`,
    codePostal: '91000',
    ville: 'Évry-Courcouronnes',
  };
}

/** Aucun défilement horizontal : la page ne dépasse pas la largeur de la fenêtre. */
export async function attendreSansDebordement(page: Page) {
  const { largeurPage, largeurFenetre } = await page.evaluate(() => ({
    largeurPage: document.documentElement.scrollWidth,
    largeurFenetre: window.innerWidth,
  }));
  expect(largeurPage, 'débordement horizontal').toBeLessThanOrEqual(largeurFenetre);
}

export async function remplirCoordonnees(page: Page, client: ReturnType<typeof clientFictif>) {
  const fenetre = page.getByRole('dialog');
  await fenetre.getByLabel('Nom', { exact: true }).fill(client.nom);
  await fenetre.getByLabel('Prénom').fill(client.prenom);
  await fenetre.getByLabel('Téléphone').fill(client.telephone);
  await fenetre.getByLabel('E-mail').fill(client.email);
  await fenetre.getByLabel("Adresse d'intervention").fill(client.adresse);
  await fenetre.getByLabel('Code postal').fill(client.codePostal);
  await fenetre.getByLabel('Ville').fill(client.ville);
}
