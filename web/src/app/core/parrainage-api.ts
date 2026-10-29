import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of } from 'rxjs';

export type ResultatValidation = 'validee' | 'deja-validee' | 'lien-invalide' | 'erreur';

/** Parrainage (CLAUDE.md §9 bis) : vérification des codes, QR codes, validation de l'intervention par le gérant. */
@Injectable({ providedIn: 'root' })
export class ParrainageApi {
  private readonly http = inject(HttpClient);

  /** true si le code existe ; null si la vérification a échoué (réseau). */
  existe(code: string): Observable<boolean | null> {
    return this.http.get(`/api/parrainage/${encodeURIComponent(code)}`).pipe(
      map(() => true),
      catchError((e: HttpErrorResponse) => of(e.status === 404 ? false : null)),
    );
  }

  qrSvg(code: string): string {
    return `/api/parrainage/${encodeURIComponent(code)}/qr.svg`;
  }

  validerIntervention(demandeId: string, jeton: string): Observable<ResultatValidation> {
    return this.http.post(`/api/admin/demandes/${encodeURIComponent(demandeId)}/intervention-realisee`, { jeton }).pipe(
      map((): ResultatValidation => 'validee'),
      catchError((e: HttpErrorResponse) =>
        of<ResultatValidation>(e.status === 409 ? 'deja-validee' : e.status === 403 || e.status === 404 ? 'lien-invalide' : 'erreur'),
      ),
    );
  }
}
