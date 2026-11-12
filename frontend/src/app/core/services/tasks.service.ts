import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult } from '../models/paged-result.model';
import { TaskState } from '../models/task-state.enum';
import { CreateTaskPayload, Task, TaskFilter, UpdateTaskPayload } from '../models/task.model';
import { API_BASE_URL } from './api-base-url.token';

/** Service for managing tasks through the REST API. */
@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  /**
   * Lists tasks with filters, search, sorting and pagination.
   * @param filter Query filters for the listing.
   */
  list(filter: TaskFilter = {}): Observable<PagedResult<Task>> {
    return this.http.get<PagedResult<Task>>(`${this.baseUrl}/tasks`, {
      params: this.toParams(filter)
    });
  }

  /**
   * Gets a task by its identifier.
   * @param id Task identifier.
   */
  getById(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.baseUrl}/tasks/${id}`);
  }

  /**
   * Creates a task assigned to an existing user.
   * @param payload Input data to create the task.
   */
  create(payload: CreateTaskPayload): Observable<Task> {
    return this.http.post<Task>(`${this.baseUrl}/tasks`, payload);
  }

  /**
   * Replaces the editable values of an existing task.
   * @param id Task identifier.
   * @param payload Input data with the new values.
   */
  update(id: number, payload: UpdateTaskPayload): Observable<Task> {
    return this.http.put<Task>(`${this.baseUrl}/tasks/${id}`, payload);
  }

  /**
   * Deletes an existing task.
   * @param id Task identifier.
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/tasks/${id}`);
  }

  /**
   * Changes the status of a task respecting the transition rules.
   * @param id Task identifier.
   * @param status New status for the task.
   */
  changeStatus(id: number, status: TaskState): Observable<Task> {
    return this.http.put<Task>(`${this.baseUrl}/tasks/${id}/status`, { status });
  }

  /** Converts a filter into HTTP query params, omitting undefined values. */
  private toParams(filter: TaskFilter): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value !== undefined && value !== null) {
        params = params.set(key, String(value));
      }
    }
    return params;
  }
}
