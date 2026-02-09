import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for the Create Coop Modal
 */
export class CreateCoopModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly modalTitle: Locator;
  readonly nameInput: Locator;
  readonly locationInput: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.getByRole('dialog');
    this.modalTitle = this.modal.getByRole('heading', { name: /add coop|create coop|přidat kurník|nový kurník/i });
    this.nameInput = this.modal.getByLabel(/name|název/i);
    this.locationInput = this.modal.getByLabel(/location|umístění|místo/i);
    this.submitButton = this.modal.getByRole('button', { name: /create|add|přidat|vytvořit|save|uložit/i });
    this.cancelButton = this.modal.getByRole('button', { name: /cancel|zrušit|close/i });
    this.errorMessage = this.modal.locator('[role="alert"]').or(this.modal.locator('.MuiFormHelperText-root.Mui-error'));
  }

  async fillForm(name: string, location?: string) {
    await this.nameInput.fill(name);
    if (location) {
      await this.locationInput.fill(location);
    }
  }

  async submit() {
    await this.submitButton.click();
  }

  async cancel() {
    await this.cancelButton.click();
  }

  async createCoop(name: string, location?: string) {
    await this.fillForm(name, location);
    await this.submit();
  }

  async isVisible(): Promise<boolean> {
    return await this.modal.isVisible();
  }

  async waitForClose() {
    await this.modal.waitFor({ state: 'hidden' });
  }

  async isSubmitDisabled(): Promise<boolean> {
    return await this.submitButton.isDisabled();
  }

  async getValidationError(): Promise<string | null> {
    const isVisible = await this.errorMessage.isVisible();
    if (!isVisible) {
      return null;
    }
    return await this.errorMessage.textContent();
  }
}
