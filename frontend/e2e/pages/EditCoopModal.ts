import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for the Edit Coop Modal
 */
export class EditCoopModal {
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
    this.modalTitle = this.modal.getByRole('heading', { name: /edit coop|upravit kurník/i });
    this.nameInput = this.modal.getByLabel(/name|název/i);
    this.locationInput = this.modal.getByLabel(/location|umístění|místo/i);
    this.submitButton = this.modal.getByRole('button', { name: /save|update|uložit|aktualizovat/i });
    this.cancelButton = this.modal.getByRole('button', { name: /cancel|zrušit|close/i });
    this.errorMessage = this.modal.locator('[role="alert"]').or(this.modal.locator('.MuiFormHelperText-root.Mui-error'));
  }

  async fillForm(name: string, location?: string) {
    await this.nameInput.clear();
    await this.nameInput.fill(name);

    if (location !== undefined) {
      await this.locationInput.clear();
      await this.locationInput.fill(location);
    }
  }

  async submit() {
    await this.submitButton.click();
  }

  async cancel() {
    await this.cancelButton.click();
  }

  async editCoop(name: string, location?: string) {
    await this.fillForm(name, location);
    await this.submit();
  }

  async isVisible(): Promise<boolean> {
    return await this.modal.isVisible();
  }

  async waitForClose() {
    await this.modal.waitFor({ state: 'hidden', timeout: 5000 });
  }

  async getCurrentName(): Promise<string> {
    return await this.nameInput.inputValue();
  }

  async getCurrentLocation(): Promise<string> {
    return await this.locationInput.inputValue();
  }
}
