import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { readErrorMessage } from '../../../core/http-error';
import { jsonDocumentValidator } from '../../../core/json-validator';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslateService } from '../../../core/i18n/translate.service';
import { TaskState } from '../../../core/models/task-state.enum';
import { CreateTaskPayload } from '../../../core/models/task.model';
import { User } from '../../../core/models/user.model';
import { TasksService } from '../../../core/services/tasks.service';
import { UsersService } from '../../../core/services/users.service';
import { Button } from '../../../shared/components/button/button';
import { Card } from '../../../shared/components/card/card';
import { Select } from '../../../shared/components/select/select';

/**
 * Reactive form to create a task assigned to an existing user.
 */
@Component({
  selector: 'app-task-create',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, Button, Card, Select],
  templateUrl: './task-create.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './task-create.scss',
})
export class TaskCreate implements OnInit {
  /** Exposes the enum to the template for the status selector. */
  protected readonly TaskState = TaskState;

  private readonly fb = inject(FormBuilder);
  private readonly tasksService = inject(TasksService);
  private readonly usersService = inject(UsersService);
  private readonly translate = inject(TranslateService);
  private readonly router = inject(Router);

  /** Available users for the assignment selector. */
  protected readonly users = signal<User[]>([]);
  /** Whether the creation request is in flight. */
  protected readonly submitting = signal(false);
  /** Readable error shown when the request fails. */
  protected readonly error = signal('');

  /** Reactive form with the create fields and client-side validation. */
  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    userId: ['', [Validators.required]],
    status: [TaskState.Pending, [Validators.required]],
    additionalData: ['', [jsonDocumentValidator]],
  });

  ngOnInit(): void {
    void this.loadUsers();
  }

  /** Loads the users available for assignment. */
  protected async loadUsers(): Promise<void> {
    try {
      const result = await firstValueFrom(this.usersService.list({ page: 1, pageSize: 100 }));
      this.users.set(result.items);
    } catch {
      // The selector stays empty and the API error is surfaced on submit.
    }
  }

  /** Creates the task and navigates back to the list on success. */
  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.error.set('');
    const payload: CreateTaskPayload = {
      title: this.form.controls.title.value.trim(),
      userId: Number(this.form.controls.userId.value),
      status: Number(this.form.controls.status.value) as TaskState,
      additionalData: this.additionalDataOrNull(),
    };
    try {
      await firstValueFrom(this.tasksService.create(payload));
      await this.router.navigate(['/tasks']);
    } catch (err) {
      this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
    } finally {
      this.submitting.set(false);
    }
  }

  /** Returns the validation message for the assigned user field. */
  protected userIdError(): string {
    const control = this.form.controls.userId;
    if (control.touched && control.invalid) {
      return this.translate.translate('common.required');
    }
    return '';
  }

  /** Returns the additional data value or null when it is empty. */
  private additionalDataOrNull(): string | null {
    const value = this.form.controls.additionalData.value.trim();
    return value === '' ? null : value;
  }
}
