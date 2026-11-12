// Unit tests for the pagination shared component.
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateService } from '../../../core/i18n/translate.service';
import { Pagination } from './pagination';

describe('Pagination', () => {
  let fixture: ComponentFixture<Pagination>;

  /** Creates a minimal TranslateService double returning the key itself. */
  function createTranslateServiceMock() {
    return {
      language: signal('es'),
      translations: signal<Record<string, string>>({}),
      translate: vi.fn((key: string) => key),
      load: vi.fn(async () => undefined),
      toggle: vi.fn(async () => undefined),
    } as unknown as TranslateService;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Pagination],
      providers: [{ provide: TranslateService, useValue: createTranslateServiceMock() }],
    }).compileComponents();
  });

  /** Creates the fixture with the given paging state. */
  function createComponent(page: number, totalPages: number, totalItems: number): void {
    fixture = TestBed.createComponent(Pagination);
    fixture.componentRef.setInput('page', page);
    fixture.componentRef.setInput('totalPages', totalPages);
    fixture.componentRef.setInput('totalItems', totalItems);
    fixture.detectChanges();
  }

  /** Returns the rendered navigation buttons (previous and next). */
  function buttons(): HTMLButtonElement[] {
    return Array.from(fixture.nativeElement.querySelectorAll('button')) as HTMLButtonElement[];
  }

  /** Verifies that the previous button is disabled on the first page. */
  it('should disable the previous button on the first page', () => {
    createComponent(1, 3, 30);
    expect(buttons()[0].disabled).toBe(true);
    expect(buttons()[1].disabled).toBe(false);
  });

  /** Verifies that the next button is disabled on the last page. */
  it('should disable the next button on the last page', () => {
    createComponent(3, 3, 30);
    expect(buttons()[0].disabled).toBe(false);
    expect(buttons()[1].disabled).toBe(true);
  });

  /** Verifies that clicking next emits the following page. */
  it('should emit the next page when the next button is clicked', () => {
    createComponent(1, 3, 30);
    const pageChange = vi.fn();
    fixture.componentInstance.pageChange.subscribe(pageChange);

    buttons()[1].click();

    expect(pageChange).toHaveBeenCalledWith(2);
  });

  /** Verifies that clicking a disabled previous button does not emit. */
  it('should not emit when the previous button is disabled on the first page', () => {
    createComponent(1, 3, 30);
    const pageChange = vi.fn();
    fixture.componentInstance.pageChange.subscribe(pageChange);

    buttons()[0].click();

    expect(pageChange).not.toHaveBeenCalled();
  });

  /** Verifies that the rendered summary includes the page and total pages. */
  it('should render the paging summary and item count', () => {
    createComponent(2, 3, 30);
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('pagination.summary');
    expect(text).toContain('pagination.items');
  });
});
