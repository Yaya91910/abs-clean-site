import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Tarifs } from './tarifs';
import { GrilleTarifs } from '../core/tarifs-api';

const GRILLE: GrilleTarifs = {
  elements: [
    { code: 'vitre', libelle: 'Vitre', prixUnitaire: 20, aPartirDe: false },
    { code: 'baie', libelle: 'Baie vitrée', prixUnitaire: 40, aPartirDe: true },
  ],
  frequences: [
    { code: 'ponctuel', reductionPourcent: 0, reductionConnue: true },
    { code: 'mensuel', reductionPourcent: null, reductionConnue: false },
  ],
  profils: ['particulier', 'professionnel'],
  quantiteMin: 0,
  quantiteMax: 99,
  mentionEstimation: 'Estimation confirmée après échange',
  mentionOption: 'Encadrements, rails et rebords : sur devis',
};

describe('Tarifs', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [Tarifs],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('affiche les prix renvoyés par l’API', async () => {
    const fixture = TestBed.createComponent(Tarifs);
    fixture.detectChanges();
    http.expectOne('/api/tarifs').flush(GRILLE);
    await fixture.whenStable();
    const texte = (fixture.nativeElement as HTMLElement).textContent!;

    expect(texte).toContain('Vitre');
    expect(texte).toMatch(/l'unité\s*20\s*€/);
    expect(texte).toMatch(/à partir de\s*40\s*€/);
  });

  it('signale une erreur si l’API ne répond pas', async () => {
    const fixture = TestBed.createComponent(Tarifs);
    fixture.detectChanges();
    http.expectOne('/api/tarifs').flush(null, { status: 503, statusText: 'Indisponible' });
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).querySelector('[role="alert"]')).not.toBeNull();
  });
});
