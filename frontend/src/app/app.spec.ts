// Unit tests for the root component.
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  /** Verifies that the root component is created correctly. */
  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    httpMock.expectOne('assets/i18n/es.json').flush({});
    expect(fixture.componentInstance).toBeTruthy();
  });

  /** Verifies that the shell renders the application title. */
  it('should render the application title', async () => {
    const fixture = TestBed.createComponent(App);
    httpMock.expectOne('assets/i18n/es.json').flush({ 'app.title': 'TaskPage' });
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.app-brand__name')?.textContent).toContain('TaskPage');
  });

  /** Verifies that the theme toggles and is persisted. */
  it('should toggle the theme and persist it', async () => {
    localStorage.setItem('taskpage.theme', 'light');
    const fixture = TestBed.createComponent(App);
    httpMock.expectOne('assets/i18n/es.json').flush({});
    fixture.detectChanges();

    const themeButton = fixture.nativeElement.querySelector('.app-actions button') as HTMLButtonElement;
    themeButton.click();
    fixture.detectChanges();

    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(localStorage.getItem('taskpage.theme')).toBe('dark');
  });

  /** Verifies that navigation and controls expose accessible labels. */
  it('should expose accessible labels on navigation and controls', async () => {
    const fixture = TestBed.createComponent(App);
    httpMock.expectOne('assets/i18n/es.json').flush({
      'app.title': 'TaskPage',
      'nav.title': 'Main navigation',
      'nav.tasks': 'Tasks',
      'nav.users': 'Users',
      'theme.toggle': 'Toggle theme',
      'language.toggle': 'Switch language',
    });
    await fixture.whenStable();
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('nav')?.getAttribute('aria-label')).toBe('Main navigation');
    expect(compiled.querySelector('[aria-label="Toggle theme"]')).toBeTruthy();
    expect(compiled.querySelector('[aria-label="Switch language"]')).toBeTruthy();
    expect(compiled.textContent).toContain('Tasks');
    expect(compiled.textContent).toContain('Users');
  });
});
