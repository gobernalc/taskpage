import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

/**
 * Pagination controls for paged listings.
 */
@Component({
  selector: 'app-pagination',
  imports: [TranslatePipe],
  template: `
    <nav class="pagination" [attr.aria-label]="'pagination.label' | translate">
      <button
        type="button"
        class="pagination__button"
        [disabled]="page() <= 1"
        (click)="goTo(page() - 1)"
      >
        {{ 'pagination.previous' | translate }}
      </button>
      <span class="pagination__summary">
        {{ 'pagination.summary' | translate: summaryParams() }}
      </span>
      <span class="pagination__items">
        {{ 'pagination.items' | translate: itemsParams() }}
      </span>
      <button
        type="button"
        class="pagination__button"
        [disabled]="page() >= totalPages()"
        (click)="goTo(page() + 1)"
      >
        {{ 'pagination.next' | translate }}
      </button>
    </nav>
  `,
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './pagination.scss',
})
export class Pagination {
  /** Current one-based page index. */
  readonly page = input.required<number>();

  /** Total number of available pages. */
  readonly totalPages = input.required<number>();

  /** Total number of items across all pages. */
  readonly totalItems = input.required<number>();

  /** Emits the requested page when the user navigates. */
  readonly pageChange = output<number>();

  /** Requests a new page when it is within the valid range. */
  protected goTo(page: number): void {
    if (page < 1 || page > this.totalPages()) {
      return;
    }
    this.pageChange.emit(page);
  }

  /** Builds the interpolation params for the summary text. */
  protected summaryParams(): Record<string, string> {
    return { page: String(this.page()), totalPages: String(this.totalPages()) };
  }

  /** Builds the interpolation params for the items text. */
  protected itemsParams(): Record<string, string> {
    return { totalItems: String(this.totalItems()) };
  }
}
