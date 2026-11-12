// Unit tests for the users HTTP service.
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { User } from '../models/user.model';
import { UsersService } from './users.service';

describe('UsersService', () => {
  let service: UsersService;
  let httpMock: HttpTestingController;

  const baseUrl = 'http://localhost:5017/api';

  const user: User = {
    id: 1,
    name: 'Ana Pérez',
    email: 'ana.perez@example.com',
    createdAt: '2026-11-01T10:00:00Z',
    updatedAt: null,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(UsersService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  /** Verifies that the list request sends a GET with the expected query params. */
  it('should list users with search, sorting and pagination params', () => {
    service.list({ search: 'ana', sort: 'name', order: 'asc', page: 2, pageSize: 5 }).subscribe((result) => {
      expect(result.items).toEqual([user]);
      expect(result.totalItems).toBe(1);
    });

    const request = httpMock.expectOne((req) => req.url === `${baseUrl}/users`);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('search')).toBe('ana');
    expect(request.request.params.get('sort')).toBe('name');
    expect(request.request.params.get('order')).toBe('asc');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('5');
    request.flush({ items: [user], page: 2, pageSize: 5, totalItems: 1, totalPages: 1 });
  });

  /** Verifies that omitted filters are not sent as query params. */
  it('should list users without query params when no filter is provided', () => {
    service.list().subscribe();
    const request = httpMock.expectOne((req) => req.url === `${baseUrl}/users`);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.keys().length).toBe(0);
    request.flush({ items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 });
  });

  /** Verifies that getById sends a GET to the users endpoint. */
  it('should get a user by id', () => {
    service.getById(1).subscribe((result) => {
      expect(result).toEqual(user);
    });

    const request = httpMock.expectOne(`${baseUrl}/users/1`);
    expect(request.request.method).toBe('GET');
    request.flush(user);
  });

  /** Verifies that create sends a POST with the payload. */
  it('should create a user', () => {
    const payload = { name: 'Luis Gómez', email: 'luis.gomez@example.com' };
    service.create(payload).subscribe((result) => {
      expect(result.id).toBe(2);
    });

    const request = httpMock.expectOne(`${baseUrl}/users`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush({ ...user, id: 2, name: payload.name, email: payload.email });
  });

  /** Verifies that update sends a PUT with the payload. */
  it('should update a user', () => {
    const payload = { name: 'Ana Pérez', email: 'ana@example.com' };
    service.update(1, payload).subscribe();

    const request = httpMock.expectOne(`${baseUrl}/users/1`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(payload);
    request.flush({ ...user, email: payload.email });
  });

  /** Verifies that delete sends a DELETE to the users endpoint. */
  it('should delete a user', () => {
    service.delete(1).subscribe();
    const request = httpMock.expectOne(`${baseUrl}/users/1`);
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });

  /** Verifies that a conflict response surfaces the HTTP error to the caller. */
  it('should propagate a conflict error when deleting a user with tasks', () => {
    service.delete(1).subscribe({
      next: () => expect.unreachable('Expected the request to fail'),
      error: (error: { status: number }) => {
        expect(error.status).toBe(409);
      },
    });

    const request = httpMock.expectOne(`${baseUrl}/users/1`);
    request.flush(
      { title: 'Conflict', detail: 'The user has associated tasks.' },
      { status: 409, statusText: 'Conflict' },
    );
  });
});
