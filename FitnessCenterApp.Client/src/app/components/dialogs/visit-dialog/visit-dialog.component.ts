import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';

import { Client } from '../../../core/models/client.model';
import { Visit } from '../../../core/models/visit.model';
import { VisitService, CreateVisitDto } from '../../../core/services/api/visit.service';

export interface VisitDialogData {
  clients: Client[];
  visit?: Visit;
}

@Component({
  selector: 'app-visit-dialog',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatButtonModule, MatSelectModule, MatDatepickerModule, MatInputModule
  ],
  templateUrl: './visit-dialog.component.html',
  styleUrls: ['./visit-dialog.component.css']
})
export class VisitDialogComponent implements OnInit {
  visitForm: FormGroup;
  clients: Client[];
  isEditMode: boolean;

  constructor(
    private fb: FormBuilder,
    private visitService: VisitService,
    public dialogRef: MatDialogRef<VisitDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: VisitDialogData,
    private snackBar: MatSnackBar
  ) {
    this.clients = data.clients;
    this.isEditMode = !!data.visit;
    this.visitForm = this.fb.group({
      clientId: [data.visit?.clientId || '', Validators.required],
      visitTime: [data.visit ? new Date(data.visit.visitTime) : new Date(), Validators.required]
    });
  }

  ngOnInit(): void { }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.visitForm.invalid) return;

    const formDataRaw = this.visitForm.value;
    const visitTimeISO = (formDataRaw.visitTime as Date).toISOString();

    const action$ = this.isEditMode && this.data.visit
      ? this.visitService.updateVisit({ ...this.data.visit, clientId: formDataRaw.clientId, visitTime: visitTimeISO })
      : this.visitService.createVisit({ clientId: formDataRaw.clientId, visitTime: visitTimeISO });

    action$.subscribe({
      next: () => {
        this.dialogRef.close(true);
      },
      error: (err) => {
        console.error('Ошибка при сохранении посещения:', err);
        this.snackBar.open('Не удалось сохранить посещение. Пожалуйста, попробуйте снова.', 'Закрыть', {
          duration: 5000,
          panelClass: 'error-snackbar'
        });
      }
    });
  }
}
