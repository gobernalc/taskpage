// Unit tests for the root component.
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
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
});
