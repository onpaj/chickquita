import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for the Delete Purchase Confirmation Dialog
 */
export class DeletePurchaseDialog {
  readonly page: Page;
  readonly dialog: Locator;
  readonly dialogTitle: Locator;
  readonly dialogContent: Locator;
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.dialog = page.locator('[role="dialog"]');
    this.dialogTitle = page.getByText(/smazat nákup|delete purchase/i);
    this.dialogContent = page.getByText(/opravdu chcete smazat|are you sure you want to delete/i);
    this.confirmButton = page.getByRole('button', { name: /^smazat$|^delete$/i }).last();
    this.cancelButton = page.getByRole('button', { name: /^zrušit$|^cancel$/i });
  }

  /**
   * Confirm deletion
   */
  async confirm() {
    await this.confirmButton.click();
    await this.page.waitForTimeout(500); // Wait for deletion to complete
  }

  /**
   * Cancel deletion
   */
  async cancel() {
    await this.cancelButton.click();
  }

  /**
   * Get the purchase name from the dialog content
   */
  async getPurchaseName(): Promise<string> {
    const content = (await this.dialogContent.textContent()) || '';
    // Extract purchase name from confirmation message
    // Message format: "Opravdu chcete smazat nákup X?"
    const match = content.match(/nákup\s+"?([^"?]+)"?/i) || content.match(/delete\s+"?([^"?]+)"?/i);
    return match ? match[1].trim() : '';
  }

  /**
   * Wait for dialog to appear
   */
  async waitForDialog() {
    await this.dialog.waitFor({ state: 'visible', timeout: 5000 });
  }

  /**
   * Wait for dialog to close
   */
  async waitForClose() {
    await this.dialog.waitFor({ state: 'hidden', timeout: 15000 });
  }
}
