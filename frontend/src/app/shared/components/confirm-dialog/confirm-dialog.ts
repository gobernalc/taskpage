import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  effect,
  inject,
  input,
  output,
  viewChild,
} from '@angular/core';

/** Visual tones supported by the confirmation dialog. */
export type ConfirmTone = 'danger' | 'primary';

/**
 * Modal confirmation dialog centered on screen.
 *
 * The parent owns the open state and the async action, so the component stays
 * presentational, reusable and free of navigation or service side effects.
 */
@Component({
  selector: 'app-confirm-dialog',
  imports: [],
  templateUrl: './confirm-dialog.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './confirm-dialog.scss',
  host: {
    '(document:keydown.escape)': 'onEscape()',
  },
})
export class ConfirmDialog {
  private readonly document = inject(DOCUMENT);
  private readonly destroyRef = inject(DestroyRef);

  /** Whether the modal is rendered. */
  readonly open = input(false);

  /** Heading announced by assistive technologies. */
  readonly title = input('');

  /** Body text that explains the action to confirm. */
  readonly message = input('');

  /** Label of the button that confirms the action. */
  readonly confirmLabel = input('');

  /** Label of the button that dismisses the modal. */
  readonly cancelLabel = input('');

  /** Accent applied to the icon and the confirm button. */
  readonly tone = input<ConfirmTone>('danger');

  /** Whether the action is running, which disables the dialog buttons. */
  readonly busy = input(false);

  /** Emitted when the user confirms the action. */
  readonly confirmed = output<void>();

  /** Emitted when the user cancels or dismisses the modal. */
  readonly cancelled = output<void>();

  private readonly panel = viewChild<ElementRef<HTMLElement>>('panel');
  private readonly cancelButton = viewChild<ElementRef<HTMLButtonElement>>('cancelButton');
  private previouslyFocused: HTMLElement | null = null;

  constructor() {
    effect(() => {
      if (this.open()) {
        this.previouslyFocused = this.document.activeElement as HTMLElement | null;
        this.document.body.style.overflow = 'hidden';
        // The cancel button is the safe default and only exists after render.
        queueMicrotask(() => this.cancelButton()?.nativeElement.focus());
      } else {
        this.document.body.style.overflow = '';
        this.previouslyFocused?.focus();
        this.previouslyFocused = null;
      }
    });

    this.destroyRef.onDestroy(() => {
      this.document.body.style.overflow = '';
    });
  }

  /** Confirms the action unless it is already running. */
  protected onConfirm(): void {
    if (!this.busy()) {
      this.confirmed.emit();
    }
  }

  /** Dismisses the modal unless the action is running. */
  protected onCancel(): void {
    if (!this.busy()) {
      this.cancelled.emit();
    }
  }

  /** Handles Escape as a cancel request while the modal is open. */
  protected onEscape(): void {
    if (this.open()) {
      this.onCancel();
    }
  }

  /** Closes the modal when the dimmed backdrop is clicked. */
  protected onBackdrop(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.onCancel();
    }
  }

  /** Keeps keyboard focus inside the modal while it is open. */
  protected onTab(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;
    const buttons = this.panel()?.nativeElement.querySelectorAll<HTMLElement>(
      'button:not([disabled])',
    );
    if (!buttons || buttons.length === 0) {
      return;
    }
    const first = buttons[0];
    const last = buttons[buttons.length - 1];
    const active = this.document.activeElement;
    if (keyboardEvent.shiftKey && active === first) {
      keyboardEvent.preventDefault();
      last.focus();
    } else if (!keyboardEvent.shiftKey && active === last) {
      keyboardEvent.preventDefault();
      first.focus();
    }
  }
}
