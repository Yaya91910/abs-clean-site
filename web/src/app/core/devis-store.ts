import { DOCUMENT, Injectable, computed, effect, inject, signal } from '@angular/core';
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

/** Choix du devis gardés dans le navigateur (aucune donnée personnelle) : on les retrouve à la visite suivante. */
const CLE_MEMOIRE = 'abs-clean.devis';

/**
 * État du devis partagé entre le formulaire (section Devis) et la fenêtre de coordonnées.
 * L'estimation est indicative : l'API recalcule toujours le total (CAHIER-DES-CHARGES.md §7).
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
  /** Vrai si des choix d'une visite précédente ont été repris. */
  readonly repris = signal(false);
  private readonly parrainageApi = inject(ParrainageApi);
  private readonly stockage = stockageLocal(inject(DOCUMENT));

  /** Total estimé, ou null tant que la grille n'est pas chargée. */
  readonly total = computed(() => {
    const grille = this.grille();
    if (!grille) return null;
    const q = this.quantites();
    return (Object.keys(q) as CodeQuantite[]).reduce((somme, code) => somme + q[code] * prix(grille, code), 0);
  });

  readonly nbElements = computed(() => Object.values(this.quantites()).reduce((a, b) => a + b, 0));

  /** Montant à afficher : null tant que rien n'est choisi ou que la grille n'est pas chargée. */
  readonly estimation = computed(() => (this.nbElements() > 0 ? this.total() : null));

  readonly libelleEstimation = computed(() => {
    const total = this.estimation();
    return total !== null ? `À partir de ${total} €` : '— €';
  });

  readonly libelleProfil = computed(() => PROFILS.find((p) => p.id === this.profil())!.libelle);
  readonly libelleFrequence = computed(() => FREQUENCES.find((f) => f.id === this.frequence())!.libelle);

  /** Vrai si la formule choisie prévoit une réduction dont le pourcentage n'est pas encore connu. */
  readonly reductionNonIncluse = computed(
    () => this.grille()?.frequences.find((f) => f.code === this.frequence())?.reductionConnue === false,
  );

  constructor() {
    // Lien de parrainage : /devis?parrain=CODE préremplit le champ (CAHIER-DES-CHARGES.md §9 bis).
    const parrain = new URLSearchParams(inject(DOCUMENT).location.search).get('parrain');
    if (parrain) this.definirCodeParrain(parrain);

    this.reprendre();
    effect(() => {
      const choix = { profil: this.profil(), frequence: this.frequence(), quantites: this.quantites() };
      try {
        this.stockage?.setItem(CLE_MEMOIRE, JSON.stringify(choix));
      } catch {
        // Stockage plein ou interdit (navigation privée) : tant pis, rien n'est perdu de la demande en cours.
      }
    });
  }

  /** Remet le formulaire à zéro (et oublie les choix gardés). */
  reinitialiser(): void {
    this.profil.set('particulier');
    this.frequence.set('ponctuel');
    this.quantites.set({ vitre: 0, baie: 0, velux: 0 });
    this.repris.set(false);
  }

  ajuster(code: CodeQuantite, delta: number): void {
    this.quantites.update((q) => ({ ...q, [code]: Math.max(0, Math.min(QUANTITE_MAX, q[code] + delta)) }));
  }

  /** Reprend les choix de la visite précédente, s'ils sont valides (le stockage peut avoir été modifié à la main). */
  private reprendre(): void {
    let choix: unknown;
    try {
      choix = JSON.parse(this.stockage?.getItem(CLE_MEMOIRE) ?? 'null');
    } catch {
      return;
    }
    if (!choix || typeof choix !== 'object') return;
    const { profil, frequence, quantites } = choix as Record<string, unknown>;
    const q = quantites as Record<CodeQuantite, unknown> | undefined;
    const valide = (n: unknown): n is number => Number.isInteger(n) && (n as number) >= 0 && (n as number) <= QUANTITE_MAX;
    if (!PROFILS.some((p) => p.id === profil) || !FREQUENCES.some((f) => f.id === frequence)) return;
    if (!q || !valide(q.vitre) || !valide(q.baie) || !valide(q.velux)) return;

    this.profil.set(profil as Profil);
    this.frequence.set(frequence as Frequence);
    this.quantites.set({ vitre: q.vitre, baie: q.baie, velux: q.velux });
    this.repris.set(q.vitre + q.baie + q.velux > 0);
  }

  definirCodeParrain(code: string): void {
    // Même règle que l'API (CodeParrain.Normaliser) : « 1765321 » ou « ABS1765321 » → « ABS-1765321 ».
    const normalise = code.replace(/\s/g, '').toUpperCase().replace(/^(?:ABS)?(\d{7})$/, 'ABS-$1');
    this.codeParrain.set(normalise);
    this.codeReconnu.set(null);
    if (!normalise) return;
    this.parrainageApi.existe(normalise).subscribe((existe) => {
      // Ignore une réponse arrivée après une nouvelle saisie.
      if (this.codeParrain() === normalise) this.codeReconnu.set(existe);
    });
  }
}

/** localStorage, ou null s'il est inaccessible (navigation privée stricte, cookies bloqués). */
function stockageLocal(document: Document): Storage | null {
  try {
    return document.defaultView?.localStorage ?? null;
  } catch {
    return null;
  }
}

function prix(grille: GrilleTarifs, code: string): number {
  return grille.elements.find((e) => e.code === code)?.prixUnitaire ?? 0;
}
