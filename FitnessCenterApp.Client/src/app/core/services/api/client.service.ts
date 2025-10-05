import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { Client } from '../../models/client.model';

@Injectable({ providedIn: 'root' })
export class ClientService {
  private apiUrl = 'http://localhost:8080/api/clients';

  constructor(private http: HttpClient) { }

  getClients(): Observable<Client[]> {
    return this.http.get<Client[]>(this.apiUrl);
  }

  createClient(client: Omit<Client, 'id'>): Observable<Client> {
    return this.http.post<Client>(this.apiUrl, client);
  }

  updateClient(client: Client): Observable<Client> {
    return this.http.put<Client>(`${this.apiUrl}/${client.id}`, client);
  }

  deleteClients(ids: string[]): Observable<void[]> {
    const deleteRequests = ids.map(id => this.http.delete<void>(`${this.apiUrl}/${id}`));
    return forkJoin(deleteRequests);
  }
}
