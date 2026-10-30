import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, retry, shareReplay, timer } from 'rxjs';

export type Profil = 'particulier' | 'professionnel';
export type Frequence = 'ponctuel' | 'mensuel' | 'trimestriel';

export interface ElementTarif {
  code: string;
  libelle: string;
  prixUnitaire: number;
  aPartirDe: boolean;
}

export interface FrequenceTarif {
  code: Frequence;
  reductionPourcent: number | null;
  reductionConnue: boolean;
}

export interface GrilleTarifs {
  elements: ElementTarif[];
  frequences: FrequenceTarif[];
  profils: Profil[];
  quantiteMin: number;
  quantiteMax: number;
  mentionEstimation: string;
  mentionOption: string;
}

/** Accès à GET /api/tarifs : source unique des prix pour le front (CAHIER-DES-CHARGES.md §10). */
@Injectable({ providedIn: 'root' })
export class TarifsApi {
  private readonly http = inject(HttpClient);

  /**
   * Chargée une seule fois, partagée entre la section Tarifs et le formulaire de devis.
   * Réseau capricieux (mobile) : jusqu'à 3 nouvelles tentatives espacées, sinon l'estimation resterait vide jusqu'au rechargement.
   */
  private readonly grille$ = this.http.get<GrilleTarifs>('/api/tarifs').pipe(
    retry({ count: 3, delay: (_, tentative) => timer(tentative * 1500) }),
    shareReplay({ bufferSize: 1, refCount: false }),
  );

  grille(): Observable<GrilleTarifs> {
    return this.grille$;
  }
}
