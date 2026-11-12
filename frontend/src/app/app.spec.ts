// Unit tests for the root component.
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { TranslateService } from './core/i18n/translate.service';

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

  /** Creates the shell and flushes its initial Spanish dictionary. */
  async function createApp(dictionary: Record<string, string> = {}) {
    const fixture = TestBed.createComponent(App);
    httpMock.expectOne('assets/i18n/es.json').flush(dictionary);
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  }

  /** Verifies that the root component is created correctly. */
  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    httpMock.expectOne('assets/i18n/es.json').flush({});
    expect(fixture.componentInstance).toBeTruthy();
  });

  /** Verifies that the shell renders the application title and the sidebar brand. */
  it('should render the application title in the sidebar', async () => {
    const fixture = await createApp({ 'app.title': 'TaskPage' });
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('aside')?.classList.contains('app-sidebar')).toBe(true);
    expect(compiled.querySelector('.app-brand__name')?.textContent).toContain('TaskPage');
  });

  /** Verifies that the language flags switch the active language and persist it. */
  it('should switch language from Spanish to English and persist it', async () => {
    const fixture = await createApp({
      'app.title': 'TaskPage',
      'language.spanish': 'Español',
      'language.english': 'English',
    });

    const languageButtons = fixture.nativeElement.querySelectorAll(
      '.app-controls__lang',
    ) as NodeListOf<HTMLButtonElement>;
    expect(languageButtons.length).toBe(2);

    languageButtons[1].click();
    httpMock.expectOne('assets/i18n/en.json').flush({ 'app.title': 'TaskPage' });
    await fixture.whenStable();
    fixture.detectChanges();

    expect(localStorage.getItem('taskpage.language')).toBe('en');
    expect(document.documentElement.lang).toBe('en');
    expect(languageButtons[1].classList.contains('is-active')).toBe(true);
  });

  /** Verifies that selecting the already active language does not reload it. */
  it('should ignore the language that is already active', async () => {
    const fixture = await createApp({
      'language.spanish': 'Español',
      'language.english': 'English',
    });

    const spanishButton = fixture.nativeElement.querySelectorAll(
      '.app-controls__lang',
    )[0] as HTMLButtonElement;
    spanishButton.click();
    fixture.detectChanges();

    expect(localStorage.getItem('taskpage.language')).toBeNull();
    expect(document.documentElement.lang).toBe('es');
  });

  /** Verifies that the theme toggles and is persisted. */
  it('should toggle the theme and persist it', async () => {
    localStorage.setItem('taskpage.theme', 'light');
    const fixture = TestBed.createComponent(App);
    httpMock.expectOne('assets/i18n/es.json').flush({});
    fixture.detectChanges();

    const themeButton = fixture.nativeElement.querySelector(
      '.app-controls__theme',
    ) as HTMLButtonElement;
    themeButton.click();
    fixture.detectChanges();

    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(localStorage.getItem('taskpage.theme')).toBe('dark');
  });

  /** Verifies that navigation and controls expose accessible labels. */
  it('should expose accessible labels on navigation and controls', async () => {
    const fixture = await createApp({
      'app.title': 'TaskPage',
      'nav.title': 'Main navigation',
      'nav.tasks': 'Tasks',
      'nav.users': 'Users',
      'theme.toggle': 'Toggle theme',
      'controls.preferences': 'Interface preferences',
      'language.spanish': 'Español',
      'language.english': 'English',
    });

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('nav')?.getAttribute('aria-label')).toBe('Main navigation');
    expect(compiled.querySelector('[aria-label="Toggle theme"]')).toBeTruthy();
    expect(compiled.querySelector('[aria-label="Interface preferences"]')).toBeTruthy();
    expect(compiled.querySelector('[aria-label="Español"]')).toBeTruthy();
    expect(compiled.querySelector('[aria-label="English"]')).toBeTruthy();
    expect(compiled.textContent).toContain('Tasks');
    expect(compiled.textContent).toContain('Users');
  });

  /** Verifies that a failed dictionary request keeps the previous translations on screen. */
  it('should keep the previous dictionary when the language request fails', async () => {
    const fixture = await createApp({ 'app.title': 'TaskPage' });
    const translate = TestBed.inject(TranslateService);

    const englishButton = fixture.nativeElement.querySelectorAll(
      '.app-controls__lang',
    )[1] as HTMLButtonElement;
    englishButton.click();
    httpMock.expectOne('assets/i18n/en.json').error(new ProgressEvent('error'));
    await fixture.whenStable();
    fixture.detectChanges();

    expect(translate.language()).toBe('en');
    expect(translate.translations()['app.title']).toBe('TaskPage');
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('TaskPage');
  });
});
