/** Read-only representation of a user returned by the API. */
export interface User {
  /** User identifier. */
  id: number;

  /** Full name of the collaborator. */
  name: string;

  /** Normalized email address. */
  email: string;

  /** Creation timestamp in UTC (ISO 8601). */
  createdAt: string;

  /** Last modification timestamp in UTC, when applicable. */
  updatedAt: string | null;
}

/** Input data to create or update a user. */
export interface UserPayload {
  /** Full name of the collaborator. */
  name: string;

  /** Email address, normalized by the service when saved. */
  email: string;
}

/** Query filters for the user listing. */
export interface UserFilter {
  /** Free text matched against name and email. */
  search?: string;

  /** Field used for sorting. */
  sort?: string;

  /** Sort direction. */
  order?: 'asc' | 'desc';

  /** One-based page index. */
  page?: number;

  /** Number of items per page. */
  pageSize?: number;
}
