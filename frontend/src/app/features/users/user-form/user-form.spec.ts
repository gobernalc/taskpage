// Unit tests for the user creation and edition form.
import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { TranslateService } from '../../../core/i18n/translate.service';
import { User } from '../../../core/models/user.model';
import { UsersService } from '../../../core/services/users.service';
import { UserForm } from './user-form';

describe('UserForm', () => {
  let fixture: ComponentFixture<UserForm>;
  let usersService: { getById: ReturnType<typeof vi.fn>; create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn> };
  let routeId: string | null;

  const user: User = {
    id: 1,
    name: 'Ana Pérez',
    email: 'ana.perez@example.com',
    createdAt: '2026-11-01T10:00:00Z',
    updatedAt: null,
  };

  /** Creates a minimal TranslateService double returning the key itself. */
  function createTranslateServiceMock() {
    return {
      language: signal('es'),
      translations: signal<Record<string, string>>({}),
      translate: vi.fn((key: string) => key),
      load: vi.fn(async () => undefined),
      toggle: vi.fn(async () => undefined),
    } as unknown as TranslateService;
  }

  beforeEach(async () => {
    routeId = null;
    usersService = {
      getById: vi.fn().mockReturnValue(of(user)),
      create: vi.fn().mockReturnValue(of(user)),
      update: vi.fn().mockReturnValue(of(user)),
    };
    await TestBed.configureTestingModule({
      imports: [UserForm],
      providers: [
        provideRouter([]),
        { provide: UsersService, useValue: usersService },
        { provide: TranslateService, useValue: createTranslateServiceMock() },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => routeId } } } },
      ],
    }).compileComponents();
  });

  /** Creates the fixture and waits for the optional initial user load. */
  async function createComponent(): Promise<void> {
    fixture = TestBed.createComponent(UserForm);
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
  }

  /** Sets the value of an input and notifies the reactive form. */
  function setFieldValue(selector: string, value: string): void {
    const element = fixture.nativeElement.querySelector(selector) as HTMLInputElement;
    element.value = value;
    element.dispatchEvent(new Event('input'));
  }

  /** Submits the rendered form. */
  async function submitForm(): Promise<void> {
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    await new Promise((resolve) => setTimeout(resolve, 0));
    fixture.detectChanges();
  }

  /** Verifies that an invalid form blocks the create request. */
  it('should not submit when the form is invalid', async () => {
    await createComponent();
    await submitForm();
    expect(usersService.create).not.toHaveBeenCalled();
  });

  /** Verifies that an invalid email blocks the create request. */
  it('should block submission when the email is invalid', async () => {
    await createComponent();
    setFieldValue('#user-name', 'Ana Pérez');
    setFieldValue('#user-email', 'not-an-email');
    fixture.detectChanges();
    await submitForm();
    expect(usersService.create).not.toHaveBeenCalled();
  });

  /** Verifies that a valid form creates the user and navigates back. */
  it('should create a user with the trimmed payload and navigate back', async () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    await createComponent();

    setFieldValue('#user-name', '  Ana Pérez  ');
    setFieldValue('#user-email', '  ana.perez@example.com  ');
    fixture.detectChanges();
    await submitForm();

    expect(usersService.create).toHaveBeenCalledWith({ name: 'Ana Pérez', email: 'ana.perez@example.com' });
    expect(navigateSpy).toHaveBeenCalledWith(['/users']);
  });

  /** Verifies that an existing user is loaded and the form is populated. */
  it('should load the user and populate the form in edit mode', async () => {
    routeId = '1';
    await createComponent();
    expect(usersService.getById).toHaveBeenCalledWith(1);
    expect(fixture.nativeElement.querySelector('#user-name').value).toBe('Ana Pérez');
    expect(fixture.nativeElement.querySelector('#user-email').value).toBe('ana.perez@example.com');
  });

  /** Verifies that the update request is sent in edit mode. */
  it('should update the user in edit mode and navigate back', async () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    routeId = '1';
    await createComponent();

    setFieldValue('#user-name', 'Ana P. Gómez');
    fixture.detectChanges();
    await submitForm();

    expect(usersService.update).toHaveBeenCalledWith(1, { name: 'Ana P. Gómez', email: 'ana.perez@example.com' });
    expect(navigateSpy).toHaveBeenCalledWith(['/users']);
  });

  /** Verifies that an API failure surfaces a readable error message. */
  it('should surface the API error when creation fails', async () => {
    usersService.create.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 409, error: { detail: 'The email is already registered.' } })),
    );
    await createComponent();

    setFieldValue('#user-name', 'Ana Pérez');
    setFieldValue('#user-email', 'ana.perez@example.com');
    fixture.detectChanges();
    await submitForm();

    expect(fixture.nativeElement.textContent).toContain('The email is already registered.');
  });
});
