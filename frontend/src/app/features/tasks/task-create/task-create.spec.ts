// Unit tests for the task creation form.
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslateService } from '../../../core/i18n/translate.service';
import { TaskState } from '../../../core/models/task-state.enum';
import { Task } from '../../../core/models/task.model';
import { User } from '../../../core/models/user.model';
import { TasksService } from '../../../core/services/tasks.service';
import { UsersService } from '../../../core/services/users.service';
import { TaskCreate } from './task-create';

describe('TaskCreate', () => {
  let fixture: ComponentFixture<TaskCreate>;
  let tasksService: { create: ReturnType<typeof vi.fn> };
  let usersService: { list: ReturnType<typeof vi.fn> };

  const user: User = {
    id: 2,
    name: 'Luis Gómez',
    email: 'luis.gomez@example.com',
    createdAt: '2026-11-01T10:00:00Z',
    updatedAt: null,
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
    tasksService = { create: vi.fn() };
    usersService = { list: vi.fn().mockReturnValue(of({ items: [user], page: 1, pageSize: 100, totalItems: 1, totalPages: 1 })) };
    await TestBed.configureTestingModule({
      imports: [TaskCreate],
      providers: [
        provideRouter([]),
        { provide: TasksService, useValue: tasksService },
        { provide: UsersService, useValue: usersService },
        { provide: TranslateService, useValue: createTranslateServiceMock() },
      ],
    }).compileComponents();
  });

  /** Creates the fixture and waits for the initial users load. */
  async function createComponent(): Promise<void> {
    fixture = TestBed.createComponent(TaskCreate);
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
  }

  /** Sets the value of an input or textarea and notifies the reactive form. */
  function setFieldValue(selector: string, value: string): void {
    const element = fixture.nativeElement.querySelector(selector) as HTMLInputElement | HTMLTextAreaElement;
    element.value = value;
    element.dispatchEvent(new Event('input'));
  }

  /** Sets the value of the assigned user select and notifies the reactive form. */
  function setSelectValue(value: string): void {
    const select = fixture.nativeElement.querySelector('#task-user') as HTMLSelectElement;
    select.value = value;
    select.dispatchEvent(new Event('change'));
  }

  /** Submits the rendered form. */
  async function submitForm(): Promise<void> {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
  }

  /** Verifies that the available users are loaded into the assignment selector. */
  it('should load the available users for the selector', async () => {
    await createComponent();
    expect(usersService.list).toHaveBeenCalledWith({ page: 1, pageSize: 100 });
    expect(fixture.nativeElement.textContent).toContain('Luis Gómez');
  });

  /** Verifies that an invalid form blocks the creation request. */
  it('should not submit when the form is invalid', async () => {
    await createComponent();
    await submitForm();
    expect(tasksService.create).not.toHaveBeenCalled();
  });

  /** Verifies that a valid submission trims the title and navigates back to the list. */
  it('should create a task with the trimmed payload and navigate back', async () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const created: Task = { id: 3, title: 'New task', status: TaskState.Pending, userId: 2, userName: user.name, createdAt: '2026-11-02T10:00:00Z', updatedAt: null, additionalData: '{"priority":"High"}' };
    tasksService.create.mockReturnValue(of(created));
    await createComponent();

    setFieldValue('#task-title', '  New task  ');
    setSelectValue('2');
    setFieldValue('#task-additional-data', '{"priority":"High"}');
    fixture.detectChanges();
    await submitForm();

    expect(tasksService.create).toHaveBeenCalledWith({
      title: 'New task',
      userId: 2,
      status: TaskState.Pending,
      additionalData: '{"priority":"High"}',
    });
    expect(navigateSpy).toHaveBeenCalledWith(['/tasks']);
  });

  /** Verifies that an API failure surfaces a readable error message. */
  it('should surface the API error when creation fails', async () => {
    tasksService.create.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 409, error: { detail: 'The title already exists.' } })),
    );
    await createComponent();

    setFieldValue('#task-title', 'Duplicate');
    setSelectValue('2');
    fixture.detectChanges();
    await submitForm();

    expect(fixture.nativeElement.textContent).toContain('The title already exists.');
  });

  /** Verifies that invalid JSON in the additional data field blocks submission. */
  it('should block submission when the additional data is not valid JSON', async () => {
    await createComponent();
    setFieldValue('#task-title', 'Valid title');
    setSelectValue('2');
    setFieldValue('#task-additional-data', '{not-json}');
    fixture.detectChanges();
    await submitForm();

    expect(tasksService.create).not.toHaveBeenCalled();
  });
});
