import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, firstValueFrom } from 'rxjs';
import { readErrorMessage } from '../../../core/http-error';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslateService } from '../../../core/i18n/translate.service';
import { TaskState } from '../../../core/models/task-state.enum';
import { Task } from '../../../core/models/task.model';
import { TasksService } from '../../../core/services/tasks.service';
import { Badge, BadgeTone } from '../../../shared/components/badge/badge';
import { Button } from '../../../shared/components/button/button';
import { Pagination } from '../../../shared/components/pagination/pagination';
import { TaskStatus } from '../task-status/task-status';

/** Filter option rendered in the status selector. */
interface StatusOption {
  /** Value bound to the native option. */
  value: TaskState | '';
  /** Translation key for the option label. */
  key: string;
}

/**
 * Lists tasks with status filter, title search, pagination and deletion.
 */
@Component({
  selector: 'app-task-list',
  imports: [RouterLink, TranslatePipe, Badge, Button, Pagination, TaskStatus],
  templateUrl: './task-list.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './task-list.scss',
})
export class TaskList implements OnInit {
  private readonly tasksService = inject(TasksService);
  private readonly translate = inject(TranslateService);

  /** Current page items. */
  protected readonly tasks = signal<Task[]>([]);
  /** One-based index of the active page. */
  protected readonly page = signal(1);
  /** Items per page requested from the API. */
  protected readonly pageSize = signal(10);
  /** Total number of tasks across all pages. */
  protected readonly totalItems = signal(0);
  /** Total number of available pages. */
  protected readonly totalPages = signal(0);
  /** Whether a request is in flight. */
  protected readonly loading = signal(false);
  /** Readable error shown when a request fails. */
  protected readonly error = signal('');
  /** Active status filter (empty means all). */
  protected readonly statusFilter = signal<TaskState | ''>('');
  /** Debounced title search term. */
  protected readonly search = signal('');
  /** Identifier of the task being deleted, used to disable its button. */
  protected readonly deleting = signal<number | null>(null);

  /** Options of the status filter selector. */
  protected readonly statusOptions: StatusOption[] = [
    { value: '', key: 'filters.all' },
    { value: TaskState.Pending, key: 'status.pending' },
    { value: TaskState.InProgress, key: 'status.inProgress' },
    { value: TaskState.Done, key: 'status.done' },
  ];

  private readonly searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject.pipe(debounceTime(300)).subscribe((value) => {
      this.search.set(value.trim());
      this.page.set(1);
      void this.load();
    });
  }

  ngOnInit(): void {
    void this.load();
  }

  /** Loads the current page applying the active filters. */
  protected async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const result = await firstValueFrom(this.tasksService.list(this.buildFilter()));
      this.tasks.set(result.items);
      this.totalItems.set(result.totalItems);
      this.totalPages.set(result.totalPages);
      this.page.set(result.page);
    } catch (err) {
      this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
    } finally {
      this.loading.set(false);
    }
  }

  /** Queues a debounced search for the typed title. */
  protected onSearchChange(event: Event): void {
    this.searchSubject.next((event.target as HTMLInputElement).value);
  }

  /** Applies a status filter and reloads from the first page. */
  protected onStatusChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.statusFilter.set(value === '' ? '' : (Number(value) as TaskState));
    this.page.set(1);
    void this.load();
  }

  /** Navigates to the requested page. */
  protected onPageChange(page: number): void {
    this.page.set(page);
    void this.load();
  }

  /** Replaces a task in place after a successful status change. */
  protected onTaskChanged(updated: Task): void {
    this.tasks.update((list) => list.map((task) => (task.id === updated.id ? updated : task)));
  }

  /** Deletes a task after user confirmation and reloads the list. */
  protected deleteTask(task: Task): void {
    const message = this.translate.translate('tasks.deleteConfirm', { title: task.title });
    if (!window.confirm(message)) {
      return;
    }
    this.deleting.set(task.id);
    this.error.set('');
    this.tasksService.delete(task.id).subscribe({
      next: () => {
        this.deleting.set(null);
        void this.load();
      },
      error: (err) => {
        this.deleting.set(null);
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

  /** Builds the query filter from the active signals. */
  private buildFilter() {
    const status = this.statusFilter();
    return {
      page: this.page(),
      pageSize: this.pageSize(),
      search: this.search() || undefined,
      status: status === '' ? undefined : status,
    };
  }
}
