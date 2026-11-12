import { HttpErrorResponse } from '@angular/common/http';

/** Shape of the ProblemDetails payload returned by the API on errors. */
interface ProblemDetails {
  title?: string;
  detail?: string;
}

/**
 * Extracts a readable message from an HTTP error response.
 * @param error Unknown error captured from a failed request.
 * @returns The ProblemDetails detail or title, or null when not available.
 */
export function readErrorMessage(error: unknown): string | null {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as ProblemDetails | null;
    if (body && typeof body === 'object') {
      return body.detail ?? body.title ?? null;
    }
  }
  return null;
}
