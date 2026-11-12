import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Surface that wraps a group of related content with an optional heading.
 */
@Component({
  selector: 'app-card',
  imports: [],
  template: `
    <section class="card">
      @if (title()) {
        <h2 class="card__title">{{ title() }}</h2>
      }
      <ng-content />
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './card.scss',
})
export class Card {
  /** Optional heading rendered above the projected content. */
  readonly title = input<string>();
}
