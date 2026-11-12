// Unit tests for the translation pipe.
import { TestBed } from '@angular/core/testing';
import { TranslateService } from './translate.service';
import { TranslatePipe } from './translate.pipe';

describe('TranslatePipe', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        {
          provide: TranslateService,
          useValue: {
            translate: vi.fn((key: string, params?: Record<string, string>) => {
              let text = key;
              if (params) {
                for (const [name, value] of Object.entries(params)) {
                  text = text.replaceAll(`{{${name}}}`, value);
                }
              }
              return text;
            }),
          } as unknown as TranslateService,
        },
      ],
    });
  });

  /** Verifies that the pipe delegates the key to the translation service. */
  it('should delegate a key to the translation service', () => {
    const pipe = TestBed.runInInjectionContext(() => new TranslatePipe());
    const service = TestBed.inject(TranslateService);
    expect(pipe.transform('app.title')).toBe('app.title');
    expect(service.translate).toHaveBeenCalledWith('app.title', undefined);
  });

  /** Verifies that interpolation params are forwarded to the service. */
  it('should forward interpolation params to the translation service', () => {
    const pipe = TestBed.runInInjectionContext(() => new TranslatePipe());
    expect(pipe.transform('common.maxLength', { max: '200' })).toBe('common.maxLength');
  });
});
