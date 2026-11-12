import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

/** Placeholder shell for the user list feature (full implementation in Phase 7). */
@Component({
  selector: 'app-user-list',
  imports: [TranslatePipe],
  template: `
    <section>
      <h1>{{ 'users.title' | translate }}</h1>
      <p>{{ 'users.empty' | translate }}</p>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class UserList {}
