import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { SelectionModel } from '@angular/cdk/collections';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSort } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';

import { Client } from '../../core/models/client.model';
import { ClientService } from '../../core/services/api/client.service';
import { ClientDialogComponent } from '../dialogs/client-dialog/client-dialog.component';
import { ConfirmDialogComponent } from '../dialogs/confirm-dialog/confirm-dialog.component';

import { MatTableModule } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSortModule } from '@angular/material/sort';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [
    CommonModule, MatTableModule, MatCheckboxModule, MatButtonModule,
    MatIconModule, MatSortModule, MatTooltipModule 
  ],
  templateUrl: './client-list.component.html',
  styleUrl: './client-list.component.css' 
})
export class ClientListComponent implements OnInit {
  displayedColumns: string[] = ['select', 'fullName', 'email', 'phoneNumber'];
  dataSource = new MatTableDataSource<Client>();
  selection = new SelectionModel<Client>(true, []);

  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private clientService: ClientService,
    public dialog: MatDialog,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void { this.loadClients(); }

  loadClients(): void {
    this.clientService.getClients().subscribe(clients => {
      this.dataSource.data = clients;
      this.dataSource.sort = this.sort;
      this.selection.clear();
    });
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

  editSelectedClient(): void {
    if (this.selection.selected.length !== 1) {
      return;
    }
    const selectedClient = this.selection.selected[0];
    this.openClientDialog(selectedClient);
  }

  openClientDialog(client?: Client): void {
    const dialogRef = this.dialog.open(ClientDialogComponent, {
      width: '400px',
      data: client ? { ...client } : null 
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadClients(); 
        this.snackBar.open(`Клиент ${client ? 'обновлен' : 'создан'} успешно!`, 'ОК', { duration: 3000 });
      }
    });
  }

  deleteSelectedClients(): void {
    const selectedIds = this.selection.selected.map(c => c.id);
    if (selectedIds.length === 0) return;

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Подтверждение удаления',
        message: `Вы уверены, что хотите удалить ${selectedIds.length} клиент(ов)? Это действие необратимо.`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.clientService.deleteClients(selectedIds).subscribe(() => {
          this.loadClients();
          this.snackBar.open(`${selectedIds.length} клиент(ов) удалено.`, 'ОК', { duration: 3000 });
        });
      }
    });
  }
}
