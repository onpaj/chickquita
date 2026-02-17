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
    // Use first matching button (handles case where there are multiple add buttons)
    this.createCoopButton = page.getByRole('button', { name: /add coop|přidat kurník|create coop/i }).first();
    this.emptyStateMessage = page.getByText(/no coops here yet|zatím tu nejsou žádné kurníky/i);
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

  /**
   * Shared helper to click menu action with proper timing
   * Waits for menu to appear and stabilize before clicking item
   */
  private async clickMenuAction(coopName: string, actionName: RegExp) {
    const card = await this.getCoopCard(coopName);

    // Open the menu first
    await card.getByRole('button', { name: /more|více/i }).click();

    // Wait for menu item to be visible and stable
    const menuItem = this.page.getByRole('menuitem', { name: actionName });
    await menuItem.waitFor({ state: 'visible', timeout: 5000 });

    // Small delay for DOM stability
    await this.page.waitForTimeout(100);

    // Click the menu item
    await menuItem.click();
  }

  async clickEditCoop(coopName: string) {
    await this.clickMenuAction(coopName, /edit|upravit/i);
  }

  async clickArchiveCoop(coopName: string) {
    await this.clickMenuAction(coopName, /archive|archivovat/i);
  }

  async clickDeleteCoop(coopName: string) {
    await this.clickMenuAction(coopName, /delete|smazat/i);
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

  async clickAddButton() {
    await this.createCoopButton.click();
  }

  async waitForCoopCard(coopName: string) {
    const card = await this.getCoopCard(coopName);
    await card.waitFor({ state: 'visible', timeout: 10000 });
  }

  /**
   * Delete all existing coops in the list
   * Useful for test cleanup to ensure empty state
   */
  async deleteAllCoops(): Promise<void> {
    let coopCount = await this.getCoopCount();

    while (coopCount > 0) {
      // Get the first coop card
      const firstCard = this.coopCardsList.first();

      // Open the menu
      const moreButton = firstCard.getByRole('button', { name: /more|více/i });
      await moreButton.click();

      // Wait for menu to be visible
      const deleteMenuItem = this.page.getByRole('menuitem', { name: /delete|smazat/i });
      await deleteMenuItem.waitFor({ state: 'visible', timeout: 5000 });

      // Click delete in the menu
      await deleteMenuItem.click();

      // Wait for confirmation dialog
      const confirmDialog = this.page.getByRole('dialog');
      await confirmDialog.waitFor({ state: 'visible', timeout: 5000 });

      // Confirm deletion
      const confirmButton = confirmDialog.getByRole('button', { name: /delete|confirm|yes|ano|smazat/i });
      await confirmButton.waitFor({ state: 'visible', timeout: 5000 });
      await confirmButton.click();

      // Wait for dialog to close
      await confirmDialog.waitFor({ state: 'hidden', timeout: 15000 });

      // Wait for network to be idle after deletion
      await this.page.waitForLoadState('networkidle');

      // Update count
      const newCount = await this.getCoopCount();

      // Safety check: if count didn't decrease, break to avoid infinite loop
      if (newCount >= coopCount) {
        throw new Error(`Failed to delete coop: count remained at ${coopCount}`);
      }

      coopCount = newCount;
    }
  }
}
