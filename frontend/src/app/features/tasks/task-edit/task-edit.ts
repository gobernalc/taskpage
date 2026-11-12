import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { readErrorMessage } from '../../../core/http-error';
import { jsonDocumentValidator } from '../../../core/json-validator';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslateService } from '../../../core/i18n/translate.service';
import { UpdateTaskPayload } from '../../../core/models/task.model';
import { User } from '../../../core/models/user.model';
import { TasksService } from '../../../core/services/tasks.service';
import { UsersService } from '../../../core/services/users.service';
import { Button } from '../../../shared/components/button/button';
import { Card } from '../../../shared/components/card/card';
import { Select } from '../../../shared/components/select/select';

/**
 * Reactive form to edit the editable values of an existing task.
 */
@Component({
  selector: 'app-task-edit',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, Button, Card, Select],
  templateUrl: './task-edit.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './task-edit.scss',
})
export class TaskEdit implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly tasksService = inject(TasksService);
  private readonly usersService = inject(UsersService);
  private readonly translate = inject(TranslateService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  /** Available users for the assignment selector. */
  protected readonly users = signal<User[]>([]);
  /** Whether the task is being loaded. */
  protected readonly loading = signal(false);
  /** Whether the update request is in flight. */
  protected readonly submitting = signal(false);
  /** Readable error shown when a request fails. */
  protected readonly error = signal('');

  /** Identifier of the task being edited. */
  private taskId = 0;

  /** Reactive form with the editable fields and client-side validation. */
  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    userId: ['', [Validators.required]],
    additionalData: ['', [jsonDocumentValidator]],
  });

  ngOnInit(): void {
    void this.initialize();
  }

  /** Loads the users and the task before rendering the form. */
  private async initialize(): Promise<void> {
    await Promise.all([this.loadUsers(), this.loadTask()]);
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

  /** Loads the task and populates the form with its current values. */
  private async loadTask(): Promise<void> {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id <= 0) {
      await this.router.navigate(['/tasks']);
      return;
    }
    this.taskId = id;
    this.loading.set(true);
    try {
      const task = await firstValueFrom(this.tasksService.getById(id));
      this.form.setValue({
        title: task.title,
        userId: String(task.userId),
        additionalData: task.additionalData ?? '',
      });
    } catch (err) {
      this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
    } finally {
      this.loading.set(false);
    }
  }

  /** Saves the changes and navigates back to the list on success. */
  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.error.set('');
    const payload: UpdateTaskPayload = {
      title: this.form.controls.title.value.trim(),
      userId: Number(this.form.controls.userId.value),
      additionalData: this.additionalDataOrNull(),
    };
    try {
      await firstValueFrom(this.tasksService.update(this.taskId, payload));
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
