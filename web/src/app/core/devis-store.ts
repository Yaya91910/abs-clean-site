import { DOCUMENT, Injectable, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { Frequence, GrilleTarifs, Profil, TarifsApi } from './tarifs-api';
import { ParrainageApi } from './parrainage-api';

export type CodeQuantite = 'vitre' | 'baie' | 'velux';

export const PROFILS: { id: Profil; libelle: string }[] = [
  { id: 'particulier', libelle: 'Particulier' },
  { id: 'professionnel', libelle: 'Professionnel' },
];

export const FREQUENCES: { id: Frequence; libelle: string }[] = [
  { id: 'ponctuel', libelle: 'Ponctuel' },
  { id: 'mensuel', libelle: 'Mensuel' },
  { id: 'trimestriel', libelle: 'Trimestriel' },
];

const QUANTITE_MAX = 99;

/**
 * État du devis partagé entre le formulaire (section Devis) et la fenêtre de coordonnées.
 * L'estimation est indicative : l'API recalcule toujours le total (CLAUDE.md §7).
 */
@Injectable({ providedIn: 'root' })
export class DevisStore {
  private readonly grille = toSignal(inject(TarifsApi).grille().pipe(catchError(() => of(null))), { initialValue: null });

  readonly profil = signal<Profil>('particulier');
  readonly frequence = signal<Frequence>('ponctuel');
  readonly quantites = signal<Record<CodeQuantite, number>>({ vitre: 0, baie: 0, velux: 0 });
  readonly localisation = signal('');
  readonly codeParrain = signal('');
  /** Résultat de la vérification du code saisi : reconnu, inconnu, ou null (vide / non vérifié). */
  readonly codeReconnu = signal<boolean | null>(null);
  private readonly parrainageApi = inject(ParrainageApi);

  /** Total estimé, ou null tant que la grille n'est pas chargée. */
  readonly total = computed(() => {
    const grille = this.grille();
    if (!grille) return null;
    const q = this.quantites();
    return (Object.keys(q) as CodeQuantite[]).reduce((somme, code) => somme + q[code] * prix(grille, code), 0);
  });

  readonly nbElements = computed(() => Object.values(this.quantites()).reduce((a, b) => a + b, 0));

  readonly libelleEstimation = computed(() => {
    const total = this.total();
    return this.nbElements() > 0 && total !== null ? `À partir de ${total} €` : '— €';
  });

  readonly libelleProfil = computed(() => PROFILS.find((p) => p.id === this.profil())!.libelle);
  readonly libelleFrequence = computed(() => FREQUENCES.find((f) => f.id === this.frequence())!.libelle);

  /** Vrai si la formule choisie prévoit une réduction dont le pourcentage n'est pas encore connu. */
  readonly reductionNonIncluse = computed(
    () => this.grille()?.frequences.find((f) => f.code === this.frequence())?.reductionConnue === false,
  );

  constructor() {
    // Lien de parrainage : /devis?parrain=CODE préremplit le champ (CLAUDE.md §9 bis).
    const parrain = new URLSearchParams(inject(DOCUMENT).location.search).get('parrain');
    if (parrain) this.definirCodeParrain(parrain);
  }

  ajuster(code: CodeQuantite, delta: number): void {
    this.quantites.update((q) => ({ ...q, [code]: Math.max(0, Math.min(QUANTITE_MAX, q[code] + delta)) }));
  }

  definirCodeParrain(code: string): void {
    const normalise = code.replace(/\s/g, '').toUpperCase();
    this.codeParrain.set(normalise);
    this.codeReconnu.set(null);
    if (!normalise) return;
    this.parrainageApi.existe(normalise).subscribe((existe) => {
      // Ignore une réponse arrivée après une nouvelle saisie.
      if (this.codeParrain() === normalise) this.codeReconnu.set(existe);
    });
  }
}

function prix(grille: GrilleTarifs, code: string): number {
  return grille.elements.find((e) => e.code === code)?.prixUnitaire ?? 0;
}
