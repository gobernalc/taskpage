// Unit tests for the read-only task detail view.
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslateService } from '../../../core/i18n/translate.service';
import { TaskState } from '../../../core/models/task-state.enum';
import { Task } from '../../../core/models/task.model';
import { TasksService } from '../../../core/services/tasks.service';
import { TaskDetail } from './task-detail';

describe('TaskDetail', () => {
  let fixture: ComponentFixture<TaskDetail>;
  let tasksService: {
    getById: ReturnType<typeof vi.fn>;
    delete: ReturnType<typeof vi.fn>;
  };
  let router: Router;
  let routeId: string | null = '1';

  const task: Task = {
    id: 1,
    title: 'Design the login flow',
    status: TaskState.InProgress,
    userId: 2,
    userName: 'Luis Gómez',
    createdAt: '2026-11-01T10:00:00Z',
    updatedAt: '2026-11-02T11:30:00Z',
    additionalData: '{"priority":"High"}',
  };

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
    routeId = '1';
    tasksService = {
      getById: vi.fn().mockReturnValue(of(task)),
      delete: vi.fn().mockReturnValue(of(undefined)),
    };
    await TestBed.configureTestingModule({
      imports: [TaskDetail],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            get snapshot() {
              return {
                paramMap: convertToParamMap(routeId === null ? {} : { id: routeId }),
              };
            },
          },
        },
        { provide: TasksService, useValue: tasksService },
        { provide: TranslateService, useValue: createTranslateServiceMock() },
      ],
    }).compileComponents();
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(TaskDetail);
  });

  /** Renders the component and waits for the asynchronous load. */
  async function render(): Promise<void> {
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
  }

  /** Verifies that the loaded task is rendered with its metadata and JSON data. */
  it('should render the task detail with its metadata and additional data', async () => {
    await render();

    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('Design the login flow');
    expect(text).toContain('Luis Gómez');
    expect(text).toContain('{"priority":"High"}');
    expect(text).toContain('status.inProgress');
    expect(tasksService.getById).toHaveBeenCalledWith(1);
  });

  /** Verifies that an invalid route id sends the user back to the task list. */
  it('should navigate back to the task list when the route id is invalid', async () => {
    routeId = 'not-a-number';
    await render();

    expect(tasksService.getById).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/tasks']);
  });

  /** Verifies that deletion only runs after the confirmation dialog is accepted. */
  it('should delete the task after the confirmation dialog is accepted', async () => {
    await render();

    const deleteButton = fixture.nativeElement.querySelector(
      'button[data-variant="danger"]',
    ) as HTMLButtonElement;
    deleteButton.click();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeTruthy();
    expect(tasksService.delete).not.toHaveBeenCalled();

    const confirmButton = fixture.nativeElement.querySelector(
      '[data-action="confirm"]',
    ) as HTMLButtonElement;
    confirmButton.click();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();

    expect(tasksService.delete).toHaveBeenCalledWith(task.id);
    expect(router.navigate).toHaveBeenCalledWith(['/tasks']);
  });

  /** Verifies that cancelling the confirmation keeps the task untouched. */
  it('should not delete the task when the confirmation is cancelled', async () => {
    await render();

    (fixture.nativeElement.querySelector(
      'button[data-variant="danger"]',
    ) as HTMLButtonElement).click();
    fixture.detectChanges();

    (fixture.nativeElement.querySelector(
      '[data-action="cancel"]',
    ) as HTMLButtonElement).click();
    fixture.detectChanges();

    expect(tasksService.delete).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('[role="alertdialog"]')).toBeNull();
  });

  /** Verifies that a load failure surfaces a readable error message. */
  it('should surface the API error when the task cannot be loaded', async () => {
    tasksService.getById.mockReturnValue(
      throwError(
        () => new HttpErrorResponse({ status: 404, error: { detail: 'Task not found' } }),
      ),
    );
    await render();

    expect((fixture.nativeElement.textContent as string)).toContain('Task not found');
  });
});
