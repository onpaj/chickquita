import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for the Coops List page
 */
export class CoopsPage {
  readonly page: Page;
  readonly pageTitle: Locator;
  readonly createCoopButton: Locator;
  readonly emptyStateMessage: Locator;
  readonly coopCardsList: Locator;

  constructor(page: Page) {
    this.page = page;
    this.pageTitle = page.getByRole('heading', { name: /coops|kurníky/i });
    this.createCoopButton = page.getByRole('button', { name: /add coop|přidat kurník|create coop/i });
    this.emptyStateMessage = page.getByText(/no coops yet|zatím nemáte žádné kurníky/i);
    this.coopCardsList = page.locator('[data-testid="coop-card"]');
  }

  async goto() {
    await this.page.goto('/coops');
  }

  async openCreateCoopModal() {
    await this.createCoopButton.click();
  }

  async getCoopCard(coopName: string): Promise<Locator> {
    return this.page.locator('[data-testid="coop-card"]').filter({ hasText: coopName });
  }

  async getCoopCount(): Promise<number> {
    return await this.coopCardsList.count();
  }

  async clickEditCoop(coopName: string) {
    const card = await this.getCoopCard(coopName);
    await card.getByRole('button', { name: /edit|upravit/i }).click();
  }

  async clickArchiveCoop(coopName: string) {
    const card = await this.getCoopCard(coopName);
    await card.getByRole('button', { name: /archive|archivovat/i }).click();
  }

  async clickDeleteCoop(coopName: string) {
    const card = await this.getCoopCard(coopName);
    await card.getByRole('button', { name: /delete|smazat/i }).click();
  }

  async clickCoopCard(coopName: string) {
    const card = await this.getCoopCard(coopName);
    await card.click();
  }

  async isEmptyStateVisible(): Promise<boolean> {
    return await this.emptyStateMessage.isVisible();
  }

  async isCoopVisible(coopName: string): Promise<boolean> {
    const card = await this.getCoopCard(coopName);
    return await card.isVisible().catch(() => false);
  }
}
