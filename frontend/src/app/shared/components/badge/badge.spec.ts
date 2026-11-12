// Unit tests for the badge shared component.
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Badge, BadgeTone } from './badge';

describe('Badge', () => {
  let fixture: ComponentFixture<Badge>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Badge] }).compileComponents();
  });

  /** Creates the fixture with the given tone. */
  function createComponent(tone: BadgeTone): void {
    fixture = TestBed.createComponent(Badge);
    fixture.componentRef.setInput('tone', tone);
    fixture.detectChanges();
  }

  /** Verifies that the badge exposes a tone attribute for each status color. */
  it('should render the data-tone attribute that drives the contrast styles', () => {
    const tones: BadgeTone[] = ['neutral', 'info', 'success', 'warning', 'danger'];
    for (const tone of tones) {
      createComponent(tone);
      const badge = fixture.nativeElement.querySelector('.badge') as HTMLElement;
      expect(badge.getAttribute('data-tone')).toBe(tone);
    }
  });

  /** Verifies that the neutral tone is the default when no tone is provided. */
  it('should default to the neutral tone', () => {
    fixture = TestBed.createComponent(Badge);
    fixture.detectChanges();
    const badge = fixture.nativeElement.querySelector('.badge') as HTMLElement;
    expect(badge.getAttribute('data-tone')).toBe('neutral');
  });
});
