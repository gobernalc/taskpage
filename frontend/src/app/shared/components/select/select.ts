import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

/**
 * Labeled select bound to a reactive form control, with optional error text.
 */
@Component({
  selector: 'app-select',
  imports: [ReactiveFormsModule],
  template: `
    <div class="field">
      <label class="field__label" [for]="fieldId()">{{ label() }}</label>
      <select
        class="field__control"
        [class.is-invalid]="!!error()"
        [id]="fieldId()"
        [formControl]="control()"
      >
        <ng-content />
      </select>
      @if (error()) {
        <p class="field__error">{{ error() }}</p>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class Select {
  /** Label text shown above the select. */
  readonly label = input.required<string>();

  /** Identifier linking the label to the select element. */
  readonly fieldId = input.required<string>();

  /** Reactive form control bound to the select. */
  readonly control = input.required<FormControl>();

  /** Optional validation message rendered under the control. */
  readonly error = input<string>();
}
