import { Page, Locator } from '@playwright/test';

export class ArchiveFlockDialog {
  readonly page: Page;
  readonly dialog: Locator;
  readonly dialogTitle: Locator;
  readonly dialogMessage: Locator;
  readonly flockName: Locator;
  readonly confirmButton: Locator;
  readonly cancelButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.dialog = page.getByRole('dialog');
    this.dialogTitle = page.getByRole('dialog').getByRole('heading', { name: /archivovat hejno|archive flock/i });
    this.dialogMessage = page.getByRole('dialog').locator('#archive-flock-dialog-description');
    this.flockName = page.getByRole('dialog').locator('.MuiDialogContent-root .MuiDialogContentText-root').nth(1);
    this.confirmButton = page.getByRole('dialog').getByRole('button', { name: /archivovat|archive/i });
    this.cancelButton = page.getByRole('dialog').getByRole('button', { name: /zrušit|cancel/i });
  }

  /**
   * Confirm the archive action
   */
  async confirm() {
    await this.confirmButton.click();
    await this.waitForClose();
  }

  /**
   * Cancel the archive action
   */
  async cancel() {
    await this.cancelButton.click();
    await this.waitForClose();
  }

  /**
   * Check if dialog is visible
   */
  async isVisible(): Promise<boolean> {
    return await this.dialog.isVisible();
  }

  /**
   * Wait for dialog to close
   */
  async waitForClose() {
    await this.dialog.waitFor({ state: 'hidden', timeout: 5000 });
  }

  /**
   * Get the flock name displayed in the dialog
   */
  async getFlockName(): Promise<string> {
    return await this.flockName.textContent() || '';
  }

  /**
   * Get the dialog message text
   */
  async getMessage(): Promise<string> {
    return await this.dialogMessage.textContent() || '';
  }

  /**
   * Check if confirm button is disabled
   */
  async isConfirmDisabled(): Promise<boolean> {
    return await this.confirmButton.isDisabled();
  }
}
