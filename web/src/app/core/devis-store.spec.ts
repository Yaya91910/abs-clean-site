import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DevisStore } from './devis-store';
import { GrilleTarifs } from './tarifs-api';

export const GRILLE: GrilleTarifs = {
  elements: [
    { code: 'vitre', libelle: 'Vitre', prixUnitaire: 20, aPartirDe: false },
    { code: 'baie', libelle: 'Baie vitrée', prixUnitaire: 40, aPartirDe: true },
    { code: 'velux', libelle: 'Velux', prixUnitaire: 25, aPartirDe: false },
    { code: 'vitrine', libelle: 'Vitrine', prixUnitaire: 50, aPartirDe: true },
  ],
  frequences: [
    { code: 'ponctuel', reductionPourcent: 0, reductionConnue: true },
    { code: 'mensuel', reductionPourcent: null, reductionConnue: false },
    { code: 'trimestriel', reductionPourcent: null, reductionConnue: false },
  ],
  profils: ['particulier', 'professionnel'],
  quantiteMin: 0,
  quantiteMax: 99,
  mentionEstimation: 'Estimation confirmée après échange',
  mentionOption: 'Encadrements, rails et rebords : sur devis',
};

describe('DevisStore', () => {
  let store: DevisStore;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    store = TestBed.inject(DevisStore);
    TestBed.inject(HttpTestingController).expectOne('/api/tarifs').flush(GRILLE);
  });

  it('affiche « — € » tant qu’aucun élément n’est choisi', () => {
    expect(store.libelleEstimation()).toBe('— €');
  });

  it('calcule l’estimation avec les prix de l’API', () => {
    store.ajuster('vitre', 3);
    store.ajuster('baie', 1);
    store.ajuster('velux', 2);

    expect(store.total()).toBe(3 * 20 + 40 + 2 * 25);
    expect(store.libelleEstimation()).toBe('À partir de 150 €');
  });

  it('borne les quantités entre 0 et 99', () => {
    store.ajuster('vitre', -1);
    expect(store.quantites().vitre).toBe(0);

    store.ajuster('vitre', 150);
    expect(store.quantites().vitre).toBe(99);
  });

  it('signale une réduction non incluse pour les formules régulières', () => {
    expect(store.reductionNonIncluse()).toBe(false);
    store.frequence.set('mensuel');
    expect(store.reductionNonIncluse()).toBe(true);
  });

  it('normalise le code de parrainage en majuscules', () => {
    store.definirCodeParrain('  abs-176 5321 ');
    expect(store.codeParrain()).toBe('ABS-1765321');
  });

  it('ajoute le préfixe ABS- à un code saisi en chiffres seuls', () => {
    store.definirCodeParrain('1765321');
    expect(store.codeParrain()).toBe('ABS-1765321');
    store.definirCodeParrain('abs1765321');
    expect(store.codeParrain()).toBe('ABS-1765321');
  });
});
