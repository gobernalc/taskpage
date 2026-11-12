import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Visual tones supported by the badge. */
export type BadgeTone = 'neutral' | 'info' | 'success' | 'warning' | 'danger';

/**
 * Colored pill used to highlight short values such as task states.
 */
@Component({
  selector: 'app-badge',
  imports: [],
  template: `<span class="badge" [attr.data-tone]="tone()"><ng-content /></span>`,
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './badge.scss',
})
export class Badge {
  /** Tone that defines the badge color. */
  readonly tone = input<BadgeTone>('neutral');
}
