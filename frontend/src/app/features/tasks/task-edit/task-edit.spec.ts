// Unit tests for the task edition form.
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslateService } from '../../../core/i18n/translate.service';
import { TaskState } from '../../../core/models/task-state.enum';
import { Task } from '../../../core/models/task.model';
import { User } from '../../../core/models/user.model';
import { TasksService } from '../../../core/services/tasks.service';
import { UsersService } from '../../../core/services/users.service';
import { TaskEdit } from './task-edit';

describe('TaskEdit', () => {
  let fixture: ComponentFixture<TaskEdit>;
  let tasksService: { getById: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let usersService: { list: ReturnType<typeof vi.fn> };

  const user: User = {
    id: 2,
    name: 'Luis Gómez',
    email: 'luis.gomez@example.com',
    createdAt: '2026-11-01T10:00:00Z',
    updatedAt: null,
  };

  const task: Task = {
    id: 1,
    title: 'Design the login flow',
    status: TaskState.Pending,
    userId: 2,
    userName: user.name,
    createdAt: '2026-11-01T10:00:00Z',
    updatedAt: null,
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
    tasksService = { getById: vi.fn().mockReturnValue(of(task)), update: vi.fn() };
    usersService = { list: vi.fn().mockReturnValue(of({ items: [user], page: 1, pageSize: 100, totalItems: 1, totalPages: 1 })) };
    await TestBed.configureTestingModule({
      imports: [TaskEdit],
      providers: [
        provideRouter([]),
        { provide: TasksService, useValue: tasksService },
        { provide: UsersService, useValue: usersService },
        { provide: TranslateService, useValue: createTranslateServiceMock() },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: (key: string) => (key === 'id' ? '1' : null) } } } },
      ],
    }).compileComponents();
  });

  /** Creates the fixture and waits for the initial task and users load. */
  async function createComponent(): Promise<void> {
    fixture = TestBed.createComponent(TaskEdit);
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
  }

  /** Sets the value of an input and notifies the reactive form. */
  function setFieldValue(selector: string, value: string): void {
    const element = fixture.nativeElement.querySelector(selector) as HTMLInputElement;
    element.value = value;
    element.dispatchEvent(new Event('input'));
  }

  /** Submits the rendered form. */
  async function submitForm(): Promise<void> {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
  }

  /** Verifies that the task and users are loaded and the form is populated. */
  it('should load the task and populate the form', async () => {
    await createComponent();
    expect(tasksService.getById).toHaveBeenCalledWith(1);
    expect(fixture.nativeElement.querySelector('#task-title').value).toBe('Design the login flow');
    expect(fixture.nativeElement.textContent).toContain('Luis Gómez');
  });

  /** Verifies that saving sends the update request and navigates back. */
  it('should save the changes and navigate back to the list', async () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    tasksService.update.mockReturnValue(of({ ...task, title: 'Updated title' }));
    await createComponent();

    setFieldValue('#task-title', 'Updated title');
    fixture.detectChanges();
    await submitForm();

    expect(tasksService.update).toHaveBeenCalledWith(1, {
      title: 'Updated title',
      userId: 2,
      additionalData: '{"priority":"High"}',
    });
    expect(navigateSpy).toHaveBeenCalledWith(['/tasks']);
  });

  /** Verifies that an invalid form blocks the update request. */
  it('should not submit when the form is invalid', async () => {
    await createComponent();
    setFieldValue('#task-title', '');
    fixture.detectChanges();
    await submitForm();
    expect(tasksService.update).not.toHaveBeenCalled();
  });

  /** Verifies that a load failure surfaces a readable error message. */
  it('should surface the API error when the task cannot be loaded', async () => {
    tasksService.getById.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 404, error: { detail: 'The task was not found.' } })),
    );
    await createComponent();
    expect(fixture.nativeElement.textContent).toContain('The task was not found.');
  });
});
