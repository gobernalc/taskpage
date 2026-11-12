import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslateService } from './translate.service';

/** Translates a dictionary key with optional interpolation. */
@Pipe({
  name: 'translate',
  standalone: true,
  pure: false,
})
export class TranslatePipe implements PipeTransform {
  private readonly service = inject(TranslateService);

  /**
   * Returns the localized text for the key.
   * @param key Dictionary key to translate.
   * @param params Optional values used to replace placeholders.
   * @returns The localized text, or the key itself when it is missing.
   */
  transform(key: string, params?: Record<string, string>): string {
    return this.service.translate(key, params);
  }
}
