import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

/** Supported UI languages. */
export type Language = 'es' | 'en';

/** Translation dictionary: flat keys mapped to their localized text. */
export type TranslationDictionary = Record<string, string>;

/** Storage key used to persist the selected language. */
const STORAGE_KEY = 'taskpage.language';

/** Loads and switches UI translations without recompiling. */
@Injectable({ providedIn: 'root' })
export class TranslateService {
  private readonly http = inject(HttpClient);

  /** Currently active language. */
  readonly language = signal<Language>(this.readStoredLanguage());

  /** Flat dictionary with the translations of the active language. */
  readonly translations = signal<TranslationDictionary>({});

  constructor() {
    this.applyDocumentLanguage(this.language());
    void this.load(this.language()).catch(() => {
      // The UI keeps rendering the keys instead of breaking when the dictionary fails.
      console.error(`Failed to load translations for "${this.language()}".`);
    });
  }

  /**
   * Loads the dictionary for the given language.
   * @param language Language whose translations are loaded.
   */
  async load(language: Language): Promise<void> {
    const dictionary = await firstValueFrom(
      this.http.get<TranslationDictionary>(`assets/i18n/${language}.json`),
    );
    this.translations.set(dictionary);
  }

  /**
   * Activates the requested language, persists it and reloads its dictionary.
   * @param language Language to activate.
   */
  async set(language: Language): Promise<void> {
    this.language.set(language);
    localStorage.setItem(STORAGE_KEY, language);
    this.applyDocumentLanguage(language);
    try {
      await this.load(language);
    } catch {
      // Keep the previous dictionary so a failed request does not wipe the UI.
      console.error(`Failed to load translations for "${language}".`);
    }
  }

  /** Switches between Spanish and English and reloads the dictionary. */
  async toggle(): Promise<void> {
    await this.set(this.language() === 'es' ? 'en' : 'es');
  }

  /**
   * Translates a key with optional `{{name}}` interpolation.
   * @param key Dictionary key to translate.
   * @param params Optional values used to replace placeholders.
   * @returns The localized text, or the key itself when it is missing.
   */
  translate(key: string, params?: Record<string, string>): string {
    let text = this.translations()[key] ?? key;
    if (params) {
      for (const [name, value] of Object.entries(params)) {
        text = text.replaceAll(`{{${name}}}`, value);
      }
    }
    return text;
  }

  /** Keeps the document language in sync for assistive technologies. */
  private applyDocumentLanguage(language: Language): void {
    document.documentElement.lang = language;
  }

  /** Reads the persisted language, defaulting to Spanish. */
  private readStoredLanguage(): Language {
    return localStorage.getItem(STORAGE_KEY) === 'en' ? 'en' : 'es';
  }
}
