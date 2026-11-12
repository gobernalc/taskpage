// Unit tests for the translation service.
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TranslateService } from './translate.service';

describe('TranslateService', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  /** Creates the service and flushes its initial Spanish dictionary load. */
  function createService(): TranslateService {
    const service = TestBed.inject(TranslateService);
    httpMock.expectOne('assets/i18n/es.json').flush({});
    return service;
  }

  /** Verifies that the default language is Spanish and the initial load is Spanish. */
  it('should default to Spanish and load the Spanish dictionary', async () => {
    const service = TestBed.inject(TranslateService);
    httpMock.expectOne('assets/i18n/es.json').flush({ 'app.title': 'TaskPage' });
    await new Promise((resolve) => setTimeout(resolve, 0));
    expect(service.language()).toBe('es');
    expect(service.translations()['app.title']).toBe('TaskPage');
  });

  /** Verifies that a persisted English selection is restored on startup. */
  it('should restore English when it is stored as the selected language', () => {
    localStorage.setItem('taskpage.language', 'en');
    const service = TestBed.inject(TranslateService);
    httpMock.expectOne('assets/i18n/en.json').flush({});
    expect(service.language()).toBe('en');
  });

  /** Verifies that load replaces the dictionary for the requested language. */
  it('should load the English dictionary when requested', async () => {
    const service = createService();
    const promise = service.load('en');
    httpMock.expectOne('assets/i18n/en.json').flush({ 'app.title': 'TaskPage' });
    await promise;
    expect(service.translations()['app.title']).toBe('TaskPage');
  });

  /** Verifies that toggle switches the language, persists it and reloads the dictionary. */
  it('should toggle from Spanish to English and persist the selection', async () => {
    const service = createService();
    const promise = service.toggle();
    httpMock.expectOne('assets/i18n/en.json').flush({ 'nav.tasks': 'Tasks' });
    await promise;
    expect(service.language()).toBe('en');
    expect(localStorage.getItem('taskpage.language')).toBe('en');
    expect(service.translations()['nav.tasks']).toBe('Tasks');
  });

  /** Verifies that a missing key falls back to the key itself. */
  it('should return the key itself when the translation is missing', () => {
    const service = createService();
    expect(service.translate('missing.key')).toBe('missing.key');
  });

  /** Verifies that placeholders are interpolated with the provided values. */
  it('should interpolate placeholders in the translated text', () => {
    const service = createService();
    service.translations.set({ 'users.deleteConfirm': 'Delete the user "{{name}}"?' });
    expect(service.translate('users.deleteConfirm', { name: 'Ana' })).toBe('Delete the user "Ana"?');
  });

  /** Verifies that interpolation leaves unrelated placeholders untouched. */
  it('should keep placeholders that have no matching parameter', () => {
    const service = createService();
    service.translations.set({ 'common.maxLength': 'Must not exceed {{max}} characters' });
    expect(service.translate('common.maxLength', {})).toBe('Must not exceed {{max}} characters');
  });
});
