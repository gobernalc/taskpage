// Unit tests for the user listing component.
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslateService } from '../../../core/i18n/translate.service';
import { User } from '../../../core/models/user.model';
import { UsersService } from '../../../core/services/users.service';
import { UserList } from './user-list';

describe('UserList', () => {
  let fixture: ComponentFixture<UserList>;
  let usersService: { list: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };

  const users: User[] = [
    { id: 1, name: 'Ana Pérez', email: 'ana.perez@example.com', createdAt: '2026-11-01T10:00:00Z', updatedAt: null },
    { id: 2, name: 'Luis Gómez', email: 'luis.gomez@example.com', createdAt: '2026-11-02T10:00:00Z', updatedAt: null },
  ];

  function page(items: User[]): { items: User[]; page: number; pageSize: number; totalItems: number; totalPages: number } {
    return { items, page: 1, pageSize: 10, totalItems: items.length, totalPages: 1 };
  }

  /** Creates a minimal TranslateService double returning the key itself. */
  function createTranslateServiceMock() {
    return {
      language: signal('es'),
      translations: signal<Record<string, string>>({}),
      translate: vi.fn((key: string) => key),
      load: vi.fn(async () => undefined),
      toggle: vi.fn(async () => undefined),
    } as unknown as TranslateService;
  }

  beforeEach(async () => {
    usersService = {
      list: vi.fn().mockReturnValue(of(page(users))),
      delete: vi.fn().mockReturnValue(of(undefined)),
    };
    await TestBed.configureTestingModule({
      imports: [UserList],
      providers: [
        provideRouter([]),
        { provide: UsersService, useValue: usersService },
        { provide: TranslateService, useValue: createTranslateServiceMock() },
      ],
    }).compileComponents();
  });

  /** Creates the fixture and renders the initial page. */
  async function createComponent(): Promise<void> {
    fixture = TestBed.createComponent(UserList);
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
  }

  /** Verifies that the loaded users are rendered with their names and emails. */
  it('should render the user list', async () => {
    await createComponent();
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Ana Pérez');
    expect(text).toContain('ana.perez@example.com');
    expect(text).toContain('Luis Gómez');
    expect(text).toContain('luis.gomez@example.com');
  });

  /** Verifies that the empty state is shown when there are no users. */
  it('should render the empty state when there are no users', async () => {
    usersService.list.mockReturnValue(of(page([])));
    await createComponent();
    expect(fixture.nativeElement.textContent).toContain('users.empty');
  });

  /** Verifies that the search is debounced before reloading. */
  it('should search by name or email after the debounce', async () => {
    vi.useFakeTimers();
    try {
      fixture = TestBed.createComponent(UserList);
      fixture.detectChanges();
      usersService.list.mockClear();

      const input = fixture.nativeElement.querySelector('#user-search') as HTMLInputElement;
      input.value = 'ana';
      input.dispatchEvent(new Event('input'));

      await vi.advanceTimersByTimeAsync(300);

      expect(usersService.list).toHaveBeenCalledWith(expect.objectContaining({ search: 'ana', page: 1 }));
    } finally {
      vi.useRealTimers();
    }
  });

  /** Verifies that the delete action opens the modal instead of deleting directly. */
  it('should open the confirmation dialog when the user requests a deletion', async () => {
    await createComponent();

    const deleteButton = fixture.nativeElement.querySelector('button[data-variant="danger"]') as HTMLButtonElement;
    deleteButton.click();
    fixture.detectChanges();

    const dialog = fixture.nativeElement.querySelector('[role="alertdialog"]') as HTMLElement;
    expect(dialog).toBeTruthy();
    expect(dialog.textContent).toContain('users.deleteConfirm');
    expect(usersService.delete).not.toHaveBeenCalled();
  });

  /** Verifies that a confirmed deletion removes the user and reloads the list. */
  it('should delete a user after confirmation and reload the list', async () => {
    await createComponent();
    usersService.list.mockClear();

    const deleteButton = fixture.nativeElement.querySelector('button[data-variant="danger"]') as HTMLButtonElement;
    deleteButton.click();
    fixture.detectChanges();

    const confirmButton = fixture.nativeElement.querySelector(
      '[data-action="confirm"]',
    ) as HTMLButtonElement;
    confirmButton.click();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(usersService.delete).toHaveBeenCalledWith(users[0].id);
    expect(usersService.list).toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeNull();
  });

  /** Verifies that cancelling the confirmation keeps the user untouched. */
  it('should not delete a user when the user cancels the confirmation', async () => {
    await createComponent();

    const deleteButton = fixture.nativeElement.querySelector('button[data-variant="danger"]') as HTMLButtonElement;
    deleteButton.click();
    fixture.detectChanges();

    const cancelButton = fixture.nativeElement.querySelector(
      '[data-action="cancel"]',
    ) as HTMLButtonElement;
    cancelButton.click();
    fixture.detectChanges();

    expect(usersService.delete).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeNull();
  });

  /** Verifies that a load failure surfaces a readable error message. */
  it('should surface the API error when the list cannot be loaded', async () => {
    usersService.list.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500, error: { detail: 'Database unavailable.' } })),
    );
    await createComponent();
    expect(fixture.nativeElement.textContent).toContain('Database unavailable.');
  });
});
