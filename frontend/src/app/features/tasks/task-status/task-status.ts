import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { readErrorMessage } from '../../../core/http-error';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslateService } from '../../../core/i18n/translate.service';
import { TaskState } from '../../../core/models/task-state.enum';
import { Task } from '../../../core/models/task.model';
import { TasksService } from '../../../core/services/tasks.service';

/**
 * Inline status selector that rejects the forbidden Pending -> Done transition
 * and persists the change through the API.
 */
@Component({
  selector: 'app-task-status',
  imports: [TranslatePipe],
  template: `
    <select
      class="status-select"
      [value]="task().status"
      (change)="onChange($event)"
      [attr.aria-label]="'tasks.changeStatus' | translate"
    >
      <option
        [value]="TaskState.Pending"
        [selected]="task().status === TaskState.Pending"
      >
        {{ 'status.pending' | translate }}
      </option>
      <option
        [value]="TaskState.InProgress"
        [selected]="task().status === TaskState.InProgress"
      >
        {{ 'status.inProgress' | translate }}
      </option>
      <option [value]="TaskState.Done" [selected]="task().status === TaskState.Done">
        {{ 'status.done' | translate }}
      </option>
    </select>
    @if (error()) {
      <p class="status-select__error">{{ error() }}</p>
    }
  `,
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './task-status.scss',
})
export class TaskStatus {
  /** Exposes the enum to the template for option values. */
  protected readonly TaskState = TaskState;

  private readonly tasks = inject(TasksService);
  private readonly translate = inject(TranslateService);

  /** Task whose status is being managed. */
  readonly task = input.required<Task>();

  /** Emits the updated task after a successful status change. */
  readonly changed = output<Task>();

  /** Error message shown when the transition is rejected or the call fails. */
  protected readonly error = signal('');

  /** Handles a status selection and validates the transition locally. */
  protected onChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const next = Number(select.value) as TaskState;
    const current = this.task().status;

    if (current === TaskState.Pending && next === TaskState.Done) {
      this.error.set(this.translate.translate('tasks.statusTransitionError'));
      select.value = String(current);
      return;
    }

    this.error.set('');
    this.tasks.changeStatus(this.task().id, next).subscribe({
      next: (updated) => this.changed.emit(updated),
      error: (err) => {
        select.value = String(current);
        this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
      },
    });
  }
}
