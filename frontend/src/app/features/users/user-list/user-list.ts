import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, firstValueFrom } from 'rxjs';
import { readErrorMessage } from '../../../core/http-error';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslateService } from '../../../core/i18n/translate.service';
import { User } from '../../../core/models/user.model';
import { UsersService } from '../../../core/services/users.service';
import { Button } from '../../../shared/components/button/button';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { Pagination } from '../../../shared/components/pagination/pagination';

/**
 * Lists users with search, pagination and deletion.
 */
@Component({
  selector: 'app-user-list',
  imports: [RouterLink, TranslatePipe, Button, ConfirmDialog, Pagination],
  templateUrl: './user-list.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './user-list.scss',
})
export class UserList implements OnInit {
  private readonly usersService = inject(UsersService);
  private readonly translate = inject(TranslateService);

  /** Current page items. */
  protected readonly users = signal<User[]>([]);
  /** One-based index of the active page. */
  protected readonly page = signal(1);
  /** Items per page requested from the API. */
  protected readonly pageSize = signal(10);
  /** Total number of users across all pages. */
  protected readonly totalItems = signal(0);
  /** Total number of available pages. */
  protected readonly totalPages = signal(0);
  /** Whether a request is in flight. */
  protected readonly loading = signal(false);
  /** Readable error shown when a request fails. */
  protected readonly error = signal('');
  /** Debounced search term. */
  protected readonly search = signal('');
  /** Identifier of the user being deleted, used to disable its button. */
  protected readonly deleting = signal<number | null>(null);
  /** User waiting for confirmation in the delete dialog. */
  protected readonly pendingDelete = signal<User | null>(null);

  /** Dialog heading for the delete confirmation. */
  protected readonly deleteTitle = computed(() => this.translate.translate('users.deleteTitle'));
  /** Dialog confirm label, matching the delete button of the list. */
  protected readonly deleteLabel = computed(() => this.translate.translate('users.delete'));
  /** Dialog cancel label. */
  protected readonly cancelLabel = computed(() => this.translate.translate('common.cancel'));
  /** Dialog body interpolated with the name of the pending user. */
  protected readonly deleteMessage = computed(() => {
    const user = this.pendingDelete();
    return user ? this.translate.translate('users.deleteConfirm', { name: user.name }) : '';
  });

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

  /** Loads the current page applying the active search. */
  protected async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const result = await firstValueFrom(this.usersService.list(this.buildFilter()));
      this.users.set(result.items);
      this.totalItems.set(result.totalItems);
      this.totalPages.set(result.totalPages);
      this.page.set(result.page);
    } catch (err) {
      this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
    } finally {
      this.loading.set(false);
    }
  }

  /** Queues a debounced search for the typed name or email. */
  protected onSearchChange(event: Event): void {
    this.searchSubject.next((event.target as HTMLInputElement).value);
  }

  /** Navigates to the requested page. */
  protected onPageChange(page: number): void {
    this.page.set(page);
    void this.load();
  }

  /** Opens the delete confirmation dialog for the selected user. */
  protected requestDelete(user: User): void {
    this.pendingDelete.set(user);
  }

  /** Closes the delete confirmation dialog without deleting anything. */
  protected cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  /** Deletes the user confirmed in the dialog and reloads the list. */
  protected confirmDelete(): void {
    const user = this.pendingDelete();
    if (!user) {
      return;
    }
    this.deleting.set(user.id);
    this.error.set('');
    this.usersService.delete(user.id).subscribe({
      next: () => {
        this.deleting.set(null);
        this.pendingDelete.set(null);
        void this.load();
      },
      error: (err) => {
        this.deleting.set(null);
        this.pendingDelete.set(null);
        this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
      },
    });
  }

  /** Builds the query filter from the active signals. */
  private buildFilter() {
    return {
      page: this.page(),
      pageSize: this.pageSize(),
      search: this.search() || undefined,
    };
  }
}
