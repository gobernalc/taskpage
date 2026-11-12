import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from './core/i18n/translate.pipe';
import { TranslateService } from './core/i18n/translate.service';

/** UI theme options. */
type Theme = 'light' | 'dark';

/** Storage key used to persist the selected theme. */
const THEME_STORAGE_KEY = 'taskpage.theme';

/** Root component: layout shell with theme and language switchers. */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './app.scss',
})
export class App {
  private readonly translate = inject(TranslateService);

  /** Currently active theme. */
  protected readonly theme = signal<Theme>(this.readStoredTheme());

  constructor() {
    this.applyTheme(this.theme());
  }

  /** Toggles between light and dark themes and persists the choice. */
  protected toggleTheme(): void {
    const next: Theme = this.theme() === 'light' ? 'dark' : 'light';
    this.theme.set(next);
    localStorage.setItem(THEME_STORAGE_KEY, next);
    this.applyTheme(next);
  }

  /** Toggles the UI language through the translation service. */
  protected toggleLanguage(): void {
    void this.translate.toggle();
  }

  /** Label shown on the language switch button. */
  protected languageLabel(): string {
    return this.translate.language() === 'es' ? 'EN' : 'ES';
  }

  /** Applies the theme as a data attribute on the root element. */
  private applyTheme(theme: Theme): void {
    document.documentElement.setAttribute('data-theme', theme);
  }

  /** Reads the persisted theme, falling back to the system preference. */
  private readStoredTheme(): Theme {
    const stored = localStorage.getItem(THEME_STORAGE_KEY);
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }
    const prefersDark = typeof window.matchMedia === 'function'
      ? window.matchMedia('(prefers-color-scheme: dark)').matches
      : false;
    return prefersDark ? 'dark' : 'light';
  }
}
