// Unit tests for the inline task status selector.
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { TranslateService } from '../../../core/i18n/translate.service';
import { TaskState } from '../../../core/models/task-state.enum';
import { Task } from '../../../core/models/task.model';
import { TasksService } from '../../../core/services/tasks.service';
import { TaskStatus } from './task-status';

describe('TaskStatus', () => {
  let fixture: ComponentFixture<TaskStatus>;
  let tasksService: { changeStatus: ReturnType<typeof vi.fn> };

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
    tasksService = { changeStatus: vi.fn() };
    await TestBed.configureTestingModule({
      imports: [TaskStatus],
      providers: [
        { provide: TasksService, useValue: tasksService },
        { provide: TranslateService, useValue: createTranslateServiceMock() },
      ],
    }).compileComponents();
  });

  /** Creates the fixture with the given task assigned as input. */
  function createComponent(task: Task): void {
    fixture = TestBed.createComponent(TaskStatus);
    fixture.componentRef.setInput('task', task);
    fixture.detectChanges();
  }

  /** Returns the select element rendered by the component. */
  function selectElement(): HTMLSelectElement {
    return fixture.nativeElement.querySelector('select') as HTMLSelectElement;
  }

  /** Verifies that the selector renders one option per status and receives the task. */
  it('should render one option per task status', () => {
    createComponent(pendingTask);
    const options = fixture.nativeElement.querySelectorAll('option');
    expect(options.length).toBe(3);
    expect(options[0].value).toBe(String(TaskState.Pending));
    expect(options[1].value).toBe(String(TaskState.InProgress));
    expect(options[2].value).toBe(String(TaskState.Done));
    expect(fixture.componentInstance.task().status).toBe(TaskState.Pending);
  });

  /** Verifies that the forbidden Pending -> Done transition is rejected locally. */
  it('should reject the Pending to Done transition without calling the API', () => {
    createComponent(pendingTask);
    const select = selectElement();
    select.value = String(TaskState.Done);
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(tasksService.changeStatus).not.toHaveBeenCalled();
    expect(select.value).toBe(String(TaskState.Pending));
    expect(fixture.nativeElement.textContent).toContain('tasks.statusTransitionError');
  });

  /** Verifies that an allowed transition is persisted and the updated task is emitted. */
  it('should persist an allowed transition and emit the updated task', () => {
    const updated: Task = { ...pendingTask, status: TaskState.InProgress };
    tasksService.changeStatus.mockReturnValue(of(updated));
    const changed = vi.fn();
    createComponent(pendingTask);
    fixture.componentInstance.changed.subscribe(changed);

    const select = selectElement();
    select.value = String(TaskState.InProgress);
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(tasksService.changeStatus).toHaveBeenCalledWith(pendingTask.id, TaskState.InProgress);
    expect(changed).toHaveBeenCalledWith(updated);
  });

  /** Verifies that an API failure surfaces a readable error and restores the selector. */
  it('should surface the API error and restore the previous status when the change fails', () => {
    tasksService.changeStatus.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 400, error: { detail: 'Invalid transition' } })),
    );
    createComponent(pendingTask);

    const select = selectElement();
    select.value = String(TaskState.InProgress);
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Invalid transition');
    expect(select.value).toBe(String(TaskState.Pending));
  });
});
