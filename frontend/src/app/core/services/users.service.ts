import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult } from '../models/paged-result.model';
import { User, UserFilter, UserPayload } from '../models/user.model';
import { API_BASE_URL } from './api-base-url.token';

/** Service for managing users through the REST API. */
@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  /**
   * Lists users with search, sorting and pagination.
   * @param filter Query filters for the listing.
   */
  list(filter: UserFilter = {}): Observable<PagedResult<User>> {
    return this.http.get<PagedResult<User>>(`${this.baseUrl}/users`, {
      params: this.toParams(filter)
    });
  }

  /**
   * Gets a user by its identifier.
   * @param id User identifier.
   */
  getById(id: number): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/users/${id}`);
  }

  /**
   * Creates a new user.
   * @param payload Input data to create the user.
   */
  create(payload: UserPayload): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}/users`, payload);
  }

  /**
   * Replaces the values of an existing user.
   * @param id User identifier.
   * @param payload Input data with the new values.
   */
  update(id: number, payload: UserPayload): Observable<User> {
    return this.http.put<User>(`${this.baseUrl}/users/${id}`, payload);
  }

  /**
   * Deletes a user with no associated tasks.
   * @param id User identifier.
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/${id}`);
  }

  /** Converts a filter into HTTP query params, omitting undefined values. */
  private toParams(filter: UserFilter): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value !== undefined && value !== null) {
        params = params.set(key, String(value));
      }
    }
    return params;
  }
}
