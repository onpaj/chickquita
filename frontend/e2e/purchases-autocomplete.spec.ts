import { test, expect } from '@playwright/test';
import { PurchasesPage } from './pages/PurchasesPage';
import { PurchaseFormModal } from './pages/PurchaseFormModal';

/**
 * E2E Tests for Purchase Autocomplete
 *
 * Regression test for: TypeError: options.filter is not a function
 * The Autocomplete component in the purchase form must handle non-array
 * API responses gracefully without crashing.
 *
 * Coverage:
 * - Opening the purchase form does not crash
 * - Typing in the name field triggers autocomplete without errors
 * - Form remains functional after autocomplete interaction
 * - Purchase can be created after using the name field
 */
test.describe('Purchase Form - Autocomplete Regression', () => {
  let purchasesPage: PurchasesPage;
  let purchaseFormModal: PurchaseFormModal;

  test.beforeEach(async ({ page }) => {
    purchasesPage = new PurchasesPage(page);
    purchaseFormModal = new PurchaseFormModal(page);
    await purchasesPage.goto();
  });

  test('should open purchase form without Autocomplete crash', async ({ page }) => {
    await purchasesPage.openCreatePurchaseModal();
    await expect(purchaseFormModal.modalTitle).toBeVisible();

    // The name input (Autocomplete) should be visible and functional
    await expect(purchaseFormModal.nameInput).toBeVisible();

    // No console errors related to options.filter
    const consoleErrors: string[] = [];
    page.on('console', (msg) => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    // Type a short string (< 2 chars, no API call)
    await purchaseFormModal.nameInput.fill('K');
    await page.waitForTimeout(500);

    // Type enough to trigger autocomplete (>= 2 chars)
    await purchaseFormModal.nameInput.fill('Kr');
    await page.waitForTimeout(500);

    // Verify no crash - form should still be functional
    await expect(purchaseFormModal.nameInput).toBeVisible();
    await expect(purchaseFormModal.submitButton).toBeVisible();

    // Verify no "options.filter is not a function" error
    const filterErrors = consoleErrors.filter((e) =>
      e.includes('options.filter is not a function')
    );
    expect(filterErrors).toHaveLength(0);

    await purchaseFormModal.cancel();
  });

  test('should create purchase after typing in autocomplete field', async ({ page }) => {
    await purchasesPage.openCreatePurchaseModal();
    await expect(purchaseFormModal.modalTitle).toBeVisible();

    // Type in name field to trigger autocomplete, then set a value
    await purchaseFormModal.nameInput.fill('Te');
    await page.waitForTimeout(400); // Wait for debounce

    // Clear and set final name
    await purchaseFormModal.nameInput.fill('Test Autocomplete Purchase');

    // Fill remaining required fields
    const today = new Date().toISOString().split('T')[0];
    await purchaseFormModal.purchaseDateInput.fill(today);

    // Set amount and quantity using the form fill method
    await purchaseFormModal.fillPartialForm({
      type: 'Krmivo',
      amount: 100,
      quantity: 10,
      unit: 'kg',
    });

    // Submit should be enabled
    await expect(purchaseFormModal.submitButton).not.toBeDisabled();
    await purchaseFormModal.submitButton.click();

    // Wait for modal to close (purchase created successfully)
    await purchaseFormModal.waitForClose();

    // Verify purchase appears in the list
    await purchasesPage.waitForPurchasesToLoad();
    await expect(
      purchasesPage.getPurchaseCard('Test Autocomplete Purchase')
    ).toBeVisible();
  });

  test('should handle rapid typing without crash', async ({ page }) => {
    await purchasesPage.openCreatePurchaseModal();
    await expect(purchaseFormModal.nameInput).toBeVisible();

    // Simulate rapid typing (triggers multiple debounce resets)
    await purchaseFormModal.nameInput.pressSequentially('Krmivo Premium', {
      delay: 50,
    });

    // Wait for debounce to settle
    await page.waitForTimeout(500);

    // Form should still be functional
    await expect(purchaseFormModal.nameInput).toBeVisible();
    await expect(purchaseFormModal.submitButton).toBeVisible();

    // Name field should have the typed value
    const nameValue = await purchaseFormModal.nameInput.inputValue();
    expect(nameValue).toBe('Krmivo Premium');

    await purchaseFormModal.cancel();
  });

  test('should handle clearing and retyping in autocomplete field', async ({ page }) => {
    await purchasesPage.openCreatePurchaseModal();
    await expect(purchaseFormModal.nameInput).toBeVisible();

    // Type to trigger autocomplete
    await purchaseFormModal.nameInput.fill('Krmivo');
    await page.waitForTimeout(400);

    // Clear the field
    await purchaseFormModal.nameInput.fill('');
    await page.waitForTimeout(200);

    // Retype - this should not crash even if previous query was in-flight
    await purchaseFormModal.nameInput.fill('Vitamíny');
    await page.waitForTimeout(400);

    // Form should still be functional
    await expect(purchaseFormModal.nameInput).toBeVisible();
    const nameValue = await purchaseFormModal.nameInput.inputValue();
    expect(nameValue).toBe('Vitamíny');

    await purchaseFormModal.cancel();
  });
});
