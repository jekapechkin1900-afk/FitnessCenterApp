import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProfitableMonth } from '../../models/analytics.model';

@Injectable({ providedIn: 'root' })
export class AnalyticsService {
  private apiUrl = 'http://localhost:8080/api/analytics';

  constructor(private http: HttpClient) { }
  
  getMostProfitableMonth(): Observable<ProfitableMonth> {
    return this.http.get<ProfitableMonth>(`${this.apiUrl}/most-profitable-month`);
  }

  getMostPopularMembershipType(): Observable<string> {
    return this.http.get(`${this.apiUrl}/most-popular-membership`, { responseType: 'text' });
  }

  getMostActiveClientId(): Observable<string> {
    return this.http.get(`${this.apiUrl}/most-active-client`, { responseType: 'text' });
  }

  getBusiestDayOfWeek(): Observable<string> {
    return this.http.get(`${this.apiUrl}/busiest-day`, { responseType: 'text' });
  }

  getTotalVisitsInPeriod(start: Date, end: Date): Observable<number> {
    const formatDate = (d: Date) => d.toISOString().split('T')[0];

    const params = new HttpParams()
      .set('start', formatDate(start))
      .set('end', formatDate(end));

    return this.http.get<number>(`${this.apiUrl}/visits-in-period`, { params });
  }
}
