// Unit tests for the task listing component.
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslateService } from '../../../core/i18n/translate.service';
import { TaskState } from '../../../core/models/task-state.enum';
import { Task } from '../../../core/models/task.model';
import { TasksService } from '../../../core/services/tasks.service';
import { TaskList } from './task-list';

describe('TaskList', () => {
  let fixture: ComponentFixture<TaskList>;
  let tasksService: { list: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };

  const pendingTask: Task = {
    id: 1,
    title: 'Design the login flow',
    status: TaskState.Pending,
    userId: 2,
    userName: 'Luis Gómez',
    createdAt: '2026-11-01T10:00:00Z',
    updatedAt: null,
    additionalData: null,
  };

  const doneTask: Task = {
    id: 2,
    title: 'Release the API',
    status: TaskState.Done,
    userId: 1,
    userName: 'Ana Pérez',
    createdAt: '2026-11-02T10:00:00Z',
    updatedAt: null,
    additionalData: '{"priority":"Low"}',
  };

  function page(items: Task[], pageIndex = 1, totalItems = items.length): { items: Task[]; page: number; pageSize: number; totalItems: number; totalPages: number } {
    return { items, page: pageIndex, pageSize: 10, totalItems, totalPages: Math.max(1, Math.ceil(totalItems / 10)) };
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
    tasksService = {
      list: vi.fn().mockReturnValue(of(page([pendingTask, doneTask], 1, 2))),
      delete: vi.fn().mockReturnValue(of(undefined)),
    };
    await TestBed.configureTestingModule({
      imports: [TaskList],
      providers: [
        provideRouter([]),
        { provide: TasksService, useValue: tasksService },
        { provide: TranslateService, useValue: createTranslateServiceMock() },
      ],
    }).compileComponents();
  });

  /** Creates the fixture and renders the initial page. */
  async function createComponent(): Promise<void> {
    fixture = TestBed.createComponent(TaskList);
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
  }

  /** Verifies that the loaded tasks are rendered with their titles and users. */
  it('should render the task list', async () => {
    await createComponent();
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Design the login flow');
    expect(text).toContain('Release the API');
    expect(text).toContain('Luis Gómez');
    expect(text).toContain('Ana Pérez');
  });

  /** Verifies that the empty state is shown when there are no tasks. */
  it('should render the empty state when there are no tasks', async () => {
    tasksService.list.mockReturnValue(of(page([])));
    await createComponent();
    expect(fixture.nativeElement.textContent).toContain('tasks.empty');
  });

  /** Verifies that changing the status filter reloads from the first page. */
  it('should apply the status filter and reload from the first page', async () => {
    await createComponent();
    tasksService.list.mockClear();

    const select = fixture.nativeElement.querySelector('#task-status-filter') as HTMLSelectElement;
    select.value = String(TaskState.Done);
    select.dispatchEvent(new Event('change'));
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(tasksService.list).toHaveBeenCalledWith(expect.objectContaining({ status: TaskState.Done, page: 1 }));
  });

  /** Verifies that the title search is debounced before reloading. */
  it('should search by title after the debounce', async () => {
    vi.useFakeTimers();
    try {
      fixture = TestBed.createComponent(TaskList);
      fixture.detectChanges();
      tasksService.list.mockClear();

      const input = fixture.nativeElement.querySelector('#task-search') as HTMLInputElement;
      input.value = 'login';
      input.dispatchEvent(new Event('input'));

      await vi.advanceTimersByTimeAsync(300);

      expect(tasksService.list).toHaveBeenCalledWith(expect.objectContaining({ search: 'login', page: 1 }));
    } finally {
      vi.useRealTimers();
    }
  });

  /** Verifies that the next page button requests the following page. */
  it('should navigate to the next page through the pagination control', async () => {
    tasksService.list.mockReturnValue(of(page([pendingTask, doneTask], 1, 25)));
    await createComponent();
    tasksService.list.mockClear();

    const nextButton = fixture.nativeElement.querySelectorAll('.pagination__button')[1] as HTMLButtonElement;
    nextButton.click();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(tasksService.list).toHaveBeenCalledWith(expect.objectContaining({ page: 2 }));
  });

  /** Verifies that the delete action opens the modal instead of deleting directly. */
  it('should open the confirmation dialog when the user requests a deletion', async () => {
    await createComponent();

    const deleteButton = fixture.nativeElement.querySelector('button[data-variant="danger"]') as HTMLButtonElement;
    deleteButton.click();
    fixture.detectChanges();

    const dialog = fixture.nativeElement.querySelector('[role="alertdialog"]') as HTMLElement;
    expect(dialog).toBeTruthy();
    expect(dialog.textContent).toContain('tasks.deleteConfirm');
    expect(tasksService.delete).not.toHaveBeenCalled();
  });

  /** Verifies that a confirmed deletion removes the task and reloads the list. */
  it('should delete a task after confirmation and reload the list', async () => {
    await createComponent();
    tasksService.list.mockClear();

    const deleteButton = fixture.nativeElement.querySelector('button[data-variant="danger"]') as HTMLButtonElement;
    deleteButton.click();
    fixture.detectChanges();

    const confirmButton = fixture.nativeElement.querySelector(
      '[data-action="confirm"]',
    ) as HTMLButtonElement;
    confirmButton.click();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(tasksService.delete).toHaveBeenCalledWith(pendingTask.id);
    expect(tasksService.list).toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeNull();
  });

  /** Verifies that cancelling the confirmation keeps the task untouched. */
  it('should not delete a task when the user cancels the confirmation', async () => {
    await createComponent();

    const deleteButton = fixture.nativeElement.querySelector('button[data-variant="danger"]') as HTMLButtonElement;
    deleteButton.click();
    fixture.detectChanges();

    const cancelButton = fixture.nativeElement.querySelector(
      '[data-action="cancel"]',
    ) as HTMLButtonElement;
    cancelButton.click();
    fixture.detectChanges();

    expect(tasksService.delete).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeNull();
  });

  /** Verifies that a load failure surfaces a readable error message. */
  it('should surface the API error when the list cannot be loaded', async () => {
    tasksService.list.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500, error: { detail: 'Database unavailable.' } })),
    );
    await createComponent();
    expect(fixture.nativeElement.textContent).toContain('Database unavailable.');
  });
});
