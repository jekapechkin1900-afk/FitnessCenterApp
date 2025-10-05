import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ClientListComponent } from './components/client-list/client-list.component';
import { MembershipListComponent } from './components/membership-list/membership-list.component';
import { VisitListComponent } from './components/visit-list/visit-list.component';

export const routes: Routes = [
  { path: 'dashboard', component: DashboardComponent },
  { path: 'clients', component: ClientListComponent },
  { path: 'memberships', component: MembershipListComponent },
  { path: 'visits', component: VisitListComponent },
  { path: '**', redirectTo: '/dashboard' }
];
