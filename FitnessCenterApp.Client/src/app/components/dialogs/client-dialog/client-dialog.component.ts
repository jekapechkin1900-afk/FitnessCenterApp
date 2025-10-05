import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

import { Client } from '../../../core/models/client.model';
import { ClientService } from '../../../core/services/api/client.service';

@Component({
  selector: 'app-client-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './client-dialog.component.html',
  styleUrls: ['./client-dialog.component.css']
})
export class ClientDialogComponent {
  clientForm: FormGroup;
  isEditMode: boolean;

  constructor(
    private fb: FormBuilder,
    private clientService: ClientService,
    public dialogRef: MatDialogRef<ClientDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Client | null
  ) {
    this.isEditMode = !!data;
    this.clientForm = this.fb.group({
      fullName: [data?.fullName || '', Validators.required],
      email: [data?.email || '', [Validators.required, Validators.email]],
      phoneNumber: [data?.phoneNumber || '']
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.clientForm.invalid) {
      return;
    }

    const formData = this.clientForm.value;

    if (this.isEditMode && this.data) {
      const updatedClient: Client = { ...this.data, ...formData };
      this.clientService.updateClient(updatedClient).subscribe(() => this.dialogRef.close(true));
    } else {
      this.clientService.createClient(formData).subscribe(() => this.dialogRef.close(true));
    }
  }
}
