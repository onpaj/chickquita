import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for the Dashboard page
 */
export class DashboardPage {
  readonly page: Page;
  readonly pageTitle: Locator;
  readonly coopsNavigationButton: Locator;
  readonly userButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.pageTitle = page.getByRole('heading', { name: /dashboard|přehled/i });
    // BottomNavigationAction renders as a button with the label as accessible name
    this.coopsNavigationButton = page.getByRole('button', { name: /^(coops|kurníky)$/i });
    this.userButton = page.locator('.cl-userButton');
  }

  async goto() {
    await this.page.goto('/');
  }

  async navigateToCoops() {
    await this.coopsNavigationButton.click();
  }

  async isUserLoggedIn(): Promise<boolean> {
    return await this.userButton.isVisible();
  }
}
