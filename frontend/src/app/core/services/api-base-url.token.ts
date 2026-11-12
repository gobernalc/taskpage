import { InjectionToken } from '@angular/core';

/** Injection token holding the backend API base URL. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  factory: () => 'http://localhost:5017/api'
});
