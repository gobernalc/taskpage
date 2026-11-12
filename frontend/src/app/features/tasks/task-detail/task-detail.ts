import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { readErrorMessage } from '../../../core/http-error';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslateService } from '../../../core/i18n/translate.service';
import { TaskState } from '../../../core/models/task-state.enum';
import { Task } from '../../../core/models/task.model';
import { TasksService } from '../../../core/services/tasks.service';
import { Badge, BadgeTone } from '../../../shared/components/badge/badge';
import { Button } from '../../../shared/components/button/button';
import { Card } from '../../../shared/components/card/card';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';

/**
 * Read-only view with the full detail of a task and the shortcut to its editor.
 */
@Component({
  selector: 'app-task-detail',
  imports: [RouterLink, TranslatePipe, Badge, Button, Card, ConfirmDialog],
  templateUrl: './task-detail.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './task-detail.scss',
})
export class TaskDetail implements OnInit {
  private readonly tasksService = inject(TasksService);
  private readonly translate = inject(TranslateService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  /** Task being displayed. */
  protected readonly task = signal<Task | null>(null);
  /** Whether the task is being loaded. */
  protected readonly loading = signal(false);
  /** Whether the delete request is in flight. */
  protected readonly deleting = signal(false);
  /** Whether the delete confirmation dialog is open. */
  protected readonly confirmingDelete = signal(false);
  /** Readable error shown when the request fails. */
  protected readonly error = signal('');

  /** Dialog heading for the delete confirmation. */
  protected readonly deleteTitle = computed(() => this.translate.translate('tasks.deleteTitle'));
  /** Dialog confirm label, matching the delete action. */
  protected readonly deleteLabel = computed(() => this.translate.translate('tasks.delete'));
  /** Dialog cancel label. */
  protected readonly cancelLabel = computed(() => this.translate.translate('common.cancel'));
  /** Dialog body interpolated with the title of the loaded task. */
  protected readonly deleteMessage = computed(() => {
    const task = this.task();
    return task ? this.translate.translate('tasks.deleteConfirm', { title: task.title }) : '';
  });

  ngOnInit(): void {
    void this.load();
  }

  /** Loads the task referenced by the route parameter. */
  protected async load(): Promise<void> {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id <= 0) {
      await this.router.navigate(['/tasks']);
      return;
    }
    this.loading.set(true);
    this.error.set('');
    try {
      this.task.set(await firstValueFrom(this.tasksService.getById(id)));
    } catch (err) {
      this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
    } finally {
      this.loading.set(false);
    }
  }

  /**
   * Formats a timestamp with the active UI language.
   * @param value ISO timestamp to format.
   * @returns Localized date and time, or an empty string when there is no value.
   */
  protected formatDate(value: string | null): string {
    if (!value) {
      return '';
    }
    const locale = this.translate.language() === 'es' ? 'es-CO' : 'en-US';
    return new Date(value).toLocaleString(locale, {
      dateStyle: 'medium',
      timeStyle: 'short',
    });
  }

  /** Opens the delete confirmation dialog for the loaded task. */
  protected requestDelete(): void {
    if (this.task()) {
      this.confirmingDelete.set(true);
    }
  }

  /** Closes the delete confirmation dialog without deleting anything. */
  protected cancelDelete(): void {
    this.confirmingDelete.set(false);
  }

  /** Deletes the confirmed task and navigates back to the list. */
  protected confirmDelete(): void {
    const task = this.task();
    if (!task) {
      return;
    }
    this.deleting.set(true);
    this.error.set('');
    this.tasksService.delete(task.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.confirmingDelete.set(false);
        void this.router.navigate(['/tasks']);
      },
      error: (err) => {
        this.deleting.set(false);
        this.confirmingDelete.set(false);
        this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
      },
    });
  }

  /** Returns the translation key of a task status. */
  protected statusKey(status: TaskState): string {
    switch (status) {
      case TaskState.InProgress:
        return 'status.inProgress';
      case TaskState.Done:
        return 'status.done';
      default:
        return 'status.pending';
    }
  }

  /** Returns the badge tone associated with a task status. */
  protected statusTone(status: TaskState): BadgeTone {
    switch (status) {
      case TaskState.InProgress:
        return 'warning';
      case TaskState.Done:
        return 'success';
      default:
        return 'info';
    }
  }
}
