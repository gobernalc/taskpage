/** Generic paged result returned by the list endpoints. */
export interface PagedResult<T> {
  /** Items of the current page. */
  items: T[];

  /** One-based index of the current page. */
  page: number;

  /** Requested page size. */
  pageSize: number;

  /** Total number of items across all pages. */
  totalItems: number;

  /** Total number of available pages. */
  totalPages: number;
}
