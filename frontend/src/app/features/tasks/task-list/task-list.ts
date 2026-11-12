import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

/** Placeholder shell for the task list feature (full implementation in Phase 7). */
@Component({
  selector: 'app-task-list',
  imports: [TranslatePipe],
  template: `
    <section>
      <h1>{{ 'tasks.title' | translate }}</h1>
      <p>{{ 'tasks.empty' | translate }}</p>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class TaskList {}
