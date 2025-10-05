import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Membership } from '../../models/membership.model';

@Injectable({ providedIn: 'root' })
export class MembershipService {
  private apiUrl = 'http://localhost:8080/api/memberships';

  constructor(private http: HttpClient) { }

  getMemberships(): Observable<Membership[]> {
    return this.http.get<Membership[]>(this.apiUrl);
  }

  updateMembership(membership: Membership): Observable<Membership> {
    return this.http.put<Membership>(`${this.apiUrl}/${membership.id}`, membership);
  }

  deleteMembership(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  createMembership(membership: Omit<Membership, 'id'>): Observable<Membership> {
    return this.http.post<Membership>(this.apiUrl, membership);
  }
}
