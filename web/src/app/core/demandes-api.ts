import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of } from 'rxjs';
import { Frequence, Profil } from './tarifs-api';

export type Creneau = 'semaineMatin' | 'semaineApresMidi' | 'samedi';

export interface NouvelleDemande {
  profil: Profil;
  frequence: Frequence;
  nbVitres: number;
  nbBaies: number;
  nbVelux: number;
  nom: string;
  prenom: string;
  telephone: string;
  email: string;
  adresse: string;
  codePostal: string;
  ville: string;
  acces: string;
  creneaux: Creneau[];
  commentaire: string;
  codeParrainage: string;
  consentement: boolean;
  /** Champ piège (honeypot) : toujours vide pour un humain. */
  siteWeb: string;
}

export type ResultatEnvoi =
  | { statut: 'ok'; numero: string; total: number }
  | { statut: 'invalide'; erreurs: Record<string, string[]> }
  | { statut: 'trop-de-demandes' }
  | { statut: 'erreur' };

/** POST /api/demandes (CLAUDE.md §9). */
@Injectable({ providedIn: 'root' })
export class DemandesApi {
  private readonly http = inject(HttpClient);

  envoyer(demande: NouvelleDemande): Observable<ResultatEnvoi> {
    return this.http.post<{ numero: string; total: number }>('/api/demandes', demande).pipe(
      map((r): ResultatEnvoi => ({ statut: 'ok', numero: r.numero, total: r.total })),
      catchError((e: HttpErrorResponse) => of(versResultat(e))),
    );
  }
}

function versResultat(e: HttpErrorResponse): ResultatEnvoi {
  if (e.status === 429) return { statut: 'trop-de-demandes' };
  if (e.status === 400 && e.error?.errors) return { statut: 'invalide', erreurs: e.error.errors };
  return { statut: 'erreur' };
}
