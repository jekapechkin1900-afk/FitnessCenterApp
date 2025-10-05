import { Component, OnInit } from '@angular/core';
import { CommonModule, KeyValuePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { provideNativeDateAdapter } from '@angular/material/core';

import { AnalyticsService } from '../../core/services/api/analytics.service';
import { ClientService } from '../../core/services/api/client.service';
import { ProfitableMonth } from '../../core/models/analytics.model';
import { Client } from '../../core/models/client.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule, MatCardModule, MatProgressSpinnerModule, KeyValuePipe,
    ReactiveFormsModule, MatFormFieldModule, MatDatepickerModule, MatButtonModule, MatInputModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  isLoading = true;
  analyticsResults: { [key: string]: string } = {};

  range = new FormGroup({
    start: new FormControl<Date | null>(null, Validators.required),
    end: new FormControl<Date | null>(null, Validators.required),
  });
  isLoadingVisits = false;
  totalVisitsInPeriod: number | null = null;

  private readonly dayOfWeekMap: { [key: string]: string } = {
    'sunday': 'Воскресенье', 'monday': 'Понедельник', 'tuesday': 'Вторник',
    'wednesday': 'Среда', 'thursday': 'Четверг', 'friday': 'Пятница', 'saturday': 'Суббота'
  };
  private readonly monthMap: string[] = [
    'Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
    'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'
  ];

  constructor(
    private analyticsService: AnalyticsService,
    private clientService: ClientService
  ) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;

    forkJoin({
      profitableMonth: this.analyticsService.getMostProfitableMonth().pipe(catchError(() => of(null))),
      popularMembership: this.analyticsService.getMostPopularMembershipType().pipe(catchError(() => of(null))),
      activeClientId: this.analyticsService.getMostActiveClientId().pipe(catchError(() => of(null))),
      busiestDay: this.analyticsService.getBusiestDayOfWeek().pipe(catchError(() => of(null))),
      clients: this.clientService.getClients().pipe(catchError(() => of([] as Client[])))
    }).subscribe(({ profitableMonth, popularMembership, activeClientId, busiestDay, clients }) => {
      this.analyticsResults['Самый прибыльный месяц'] = this.formatMonthData(profitableMonth);
      this.analyticsResults['Самый популярный абонемент'] = this.decodeAndCleanServerString(popularMembership);
      this.analyticsResults['Самый активный клиент'] = this.findClientNameById(activeClientId, clients);
      this.analyticsResults['Самый загруженный день'] = this.translateDayOfWeek(busiestDay);
      this.isLoading = false;
    });
  }

  calculateVisits(): void {
    if (this.range.invalid) {
      return;
    }
    this.isLoadingVisits = true;
    this.totalVisitsInPeriod = null;
    const { start, end } = this.range.value;

    this.analyticsService.getTotalVisitsInPeriod(start!, end!).subscribe({
      next: (count) => {
        this.totalVisitsInPeriod = count;
        this.isLoadingVisits = false;
      },
      error: () => {
        this.isLoadingVisits = false;
      }
    });
  }

  private formatMonthData(data: ProfitableMonth | null): string {
    if (!data || data.year === 0) return 'Нет данных';
    const monthName = this.monthMap[data.month - 1] || '';
    return `${monthName} ${data.year} (${data.totalRevenue.toLocaleString('ru-RU')} руб.)`;
  }

  private decodeAndCleanServerString(value: string | null): string {
    if (!value) return 'Нет данных';
    try {
      const cleanedValue = value.replace(/^"|"$/g, '');
      return JSON.parse(`"${cleanedValue}"`);
    } catch {
      return value;
    }
  }

  private findClientNameById(clientId: string | null, clients: Client[]): string {
    if (!clientId) return 'Нет данных';
    const client = clients.find(c => c.id.toLowerCase() === clientId.toLowerCase().replace(/"/g, ''));
    return client ? client.fullName : `ID: ${clientId.substring(0, 8)}...`;
  }

  private translateDayOfWeek(day: string | null): string {
    if (!day) return 'Нет данных';
    return this.dayOfWeekMap[day.toLowerCase().replace(/"/g, '')] || day;
  }
}
