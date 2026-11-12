// Unit tests for the tasks HTTP service.
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TaskState } from '../models/task-state.enum';
import { Task } from '../models/task.model';
import { TasksService } from './tasks.service';

describe('TasksService', () => {
  let service: TasksService;
  let httpMock: HttpTestingController;

  const baseUrl = 'http://localhost:5017/api';

  const task: Task = {
    id: 1,
    title: 'Design the login flow',
    status: TaskState.Pending,
    userId: 2,
    userName: 'Luis Gómez',
    createdAt: '2026-11-01T10:00:00Z',
    updatedAt: null,
    additionalData: '{"priority":"High"}',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(TasksService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  /** Verifies that the list request sends a GET with all the expected query params. */
  it('should list tasks with filters, search, sorting and pagination params', () => {
    service
      .list({ status: TaskState.InProgress, userId: 2, priority: 'high', search: 'login', sort: 'title', order: 'desc', page: 1, pageSize: 10 })
      .subscribe((result) => {
        expect(result.items).toEqual([task]);
      });

    const request = httpMock.expectOne((req) => req.url === `${baseUrl}/tasks`);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('status')).toBe(String(TaskState.InProgress));
    expect(request.request.params.get('userId')).toBe('2');
    expect(request.request.params.get('priority')).toBe('high');
    expect(request.request.params.get('search')).toBe('login');
    expect(request.request.params.get('sort')).toBe('title');
    expect(request.request.params.get('order')).toBe('desc');
    expect(request.request.params.get('page')).toBe('1');
    expect(request.request.params.get('pageSize')).toBe('10');
    request.flush({ items: [task], page: 1, pageSize: 10, totalItems: 1, totalPages: 1 });
  });

  /** Verifies that omitted filters are not sent as query params. */
  it('should list tasks without query params when no filter is provided', () => {
    service.list().subscribe();
    const request = httpMock.expectOne((req) => req.url === `${baseUrl}/tasks`);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.keys().length).toBe(0);
    request.flush({ items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 });
  });

  /** Verifies that getById sends a GET to the tasks endpoint. */
  it('should get a task by id', () => {
    service.getById(1).subscribe((result) => {
      expect(result).toEqual(task);
    });

    const request = httpMock.expectOne(`${baseUrl}/tasks/1`);
    expect(request.request.method).toBe('GET');
    request.flush(task);
  });

  /** Verifies that create sends a POST with the payload. */
  it('should create a task', () => {
    const payload = { title: 'New task', userId: 2, status: TaskState.Pending, additionalData: null };
    service.create(payload).subscribe((result) => {
      expect(result.title).toBe('New task');
    });

    const request = httpMock.expectOne(`${baseUrl}/tasks`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush({ ...task, title: 'New task', additionalData: null });
  });

  /** Verifies that update sends a PUT with the payload. */
  it('should update a task', () => {
    const payload = { title: 'Updated title', userId: 3, additionalData: '{"priority":"Low"}' };
    service.update(1, payload).subscribe();

    const request = httpMock.expectOne(`${baseUrl}/tasks/1`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(payload);
    request.flush({ ...task, title: payload.title, userId: payload.userId });
  });

  /** Verifies that delete sends a DELETE to the tasks endpoint. */
  it('should delete a task', () => {
    service.delete(1).subscribe();
    const request = httpMock.expectOne(`${baseUrl}/tasks/1`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });

  /** Verifies that changeStatus sends a PUT with the new status to the status endpoint. */
  it('should change the status of a task', () => {
    service.changeStatus(1, TaskState.Done).subscribe((result) => {
      expect(result.status).toBe(TaskState.Done);
    });

    const request = httpMock.expectOne(`${baseUrl}/tasks/1/status`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ status: TaskState.Done });
    request.flush({ ...task, status: TaskState.Done });
  });

  /** Verifies that a not found response surfaces the HTTP error to the caller. */
  it('should propagate a not found error when the task does not exist', () => {
    service.getById(999).subscribe({
      next: () => expect.unreachable('Expected the request to fail'),
      error: (error: { status: number }) => {
        expect(error.status).toBe(404);
      },
    });

    const request = httpMock.expectOne(`${baseUrl}/tasks/999`);
    request.flush({ title: 'Not Found', detail: 'The task was not found.' }, { status: 404, statusText: 'Not Found' });
  });
});
