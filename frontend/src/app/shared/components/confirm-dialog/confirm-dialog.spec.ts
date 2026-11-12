// Unit tests for the confirmation modal shared component.
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfirmDialog } from './confirm-dialog';

describe('ConfirmDialog', () => {
  let fixture: ComponentFixture<ConfirmDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ConfirmDialog] }).compileComponents();
  });

  /** Creates the dialog with the requested state and default labels. */
  function createComponent(open: boolean, busy = false): void {
    fixture = TestBed.createComponent(ConfirmDialog);
    fixture.componentRef.setInput('open', open);
    fixture.componentRef.setInput('title', 'Delete task');
    fixture.componentRef.setInput('message', 'Delete the task "Design"?');
    fixture.componentRef.setInput('confirmLabel', 'Delete');
    fixture.componentRef.setInput('cancelLabel', 'Cancel');
    fixture.componentRef.setInput('busy', busy);
    fixture.detectChanges();
  }

  /** Returns the overlay element rendered by the modal. */
  function overlay(): HTMLElement | null {
    return fixture.nativeElement.querySelector('.confirm-overlay') as HTMLElement | null;
  }

  /** Verifies that a closed dialog renders nothing on the page. */
  it('should not render the modal when it is closed', () => {
    createComponent(false);
    expect(overlay()).toBeNull();
  });

  /** Verifies that an open dialog exposes its accessible content. */
  it('should render an accessible alert dialog with the provided content', () => {
    createComponent(true);
    const dialog = fixture.nativeElement.querySelector('[role="alertdialog"]') as HTMLElement;

    expect(dialog).toBeTruthy();
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(dialog.getAttribute('aria-labelledby')).toBe('confirm-dialog-title');
    expect(dialog.getAttribute('aria-describedby')).toBe('confirm-dialog-message');
    expect(dialog.textContent).toContain('Delete task');
    expect(dialog.textContent).toContain('Delete the task "Design"?');
    expect(dialog.querySelector('[data-action="confirm"]')?.textContent).toContain('Delete');
    expect(dialog.querySelector('[data-action="cancel"]')?.textContent).toContain('Cancel');
  });

  /** Verifies that the confirm button reports the user decision. */
  it('should emit confirmed when the user confirms the action', () => {
    createComponent(true);
    const confirmed = vi.fn();
    fixture.componentInstance.confirmed.subscribe(confirmed);

    (fixture.nativeElement.querySelector('[data-action="confirm"]') as HTMLButtonElement).click();

    expect(confirmed).toHaveBeenCalledTimes(1);
  });

  /** Verifies that the cancel button reports the user decision. */
  it('should emit cancelled when the user cancels the action', () => {
    createComponent(true);
    const cancelled = vi.fn();
    fixture.componentInstance.cancelled.subscribe(cancelled);

    (fixture.nativeElement.querySelector('[data-action="cancel"]') as HTMLButtonElement).click();

    expect(cancelled).toHaveBeenCalledTimes(1);
  });

  /** Verifies that Escape dismisses the modal as a cancel request. */
  it('should emit cancelled when the user presses Escape', () => {
    createComponent(true);
    const cancelled = vi.fn();
    fixture.componentInstance.cancelled.subscribe(cancelled);

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));

    expect(cancelled).toHaveBeenCalledTimes(1);
  });

  /** Verifies that the backdrop dismisses the modal but its panel does not. */
  it('should emit cancelled only when the backdrop itself is clicked', () => {
    createComponent(true);
    const cancelled = vi.fn();
    fixture.componentInstance.cancelled.subscribe(cancelled);

    (fixture.nativeElement.querySelector('.confirm-dialog') as HTMLElement).click();
    expect(cancelled).not.toHaveBeenCalled();

    (overlay() as HTMLElement).click();
    expect(cancelled).toHaveBeenCalledTimes(1);
  });

  /** Verifies that a running action blocks the buttons and the outputs. */
  it('should disable the actions and ignore clicks while busy', () => {
    createComponent(true, true);
    const confirmed = vi.fn();
    const cancelled = vi.fn();
    fixture.componentInstance.confirmed.subscribe(confirmed);
    fixture.componentInstance.cancelled.subscribe(cancelled);

    const confirmButton = fixture.nativeElement.querySelector(
      '[data-action="confirm"]',
    ) as HTMLButtonElement;
    const cancelButton = fixture.nativeElement.querySelector(
      '[data-action="cancel"]',
    ) as HTMLButtonElement;

    expect(confirmButton.disabled).toBe(true);
    expect(cancelButton.disabled).toBe(true);

    confirmButton.click();
    cancelButton.click();
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));

    expect(confirmed).not.toHaveBeenCalled();
    expect(cancelled).not.toHaveBeenCalled();
  });

  /** Verifies that opening the modal moves focus to the safe cancel action. */
  it('should focus the cancel button when it opens', async () => {
    createComponent(true);
    await Promise.resolve();

    expect(document.activeElement).toBe(
      fixture.nativeElement.querySelector('[data-action="cancel"]'),
    );
  });
});
