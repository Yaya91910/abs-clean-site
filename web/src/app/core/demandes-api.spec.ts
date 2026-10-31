import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DELAI_MAX_ENVOI_MS, DemandesApi, NouvelleDemande, ResultatEnvoi } from './demandes-api';

describe('DemandesApi', () => {
  beforeEach(() => TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] }));
  afterEach(() => vi.useRealTimers());

  it('abandonne un envoi resté sans réponse et propose de réessayer', async () => {
    vi.useFakeTimers();
    const resultats: ResultatEnvoi[] = [];
    TestBed.inject(DemandesApi).envoyer({} as NouvelleDemande).subscribe((r) => resultats.push(r));
    const requete = TestBed.inject(HttpTestingController).expectOne('/api/demandes');

    await vi.advanceTimersByTimeAsync(DELAI_MAX_ENVOI_MS);

    expect(resultats).toEqual([{ statut: 'erreur' }]);
    expect(requete.cancelled).toBe(true);
  });

  it('traduit les réponses de l’API', () => {
    const api = TestBed.inject(DemandesApi);
    const http = TestBed.inject(HttpTestingController);
    const resultats: ResultatEnvoi[] = [];

    api.envoyer({} as NouvelleDemande).subscribe((r) => resultats.push(r));
    http.expectOne('/api/demandes').flush({ errors: { nom: ['Le nom est requis.'] } }, { status: 400, statusText: 'Bad Request' });
    api.envoyer({} as NouvelleDemande).subscribe((r) => resultats.push(r));
    http.expectOne('/api/demandes').flush(null, { status: 429, statusText: 'Too Many Requests' });

    expect(resultats).toEqual([{ statut: 'invalide', erreurs: { nom: ['Le nom est requis.'] } }, { statut: 'trop-de-demandes' }]);
  });
});
