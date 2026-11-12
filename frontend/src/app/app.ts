import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslatePipe } from './core/i18n/translate.pipe';
import { Language, TranslateService } from './core/i18n/translate.service';

/** UI theme options. */
type Theme = 'light' | 'dark';

/** Storage key used to persist the selected theme. */
const THEME_STORAGE_KEY = 'taskpage.theme';

/** Storage key used to persist the sidebar collapsed state. */
const SIDEBAR_STORAGE_KEY = 'taskpage.sidebarCollapsed';

/** Root component: collapsible sidebar shell with theme and language controls. */
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

  /** Currently active language, exposed for the flag buttons. */
  protected readonly language = this.translate.language;

  /** Whether the dark theme is active, used to pick the control icon. */
  protected readonly isDark = computed(() => this.theme() === 'dark');

  /** Whether the sidebar is collapsed to an icon rail. */
  protected readonly sidebarCollapsed = signal(this.readStoredSidebar());

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

  /**
   * Activates one of the supported UI languages.
   * @param language Language selected through its flag button.
   */
  protected setLanguage(language: Language): void {
    if (language === this.language()) {
      return;
    }
    void this.translate.set(language);
  }

  /** Toggles the sidebar between its expanded and collapsed states. */
  protected toggleSidebar(): void {
    const next = !this.sidebarCollapsed();
    this.sidebarCollapsed.set(next);
    localStorage.setItem(SIDEBAR_STORAGE_KEY, String(next));
  }

  /** Applies the theme as a data attribute on the root element. */
  private applyTheme(theme: Theme): void {
    document.documentElement.setAttribute('data-theme', theme);
  }

  /** Reads the persisted sidebar state, defaulting to expanded. */
  private readStoredSidebar(): boolean {
    return localStorage.getItem(SIDEBAR_STORAGE_KEY) === 'true';
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
