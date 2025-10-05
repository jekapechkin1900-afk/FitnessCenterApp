import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { provideNativeDateAdapter } from '@angular/material/core';

import { Client } from '../../../core/models/client.model';
import { Membership } from '../../../core/models/membership.model';
import { MembershipService } from '../../../core/services/api/membership.service';

export interface MembershipDialogData {
  clients: Client[];
  membership?: Membership;
}

@Component({
  selector: 'app-membership-dialog',
  standalone: true,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatSelectModule, MatDatepickerModule
  ],
  templateUrl: './membership-dialog.component.html',
  styleUrls: ['./membership-dialog.component.css']
})
export class MembershipDialogComponent implements OnInit {
  membershipForm: FormGroup;
  clients: Client[];
  isEditMode: boolean; 

  constructor(
    private fb: FormBuilder,
    private membershipService: MembershipService,
    public dialogRef: MatDialogRef<MembershipDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MembershipDialogData
  ) {
    this.clients = data.clients;
    this.isEditMode = !!data.membership;
    this.membershipForm = this.fb.group({
      clientId: ['', Validators.required],
      type: ['', Validators.required],
      price: [null, [Validators.required, Validators.min(0)]],
      startDate: [new Date(), Validators.required],
      endDate: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    if (this.isEditMode && this.data.membership) {
      this.membershipForm.patchValue({
        clientId: this.data.membership.clientId,
        type: this.data.membership.type,
        price: this.data.membership.price,
        startDate: new Date(this.data.membership.startDate),
        endDate: new Date(this.data.membership.endDate)
      });
    }
  } 

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.membershipForm.invalid) {
      return;
    }

    const formDataRaw = this.membershipForm.value;

    const formData = {
      ...formDataRaw,
      startDate: formDataRaw.startDate.toISOString(),
      endDate: formDataRaw.endDate.toISOString()
    };

    if (this.isEditMode && this.data.membership) {
      const updatedMembership = {
        id: this.data.membership.id,
        ...formData
      };
      this.membershipService.updateMembership(updatedMembership).subscribe(() => this.dialogRef.close(true));
    } else {
      this.membershipService.createMembership(formData).subscribe(() => this.dialogRef.close(true));
    }
  }
}
