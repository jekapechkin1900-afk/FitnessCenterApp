import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription, forkJoin, map } from 'rxjs';
import { SelectionModel } from '@angular/cdk/collections'; 
import { MatCheckboxModule } from '@angular/material/checkbox';

import { Visit } from '../../core/models/visit.model';
import { Client } from '../../core/models/client.model';
import { VisitService } from '../../core/services/api/visit.service';
import { ClientService } from '../../core/services/api/client.service';
import { ConfirmDialogComponent } from '../dialogs/confirm-dialog/confirm-dialog.component';
import { VisitDialogComponent } from '../dialogs/visit-dialog/visit-dialog.component';

interface VisitView extends Visit {
  clientName?: string;
}

@Component({
  selector: 'app-visit-list',
  standalone: true,
  imports: [
    CommonModule, MatTableModule, MatSortModule, MatButtonModule, MatDialogModule,
    MatIconModule, MatTooltipModule, MatProgressSpinnerModule, MatCheckboxModule   
  ],
  templateUrl: './visit-list.component.html',
  styleUrls: ['./visit-list.component.css']
})
export class VisitListComponent implements OnInit, OnDestroy {
  displayedColumns: string[] = ['select', 'clientName', 'visitTime'];
  dataSource = new MatTableDataSource<VisitView>();
  selection = new SelectionModel<VisitView>(true, []);
  isLoading = true;

  isAllSelected(): boolean {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle(): void {
    this.isAllSelected() ?
      this.selection.clear() :
      this.dataSource.data.forEach(row => this.selection.select(row));
  }

  editSelectedVisit(): void {
    if (this.selection.selected.length !== 1) {
      return;
    }
    const selectedVisit = this.selection.selected[0];
    this.openVisitDialog(selectedVisit);
  }

  @ViewChild(MatSort) sort!: MatSort;

  private visitsSubscription!: Subscription;
  private clients: Client[] = [];

  constructor(
    private visitService: VisitService,
    private clientService: ClientService,
    private snackBar: MatSnackBar,
    public dialog: MatDialog,
  ) { }

  ngOnInit(): void {
    forkJoin({
      clients: this.clientService.getClients(),
      initialVisits: this.visitService.loadVisits()
    }).subscribe(({ clients }) => {
      this.clients = clients;
      this.isLoading = false;

      this.visitsSubscription = this.visitService.visits$.subscribe(visits => {
        this.updateDataSource(visits);
      });
    });
  }

  updateDataSource(visits: Visit[]): void {
    const clientMap = new Map(this.clients.map(c => [c.id, c.fullName]));
    const visitViews = visits.map(v => ({
      ...v,
      clientName: clientMap.get(v.clientId) || 'Неизвестный клиент'
    }));

    this.dataSource.data = visitViews;
    this.dataSource.sort = this.sort;
  }

  openVisitDialog(visit?: VisitView): void {
    const dialogRef = this.dialog.open(VisitDialogComponent, {
      width: '450px',
      data: {
        clients: this.clients, 
        visit: visit ? { ...visit } : null
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open(`Посещение успешно ${visit ? 'обновлено' : 'добавлено'}!`, 'ОК', { duration: 3000 });
      }
    });
  }

  deleteSelectedVisits(): void {
    const selectedIds = this.selection.selected.map(v => v.id);
    if (selectedIds.length === 0) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Подтверждение удаления',
        message: `Вы уверены, что хотите удалить ${selectedIds.length} посещени(й/е)?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        const deleteRequests = selectedIds.map(id => this.visitService.deleteVisit(id));
        forkJoin(deleteRequests).subscribe(() => {
          this.snackBar.open(`${selectedIds.length} посещени(й/е) удалено.`, 'ОК', { duration: 3000 });
        });
      }
    });
  }

  ngOnDestroy(): void {
    if (this.visitsSubscription) {
      this.visitsSubscription.unsubscribe();
    }
  }
}
