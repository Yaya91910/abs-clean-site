import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Devis } from './devis';
import { GRILLE } from '../core/devis-store.spec';

describe('Devis et fenêtre de coordonnées', () => {
  let http: HttpTestingController;

  async function creer() {
    const fixture = TestBed.createComponent(Devis);
    fixture.detectChanges();
    http.expectOne('/api/tarifs').flush(GRILLE);
    await fixture.whenStable();
    const page = fixture.nativeElement as HTMLElement;
    const stable = async () => {
      await new Promise((r) => setTimeout(r));
      await fixture.whenStable();
    };
    const saisir = (id: string, valeur: string) => {
      const champ = page.querySelector<HTMLInputElement>('#' + id)!;
      champ.value = valeur;
      champ.dispatchEvent(new Event('input'));
    };
    return { fixture, page, stable, saisir };
  }

  function remplir(saisir: (id: string, v: string) => void, page: HTMLElement) {
    saisir('c-nom', 'Martin');
    saisir('c-prenom', 'Léa');
    saisir('c-telephone', '06 12 34 56 78');
    saisir('c-email', 'lea@example.com');
    saisir('c-adresse', '12 rue des Lilas');
    saisir('c-ville', 'Évry');
    page.querySelector<HTMLInputElement>('.consent input')!.click();
  }

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [Devis], providers: [provideHttpClient(), provideHttpClientTesting()] });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('refuse d’ouvrir la fenêtre sans aucun élément', async () => {
    const { page, stable } = await creer();

    page.querySelector<HTMLButtonElement>('.valider')!.click();
    await stable();

    expect(page.querySelector('.alerte')?.textContent).toContain('Ajoutez au moins une vitre');
    expect(page.querySelector('dialog')!.hasAttribute('open')).toBe(false);
  });

  it('affiche les erreurs champ par champ sans rien envoyer', async () => {
    const { page, stable } = await creer();
    page.querySelector<HTMLButtonElement>('[aria-label="Ajouter : Vitres"]')!.click();
    page.querySelector<HTMLButtonElement>('.valider')!.click();
    await stable();

    page.querySelector<HTMLButtonElement>('.envoyer')!.click();
    await stable();

    const erreurs = Array.from(page.querySelectorAll('.field-error')).map((e) => e.id);
    expect(erreurs).toEqual(['e-nom', 'e-prenom', 'e-telephone', 'e-email', 'e-adresse', 'e-codePostal', 'e-ville', 'e-consentement']);
    http.expectNone('/api/demandes');
  });

  it('préremplit le code postal et envoie la demande avec les quantités et le code parrain', async () => {
    const { page, stable, saisir } = await creer();
    page.querySelector<HTMLButtonElement>('[aria-label="Ajouter : Vitres"]')!.click();
    page.querySelector<HTMLButtonElement>('[aria-label="Ajouter : Velux"]')!.click();
    saisir('localisation', '91000');
    const code = page.querySelector<HTMLInputElement>('#code-parrain')!;
    code.value = 'abs-marie7';
    code.dispatchEvent(new Event('change'));
    page.querySelector<HTMLButtonElement>('.valider')!.click();
    await stable();

    expect(page.querySelector<HTMLInputElement>('#c-codePostal')!.value).toBe('91000');
    remplir(saisir, page);
    page.querySelector<HTMLButtonElement>('.envoyer')!.click();
    await stable();

    const requete = http.expectOne('/api/demandes');
    expect(requete.request.body).toMatchObject({
      profil: 'particulier',
      frequence: 'ponctuel',
      nbVitres: 1,
      nbBaies: 0,
      nbVelux: 1,
      nom: 'Martin',
      codePostal: '91000',
      consentement: true,
      siteWeb: '',
      codeParrainage: 'ABS-MARIE7',
    });
    expect(requete.request.body).not.toHaveProperty('total');

    requete.flush({ numero: 'ABS-2026-0001', total: 45 }, { status: 201, statusText: 'Created' });
    await stable();
    expect(page.querySelector('.succes')?.textContent).toContain('Demande envoyée !');
  });

  it('place les erreurs renvoyées par le serveur sous le bon champ', async () => {
    const { page, stable, saisir } = await creer();
    page.querySelector<HTMLButtonElement>('[aria-label="Ajouter : Vitres"]')!.click();
    page.querySelector<HTMLButtonElement>('.valider')!.click();
    await stable();
    remplir(saisir, page);
    saisir('c-codePostal', '91000');
    page.querySelector<HTMLButtonElement>('.envoyer')!.click();
    await stable();

    http.expectOne('/api/demandes').flush(
      { status: 400, errors: { email: ['Adresse e-mail invalide.'] } },
      { status: 400, statusText: 'Bad Request' },
    );
    await stable();

    expect(page.querySelector('#e-email')?.textContent).toContain('Adresse e-mail invalide.');
    expect(page.querySelector('.succes')).toBeNull();
  });

  it('explique la limitation quand le serveur répond 429', async () => {
    const { page, stable, saisir } = await creer();
    page.querySelector<HTMLButtonElement>('[aria-label="Ajouter : Vitres"]')!.click();
    page.querySelector<HTMLButtonElement>('.valider')!.click();
    await stable();
    remplir(saisir, page);
    saisir('c-codePostal', '91000');
    page.querySelector<HTMLButtonElement>('.envoyer')!.click();
    await stable();

    http.expectOne('/api/demandes').flush(null, { status: 429, statusText: 'Too Many Requests' });
    await stable();

    expect(page.querySelector('.globale')?.textContent).toContain('Trop de demandes');
  });
});
