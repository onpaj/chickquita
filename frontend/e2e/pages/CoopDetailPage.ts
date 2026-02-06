import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for the Coop Detail page
 */
export class CoopDetailPage {
  readonly page: Page;
  readonly coopName: Locator;
  readonly coopLocation: Locator;
  readonly flocksButton: Locator;
  readonly editButton: Locator;
  readonly archiveButton: Locator;
  readonly deleteButton: Locator;
  readonly backButton: Locator;
  readonly flocksSection: Locator;

  constructor(page: Page) {
    this.page = page;
    this.coopName = page.locator('[data-testid="coop-name"]').or(page.getByRole('heading', { level: 1 }));
    this.coopLocation = page.locator('[data-testid="coop-location"]');
    this.flocksButton = page.getByRole('button', { name: /^hejna$|^flocks$/i });
    this.editButton = page.getByRole('button', { name: /edit|upravit/i }).and(page.locator('[aria-label*="edit"], [aria-label*="upravit"]'));
    this.archiveButton = page.getByRole('button', { name: /archive|archivovat/i });
    this.deleteButton = page.getByRole('button', { name: /delete|smazat/i });
    this.backButton = page.getByRole('button', { name: /back|zpět/i });
    this.flocksSection = page.locator('[data-testid="flocks-section"]');
  }

  async goto(coopId: string) {
    await this.page.goto(`/coops/${coopId}`);
  }

  async clickEdit() {
    await this.editButton.click();
  }

  async clickArchive() {
    await this.archiveButton.click();
  }

  async clickDelete() {
    await this.deleteButton.click();
  }

  async clickBack() {
    await this.backButton.click();
  }

  async getCoopName(): Promise<string> {
    return await this.coopName.textContent() || '';
  }

  async getCoopLocation(): Promise<string> {
    return await this.coopLocation.textContent() || '';
  }

  async navigateToFlocks() {
    await this.flocksButton.click();
    await this.page.waitForLoadState('networkidle');
  }
}
