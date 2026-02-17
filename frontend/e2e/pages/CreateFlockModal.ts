import { Page, Locator } from '@playwright/test';
import type { FlockTestData } from '../fixtures/flock.fixture';

export class CreateFlockModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly modalTitle: Locator;
  readonly identifierInput: Locator;
  readonly hatchDateInput: Locator;
  readonly hensInput: Locator;
  readonly roostersInput: Locator;
  readonly chicksInput: Locator;
  readonly hensIncrementButton: Locator;
  readonly hensDecrementButton: Locator;
  readonly roostersIncrementButton: Locator;
  readonly roostersDecrementButton: Locator;
  readonly chicksIncrementButton: Locator;
  readonly chicksDecrementButton: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.getByRole('dialog');
    this.modalTitle = page.getByRole('dialog').getByRole('heading', { name: /přidat hejno|add flock/i });
    this.identifierInput = page.getByRole('dialog').getByLabel(/identifikátor hejna|flock identifier/i);
    this.hatchDateInput = page.getByRole('dialog').getByLabel(/datum líhnutí|hatch date/i);
    this.hensInput = page.getByRole('dialog').getByLabel(/^slepice$|^hens$/i);
    this.roostersInput = page.getByRole('dialog').getByLabel(/^kohouti$|^roosters$/i);
    this.chicksInput = page.getByRole('dialog').getByLabel(/^kuřata$|^chicks$/i);

    // Increment/decrement buttons - using aria-label since they're icon buttons
    this.hensIncrementButton = page.getByRole('dialog')
      .locator('[aria-label*="Zvýšit"], [aria-label*="Increase"]')
      .filter({ has: page.locator('input[type="number"]').filter({ has: page.getByLabel(/^slepice$|^hens$/i) }) })
      .last();
    this.hensDecrementButton = page.getByRole('dialog')
      .locator('[aria-label*="Snížit"], [aria-label*="Decrease"]')
      .filter({ has: page.locator('input[type="number"]').filter({ has: page.getByLabel(/^slepice$|^hens$/i) }) })
      .first();

    this.submitButton = page.getByRole('dialog').getByRole('button', { name: /uložit|save/i });
    this.cancelButton = page.getByRole('dialog').getByRole('button', { name: /zrušit|cancel/i });
    this.errorMessage = page.getByRole('dialog').locator('.MuiFormHelperText-root.Mui-error');
  }

  /**
   * Fill the flock form with provided data
   */
  async fillForm(data: FlockTestData) {
    // Clear and fill identifier
    await this.identifierInput.clear();
    await this.identifierInput.fill(data.identifier);

    // Clear and fill hatch date
    await this.hatchDateInput.clear();
    await this.hatchDateInput.fill(data.hatchDate);

    // Clear and fill hens
    await this.hensInput.clear();
    await this.hensInput.fill(data.hens.toString());

    // Clear and fill roosters
    await this.roostersInput.clear();
    await this.roostersInput.fill(data.roosters.toString());

    // Clear and fill chicks
    await this.chicksInput.clear();
    await this.chicksInput.fill(data.chicks.toString());
  }

  /**
   * Fill the form with partial data (for validation testing)
   */
  async fillPartialForm(data: Partial<FlockTestData>) {
    if (data.identifier !== undefined) {
      await this.identifierInput.clear();
      if (data.identifier) {
        await this.identifierInput.fill(data.identifier);
      }
    }

    if (data.hatchDate !== undefined) {
      await this.hatchDateInput.clear();
      if (data.hatchDate) {
        await this.hatchDateInput.fill(data.hatchDate);
      }
    }

    if (data.hens !== undefined) {
      await this.hensInput.clear();
      await this.hensInput.fill(data.hens.toString());
    }

    if (data.roosters !== undefined) {
      await this.roostersInput.clear();
      await this.roostersInput.fill(data.roosters.toString());
    }

    if (data.chicks !== undefined) {
      await this.chicksInput.clear();
      await this.chicksInput.fill(data.chicks.toString());
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
   * Create a flock (fill form and submit)
   */
  async createFlock(data: FlockTestData) {
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
    await this.modal.waitFor({ state: 'hidden', timeout: 15000 });
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
   * Increment hens count using the + button
   */
  async incrementHens() {
    await this.hensIncrementButton.click();
  }

  /**
   * Decrement hens count using the - button
   */
  async decrementHens() {
    await this.hensDecrementButton.click();
  }

  /**
   * Get current value of identifier input
   */
  async getIdentifier(): Promise<string> {
    return await this.identifierInput.inputValue();
  }

  /**
   * Get current value of hatch date input
   */
  async getHatchDate(): Promise<string> {
    return await this.hatchDateInput.inputValue();
  }

  /**
   * Get current value of hens input
   */
  async getHens(): Promise<number> {
    const value = await this.hensInput.inputValue();
    return parseInt(value) || 0;
  }

  /**
   * Get current value of roosters input
   */
  async getRoosters(): Promise<number> {
    const value = await this.roostersInput.inputValue();
    return parseInt(value) || 0;
  }

  /**
   * Get current value of chicks input
   */
  async getChicks(): Promise<number> {
    const value = await this.chicksInput.inputValue();
    return parseInt(value) || 0;
  }
}
