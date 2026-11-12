import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Native button types forwarded to the rendered element. */
export type ButtonType = 'button' | 'submit';

/** Visual variants of the button. */
export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost';

/**
 * Reusable button that forwards the native type and disabled state and applies
 * the design-token variants.
 */
@Component({
  selector: 'app-button',
  imports: [],
  template: `
    <button
      class="btn"
      [type]="type()"
      [disabled]="disabled()"
      [attr.data-variant]="variant()"
    >
      <ng-content />
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class Button {
  /** Native button type (submit for forms). */
  readonly type = input<ButtonType>('button');

  /** Visual variant applied to the button. */
  readonly variant = input<ButtonVariant>('primary');

  /** Whether the button is disabled. */
  readonly disabled = input(false);
}
