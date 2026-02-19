import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for the Purchase Form Modal (Create/Edit)
 */
export class PurchaseFormModal {
  readonly page: Page;
  readonly modal: Locator;
  readonly modalTitle: Locator;
  readonly typeSelect: Locator;
  readonly nameInput: Locator;
  readonly purchaseDateInput: Locator;
  readonly amountInput: Locator;
  readonly quantityInput: Locator;
  readonly unitSelect: Locator;
  readonly consumedDateInput: Locator;
  readonly notesInput: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    this.modal = page.locator('[role="dialog"]');
    this.modalTitle = page.getByText(/vytvořit nákup|upravit nákup|create purchase|edit purchase/i);
    this.typeSelect = page.getByLabel(/typ nákupu|purchase type/i);
    this.nameInput = page.getByLabel(/^název$|^name$/i);
    this.purchaseDateInput = page.getByLabel(/datum nákupu|purchase date/i);
    this.amountInput = page.getByLabel(/^částka \(kč\)$|^amount$/i);
    this.quantityInput = page.getByLabel(/^množství$|^quantity$/i);
    this.unitSelect = page.getByLabel(/jednotka|unit/i).first();
    this.consumedDateInput = page.getByLabel(/datum spotřeby|consumed date/i);
    this.notesInput = page.getByLabel(/poznámky|notes/i);
    this.submitButton = page.getByRole('button', { name: /^vytvořit$|^uložit$|^create$|^save$/i });
    this.cancelButton = page.getByRole('button', { name: /^zrušit$|^cancel$/i });
    this.errorMessage = page.locator('.MuiFormHelperText-root.Mui-error');
  }

  /**
   * Fill the complete purchase form
   */
  async fillForm(data: {
    type?: string;
    name: string;
    purchaseDate?: string;
    amount: number;
    quantity: number;
    unit?: string;
    consumedDate?: string;
    notes?: string;
  }) {
    // Type selection
    if (data.type) {
      await this.typeSelect.click();
      await this.page.getByRole('option', { name: new RegExp(data.type, 'i') }).first().click();
      // Wait for dropdown to close before interacting with next field (Mobile Safari needs this)
      await this.page.waitForTimeout(300);
    }

    // Name (with autocomplete support)
    await this.nameInput.fill(data.name);

    // Purchase date
    if (data.purchaseDate) {
      await this.purchaseDateInput.fill(data.purchaseDate);
    }

    // Amount
    await this.amountInput.fill(data.amount.toString());

    // Quantity
    await this.quantityInput.fill(data.quantity.toString());

    // Unit selection
    if (data.unit) {
      await this.unitSelect.click();
      await this.page.getByRole('option', { name: new RegExp(data.unit, 'i') }).first().click();
    }

    // Consumed date (optional)
    if (data.consumedDate) {
      await this.consumedDateInput.fill(data.consumedDate);
    }

    // Notes (optional)
    if (data.notes) {
      await this.notesInput.fill(data.notes);
    }
  }

  /**
   * Create a purchase (fill form and submit)
   */
  async createPurchase(data: {
    type?: string;
    name: string;
    purchaseDate?: string;
    amount: number;
    quantity: number;
    unit?: string;
    consumedDate?: string;
    notes?: string;
  }) {
    await this.fillForm(data);
    await this.submitButton.click();
  }

  /**
   * Edit a purchase (update fields and submit)
   */
  async editPurchase(data: Partial<{
    type: string;
    name: string;
    purchaseDate: string;
    amount: number;
    quantity: number;
    unit: string;
    consumedDate: string;
    notes: string;
  }>) {
    if (data.type) {
      await this.typeSelect.click();
      await this.page.getByRole('option', { name: new RegExp(data.type, 'i') }).first().click();
    }

    if (data.name !== undefined) {
      await this.nameInput.clear();
      await this.nameInput.fill(data.name);
    }

    if (data.purchaseDate) {
      await this.purchaseDateInput.fill(data.purchaseDate);
    }

    if (data.amount !== undefined) {
      await this.amountInput.clear();
      await this.amountInput.fill(data.amount.toString());
    }

    if (data.quantity !== undefined) {
      await this.quantityInput.clear();
      await this.quantityInput.fill(data.quantity.toString());
    }

    if (data.unit) {
      await this.unitSelect.click();
      await this.page.getByRole('option', { name: new RegExp(data.unit, 'i') }).first().click();
    }

    if (data.consumedDate !== undefined) {
      await this.consumedDateInput.fill(data.consumedDate);
    }

    if (data.notes !== undefined) {
      await this.notesInput.clear();
      await this.notesInput.fill(data.notes);
    }

    await this.submitButton.click();
  }

  /**
   * Cancel the form
   */
  async cancel() {
    await this.cancelButton.click();
  }

  /**
   * Wait for modal to close
   */
  async waitForClose() {
    await this.modal.waitFor({ state: 'hidden', timeout: 15000 });
  }

  /**
   * Get current form values
   */
  async getCurrentValues(): Promise<{
    name: string;
    amount: string;
    quantity: string;
  }> {
    return {
      name: await this.nameInput.inputValue(),
      amount: await this.amountInput.inputValue(),
      quantity: await this.quantityInput.inputValue(),
    };
  }

  /**
   * Get error message text
   */
  async getErrorMessage(): Promise<string> {
    const count = await this.errorMessage.count();
    if (count === 0) return '';
    return (await this.errorMessage.first().textContent()) || '';
  }

  /**
   * Check if submit button is disabled
   */
  async isSubmitDisabled(): Promise<boolean> {
    return await this.submitButton.isDisabled();
  }

  /**
   * Fill partial form (for validation testing)
   */
  async fillPartialForm(data: Partial<{
    type: string;
    name: string;
    purchaseDate: string;
    amount: number;
    quantity: number;
    unit: string;
  }>) {
    if (data.type) {
      await this.typeSelect.click();
      await this.page.getByRole('option', { name: new RegExp(data.type, 'i') }).first().click();
    }

    if (data.name !== undefined) {
      await this.nameInput.fill(data.name);
    }

    if (data.purchaseDate) {
      await this.purchaseDateInput.fill(data.purchaseDate);
    }

    if (data.amount !== undefined) {
      await this.amountInput.fill(data.amount.toString());
    }

    if (data.quantity !== undefined) {
      await this.quantityInput.fill(data.quantity.toString());
    }

    if (data.unit) {
      await this.unitSelect.click();
      await this.page.getByRole('option', { name: new RegExp(data.unit, 'i') }).first().click();
    }
  }
}
