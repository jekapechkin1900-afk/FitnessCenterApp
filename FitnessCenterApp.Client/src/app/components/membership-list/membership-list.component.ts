import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { forkJoin, map } from 'rxjs';
import { SelectionModel } from '@angular/cdk/collections';
import { MatCheckboxModule } from '@angular/material/checkbox';

import { Membership } from '../../core/models/membership.model';
import { Client } from '../../core/models/client.model';
import { MembershipService } from '../../core/services/api/membership.service';
import { ClientService } from '../../core/services/api/client.service';
import { ConfirmDialogComponent } from '../dialogs/confirm-dialog/confirm-dialog.component';
import { MembershipDialogComponent } from '../dialogs/membership-dialog/membership-dialog.component';

interface MembershipView extends Membership {
  clientName?: string;
}

@Component({
  selector: 'app-membership-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatSortModule, MatButtonModule, MatIconModule, MatTooltipModule, MatCheckboxModule],
  templateUrl: './membership-list.component.html',
  styleUrls: ['./membership-list.component.css']
})
export class MembershipListComponent implements OnInit {
  displayedColumns: string[] = ['select', 'clientName', 'type', 'price', 'startDate', 'endDate'];
  dataSource = new MatTableDataSource<MembershipView>();
  selection = new SelectionModel<MembershipView>(true, []);
  clients: Client[] = [];

  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private membershipService: MembershipService,
    private clientService: ClientService,
    public dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

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

  editSelectedMembership(): void {
    if (this.selection.selected.length !== 1) {
      return;
    }
    const selectedMembership = this.selection.selected[0];
    this.openMembershipDialog(selectedMembership);
  }

  loadData(): void {
    forkJoin({
      memberships: this.membershipService.getMemberships(),
      clients: this.clientService.getClients()
    }).pipe(
      map(({ memberships, clients }) => {
        this.clients = clients;
        const clientMap = new Map(clients.map(c => [c.id, c.fullName]));
        return memberships.map(m => ({
          ...m,
          clientName: clientMap.get(m.clientId) || 'Неизвестный клиент'
        }));
      })
    ).subscribe(membershipViews => {
      this.dataSource.data = membershipViews;
      this.dataSource.sort = this.sort;
    });
  }

  openMembershipDialog(membership?: MembershipView): void {
    const dialogRef = this.dialog.open(MembershipDialogComponent, {
      width: '450px',
      data: {
        clients: this.clients,
        membership: membership ? { ...membership } : null
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadData();
        this.snackBar.open(`Абонемент успешно ${membership ? 'обновлен' : 'добавлен'}!`, 'ОК', { duration: 3000 });
      }
    });
  }

  deleteSelectedMemberships(): void {
    const selectedIds = this.selection.selected.map(m => m.id);
    if (selectedIds.length === 0) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Подтверждение удаления',
        message: `Вы уверены, что хотите удалить ${selectedIds.length} абонемент(ов)?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        const deleteRequests = selectedIds.map(id => this.membershipService.deleteMembership(id));
        forkJoin(deleteRequests).subscribe(() => {
          this.loadData();
          this.snackBar.open(`${selectedIds.length} абонемент(ов) удалено.`, 'ОК', { duration: 3000 });
        });
      }
    });
  }
}
