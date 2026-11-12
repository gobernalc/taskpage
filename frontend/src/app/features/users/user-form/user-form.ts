import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { readErrorMessage } from '../../../core/http-error';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { TranslateService } from '../../../core/i18n/translate.service';
import { UserPayload } from '../../../core/models/user.model';
import { UsersService } from '../../../core/services/users.service';
import { Button } from '../../../shared/components/button/button';
import { Card } from '../../../shared/components/card/card';

/**
 * Reactive form to create or edit a user, depending on the active route.
 */
@Component({
  selector: 'app-user-form',
  imports: [ReactiveFormsModule, RouterLink, TranslatePipe, Button, Card],
  templateUrl: './user-form.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './user-form.scss',
})
export class UserForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly usersService = inject(UsersService);
  private readonly translate = inject(TranslateService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  /** Whether the form edits an existing user (false means create). */
  protected readonly isEdit = signal(false);
  /** Whether the user is being loaded. */
  protected readonly loading = signal(false);
  /** Whether the request is in flight. */
  protected readonly submitting = signal(false);
  /** Readable error shown when a request fails. */
  protected readonly error = signal('');

  /** Identifier of the user being edited. */
  private userId = 0;

  /** Reactive form with the user fields and client-side validation. */
  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(200)]],
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id === null) {
      return;
    }
    this.isEdit.set(true);
    void this.loadUser(Number(id));
  }

  /** Loads the user and populates the form with its current values. */
  private async loadUser(id: number): Promise<void> {
    if (!Number.isInteger(id) || id <= 0) {
      await this.router.navigate(['/users']);
      return;
    }
    this.userId = id;
    this.loading.set(true);
    try {
      const user = await firstValueFrom(this.usersService.getById(id));
      this.form.setValue({ name: user.name, email: user.email });
    } catch (err) {
      this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
    } finally {
      this.loading.set(false);
    }
  }

  /** Creates or updates the user and navigates back to the list on success. */
  protected async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.error.set('');
    const payload: UserPayload = {
      name: this.form.controls.name.value.trim(),
      email: this.form.controls.email.value.trim(),
    };
    try {
      if (this.isEdit()) {
        await firstValueFrom(this.usersService.update(this.userId, payload));
      } else {
        await firstValueFrom(this.usersService.create(payload));
      }
      await this.router.navigate(['/users']);
    } catch (err) {
      this.error.set(readErrorMessage(err) ?? this.translate.translate('common.apiError'));
    } finally {
      this.submitting.set(false);
    }
  }
}
