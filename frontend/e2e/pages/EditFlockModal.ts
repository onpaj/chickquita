import { Page, Locator } from '@playwright/test';
import type { FlockTestData } from '../fixtures/flock.fixture';

export class EditFlockModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly modalTitle: Locator;
  readonly identifierInput: Locator;
  readonly hatchDateInput: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.getByRole('dialog');
    this.modalTitle = page.getByRole('dialog').getByRole('heading', { name: /upravit hejno|edit flock/i });
    this.identifierInput = page.getByRole('dialog').getByLabel(/identifikátor hejna|flock identifier/i);
    this.hatchDateInput = page.getByRole('dialog').getByLabel(/datum líhnutí|hatch date/i);
    this.submitButton = page.getByRole('dialog').getByRole('button', { name: /uložit|save/i });
    this.cancelButton = page.getByRole('dialog').getByRole('button', { name: /zrušit|cancel/i });
    this.errorMessage = page.getByRole('dialog').locator('.MuiFormHelperText-root.Mui-error');
  }

  /**
   * Fill the flock form with provided data
   * Note: Edit modal only allows editing identifier and hatchDate
   * Composition (hens, roosters, chicks) cannot be edited here
   */
  async fillForm(data: Partial<FlockTestData>) {
    if (data.identifier !== undefined) {
      await this.identifierInput.clear();
      await this.identifierInput.fill(data.identifier);
    }

    if (data.hatchDate !== undefined) {
      await this.hatchDateInput.clear();
      await this.hatchDateInput.fill(data.hatchDate);
    }
  }

  /**
   * Submit the form
   */
  async submit() {
    await this.submitButton.click();
  }

  /**
   * Cancel the form
   */
  async cancel() {
    await this.cancelButton.click();
  }

  /**
   * Edit a flock (fill form and submit)
   */
  async editFlock(data: Partial<FlockTestData>) {
    await this.fillForm(data);
    await this.submit();
    await this.waitForClose();
  }

  /**
   * Check if modal is visible
   */
  async isVisible(): Promise<boolean> {
    return await this.modal.isVisible();
  }

  /**
   * Wait for modal to close
   */
  async waitForClose() {
    await this.modal.waitFor({ state: 'hidden', timeout: 30000 });
  }

  /**
   * Check if submit button is disabled
   */
  async isSubmitDisabled(): Promise<boolean> {
    return await this.submitButton.isDisabled();
  }

  /**
   * Get error message text
   */
  async getErrorMessage(): Promise<string> {
    return await this.errorMessage.first().textContent() || '';
  }

  /**
   * Check if error message is visible
   */
  async hasError(): Promise<boolean> {
    return await this.errorMessage.first().isVisible();
  }

  /**
   * Get current value of identifier input
   */
  async getCurrentIdentifier(): Promise<string> {
    return await this.identifierInput.inputValue();
  }

  /**
   * Get current value of hatch date input
   */
  async getCurrentHatchDate(): Promise<string> {
    return await this.hatchDateInput.inputValue();
  }
}
