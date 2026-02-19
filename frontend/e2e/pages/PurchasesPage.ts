import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for the Purchases List page
 */
export class PurchasesPage {
  readonly page: Page;
  readonly pageTitle: Locator;
  readonly addPurchaseButton: Locator;
  readonly emptyStateMessage: Locator;
  readonly purchaseCardsList: Locator;
  readonly fromDateFilter: Locator;
  readonly toDateFilter: Locator;
  readonly typeFilter: Locator;
  readonly flockFilter: Locator;

  constructor(page: Page) {
    this.page = page;
    this.pageTitle = page.getByRole('heading', { name: /nákupy|purchases/i });
    this.addPurchaseButton = page.getByLabel(/přidat nákup|add purchase/i).first();
    this.emptyStateMessage = page.getByText(/zatím žádné nákupy|no purchases yet/i);
    this.purchaseCardsList = page.locator('[role="article"]');
    this.fromDateFilter = page.getByLabel(/od data|from date/i).last();
    this.toDateFilter = page.getByLabel(/do data|to date/i).last();
    this.typeFilter = page.getByLabel(/typ(?! nákupu)|type(?! of)/i).first();
    this.flockFilter = page.getByLabel(/hejno|flock/i);
  }

  /**
   * Navigate to the purchases page
   */
  async goto() {
    await this.page.goto('/purchases');
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Open the create purchase modal
   */
  async openCreatePurchaseModal() {
    await this.addPurchaseButton.click();
    await this.page.waitForTimeout(500); // Wait for modal animation
  }

  /**
   * Get a purchase card by its name
   */
  getPurchaseCard(purchaseName: string): Locator {
    return this.purchaseCardsList.filter({ hasText: purchaseName }).first();
  }

  /**
   * Get count of visible purchase cards
   */
  async getPurchaseCount(): Promise<number> {
    return await this.purchaseCardsList.count();
  }

  /**
   * Click the edit button for a specific purchase
   */
  async clickEditPurchase(purchaseName: string) {
    const card = this.getPurchaseCard(purchaseName);
    const editButton = card.getByLabel(/upravit|edit/i);
    await editButton.click();
    await this.page.waitForTimeout(500); // Wait for modal animation
  }

  /**
   * Click the delete button for a specific purchase
   */
  async clickDeletePurchase(purchaseName: string) {
    const card = this.getPurchaseCard(purchaseName);
    const deleteButton = card.getByLabel(/smazat|delete/i);
    await deleteButton.click();
    await this.page.waitForTimeout(500); // Wait for dialog animation
  }

  /**
   * Filter purchases by date range
   */
  async filterByDateRange(fromDate: string, toDate: string) {
    await this.fromDateFilter.fill(fromDate);
    await this.toDateFilter.fill(toDate);
    await this.page.waitForTimeout(500); // Wait for filtering
  }

  /**
   * Filter purchases by type
   */
  async filterByType(typeName: string) {
    await this.typeFilter.click();
    await this.page.getByRole('option', { name: new RegExp(typeName, 'i') }).click();
    await this.page.waitForTimeout(500); // Wait for filtering
  }

  /**
   * Clear type filter (select "All")
   */
  async clearTypeFilter() {
    await this.typeFilter.click();
    await this.page.getByRole('option', { name: /vše|all/i }).click();
    await this.page.waitForTimeout(500); // Wait for filtering
  }

  /**
   * Check if empty state is visible
   */
  async isEmptyStateVisible(): Promise<boolean> {
    return await this.emptyStateMessage.isVisible();
  }

  /**
   * Check if a purchase is visible on the page
   */
  async isPurchaseVisible(purchaseName: string): Promise<boolean> {
    const card = this.getPurchaseCard(purchaseName);
    return await card.isVisible().catch(() => false);
  }

  /**
   * Wait for purchases to load
   */
  async waitForPurchasesToLoad() {
    // Wait for either empty state or purchase cards to appear
    await this.page.waitForFunction(
      () => {
        const emptyState = document.querySelector('[role="region"]');
        const purchaseCards = document.querySelectorAll('[role="article"]');
        return (
          (emptyState &&
            (emptyState.textContent?.includes('Zatím žádné nákupy') ||
              emptyState.textContent?.includes('No purchases yet'))) ||
          purchaseCards.length > 0
        );
      },
      { timeout: 10000 }
    );
  }

  /**
   * Get purchase card details (name, amount, quantity)
   */
  async getPurchaseDetails(purchaseName: string): Promise<{
    name: string;
    amount?: number;
    quantity?: number;
  }> {
    const card = this.getPurchaseCard(purchaseName);
    const cardText = (await card.textContent()) || '';

    // Extract amount (e.g., "250.50 Kč")
    const amountMatch = cardText.match(/(\d+(?:\.\d{2})?)\s*(?:Kč|CZK)/);
    const amount = amountMatch ? parseFloat(amountMatch[1]) : undefined;

    // Extract quantity (e.g., "25 kg")
    const quantityMatch = cardText.match(/(\d+(?:\.\d{2})?)\s*(?:kg|ks|l)/);
    const quantity = quantityMatch ? parseFloat(quantityMatch[1]) : undefined;

    return {
      name: purchaseName,
      amount,
      quantity,
    };
  }
}
