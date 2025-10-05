import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { Visit } from '../../models/visit.model';

export type CreateVisitDto = Pick<Visit, 'clientId' | 'visitTime'>;

@Injectable({ providedIn: 'root' })
export class VisitService {
  private apiUrl = 'http://localhost:8080/api/visits';
  private visitsSubject = new BehaviorSubject<Visit[]>([]);
  public visits$ = this.visitsSubject.asObservable();

  constructor(private http: HttpClient) { }

  loadVisits(): Observable<Visit[]> {
    return this.http.get<Visit[]>(this.apiUrl).pipe(
      tap(visits => this.visitsSubject.next(visits))
    );
  }

  createVisit(visitData: CreateVisitDto): Observable<Visit> {
    return this.http.post<Visit>(this.apiUrl, visitData).pipe(
      tap(newVisit => {
        const currentVisits = this.visitsSubject.getValue();
        this.visitsSubject.next([...currentVisits, newVisit]);
      })
    );
  }

  updateVisit(visit: Visit): Observable<Visit> {
    return this.http.put<Visit>(`${this.apiUrl}/${visit.id}`, visit).pipe(
      tap(updatedVisit => {
        const currentVisits = this.visitsSubject.getValue();
        const index = currentVisits.findIndex(v => v.id === updatedVisit.id);
        if (index > -1) {
          currentVisits[index] = updatedVisit;
          this.visitsSubject.next([...currentVisits]);
        }
      })
    );
  }

  deleteVisit(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => {
        const currentVisits = this.visitsSubject.getValue();
        const updatedVisits = currentVisits.filter(v => v.id !== id);
        this.visitsSubject.next(updatedVisits);
      })
    );
  }
}
