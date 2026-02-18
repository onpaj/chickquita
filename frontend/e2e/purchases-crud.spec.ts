import { test, expect } from '@playwright/test';
import { PurchasesPage } from './pages/PurchasesPage';
import { PurchaseFormModal } from './pages/PurchaseFormModal';
import { DeletePurchaseDialog } from './pages/DeletePurchaseDialog';

/**
 * E2E Tests for Purchase CRUD Flow
 * US-041: E2E - Purchase CRUD Flow
 *
 * Comprehensive E2E test for purchase lifecycle covering:
 * 1. Create Purchase
 * 2. Filter Purchases
 * 3. Edit Purchase
 * 4. Delete Purchase
 * 5. Autocomplete functionality
 *
 * Tests run on desktop Chrome and mobile viewport
 * Uses Page Object Model pattern
 * Includes screenshots on failure (configured in playwright.config.ts)
 */
test.describe('Purchase CRUD Flow - Complete Lifecycle', () => {
  let purchasesPage: PurchasesPage;
  let purchaseFormModal: PurchaseFormModal;
  let deletePurchaseDialog: DeletePurchaseDialog;

  test.beforeEach(async ({ page }) => {
    // Initialize page objects
    purchasesPage = new PurchasesPage(page);
    purchaseFormModal = new PurchaseFormModal(page);
    deletePurchaseDialog = new DeletePurchaseDialog(page);

    // Navigate to purchases page
    await purchasesPage.goto();
  });

  test.describe('Scenario 1: Create Purchase', () => {
    test('@smoke should create a new purchase with all required fields', async ({ page }) => {
      // Open create modal
      await purchasesPage.openCreatePurchaseModal();
      await expect(purchaseFormModal.modalTitle).toBeVisible();

      // Fill and submit form
      const purchaseData = {
        type: 'Krmivo',
        name: 'E2E Test Feed Purchase',
        purchaseDate: '2024-02-15',
        amount: 250.5,
        quantity: 25,
        unit: 'kg',
        notes: 'E2E test purchase for CRUD flow',
      };

      await purchaseFormModal.createPurchase(purchaseData);

      // Wait for modal to close
      await purchaseFormModal.waitForClose();

      // Verify purchase appears in the list
      await purchasesPage.waitForPurchasesToLoad();
      await expect(purchasesPage.getPurchaseCard(purchaseData.name)).toBeVisible();

      // Verify purchase details
      const details = await purchasesPage.getPurchaseDetails(purchaseData.name);
      expect(details.name).toBe(purchaseData.name);
    });

    test('should create purchase with minimal required fields', async () => {
      await purchasesPage.openCreatePurchaseModal();

      const minimalData = {
        type: 'Krmivo',
        name: 'Minimal Purchase',
        amount: 100,
        quantity: 10,
        unit: 'kg',
      };

      await purchaseFormModal.createPurchase(minimalData);
      await purchaseFormModal.waitForClose();

      await purchasesPage.waitForPurchasesToLoad();
      await expect(purchasesPage.getPurchaseCard(minimalData.name)).toBeVisible();
    });

    test('should show validation error for empty name', async () => {
      await purchasesPage.openCreatePurchaseModal();

      // Try to submit with empty name
      await purchaseFormModal.fillPartialForm({
        type: 'Krmivo',
        name: '',
        amount: 100,
        quantity: 10,
      });

      // Blur name field to trigger validation
      await purchaseFormModal.nameInput.blur();

      // Submit button should be disabled
      await expect(purchaseFormModal.submitButton).toBeDisabled();
    });

    test('should show validation error for zero amount', async () => {
      await purchasesPage.openCreatePurchaseModal();

      await purchaseFormModal.fillPartialForm({
        type: 'Krmivo',
        name: 'Test Purchase',
        amount: 0,
        quantity: 10,
      });

      // Submit button should be disabled
      await expect(purchaseFormModal.submitButton).toBeDisabled();
    });

    test('should show validation error for future purchase date', async () => {
      await purchasesPage.openCreatePurchaseModal();

      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 10);
      const futureDateStr = futureDate.toISOString().split('T')[0];

      await purchaseFormModal.fillPartialForm({
        type: 'Krmivo',
        name: 'Test Purchase',
        purchaseDate: futureDateStr,
        amount: 100,
        quantity: 10,
      });

      // Blur date field to trigger validation
      await purchaseFormModal.purchaseDateInput.blur();

      // Wait for error message
      await expect(purchaseFormModal.errorMessage.first()).toBeVisible();

      const errorText = await purchaseFormModal.getErrorMessage();
      expect(errorText.toLowerCase()).toMatch(/future|budoucnosti/i);
    });

    test('should cancel purchase creation', async () => {
      // Wait for purchases to load before capturing initial count
      await purchasesPage.waitForPurchasesToLoad();
      const initialCount = await purchasesPage.getPurchaseCount();

      await purchasesPage.openCreatePurchaseModal();
      await expect(purchaseFormModal.modalTitle).toBeVisible();

      // Fill form but cancel
      await purchaseFormModal.fillForm({
        type: 'Krmivo',
        name: 'Cancelled Purchase',
        amount: 100,
        quantity: 10,
        unit: 'kg',
      });

      await purchaseFormModal.cancel();

      // Modal should close
      await expect(purchaseFormModal.modal).not.toBeVisible();

      // Wait for list to stabilize, then verify purchase count is unchanged
      await purchasesPage.waitForPurchasesToLoad();
      const finalCount = await purchasesPage.getPurchaseCount();
      expect(finalCount).toBe(initialCount);
    });

    test('should create purchases with different types', async () => {
      const purchaseTypes = [
        { type: 'Krmivo', name: 'Feed Purchase' },
        { type: 'Vitamíny', name: 'Vitamins Purchase' },
        { type: 'Podestýlka', name: 'Bedding Purchase' },
      ];

      for (const purchase of purchaseTypes) {
        await purchasesPage.openCreatePurchaseModal();

        await purchaseFormModal.createPurchase({
          type: purchase.type,
          name: purchase.name,
          amount: 100,
          quantity: 10,
          unit: 'kg',
        });

        await purchaseFormModal.waitForClose();
        await purchasesPage.waitForPurchasesToLoad();

        await expect(purchasesPage.getPurchaseCard(purchase.name)).toBeVisible();
      }
    });
  });

  test.describe('Scenario 2: Filter Purchases', () => {
    test.beforeEach(async () => {
      // Create test purchases with different dates and types
      const testPurchases = [
        {
          type: 'Krmivo',
          name: 'Feed Feb 2024',
          purchaseDate: '2024-02-10',
          amount: 200,
          quantity: 20,
          unit: 'kg',
        },
        {
          type: 'Vitamíny',
          name: 'Vitamins Mar 2024',
          purchaseDate: '2024-03-15',
          amount: 150,
          quantity: 15,
          unit: 'ks',
        },
        {
          type: 'Krmivo',
          name: 'Feed Apr 2024',
          purchaseDate: '2024-04-20',
          amount: 300,
          quantity: 30,
          unit: 'kg',
        },
      ];

      for (const purchase of testPurchases) {
        await purchasesPage.openCreatePurchaseModal();
        await purchaseFormModal.createPurchase(purchase);
        await purchaseFormModal.waitForClose();
        await purchasesPage.waitForPurchasesToLoad();
      }
    });

    test('should filter purchases by date range', async () => {
      // Filter February purchases only
      await purchasesPage.filterByDateRange('2024-02-01', '2024-02-28');

      // Should see February purchase
      await expect(purchasesPage.getPurchaseCard('Feed Feb 2024')).toBeVisible();

      // Should not see March purchase
      await expect(purchasesPage.getPurchaseCard('Vitamins Mar 2024')).not.toBeVisible();
    });

    test('should filter purchases by type', async () => {
      // Filter by Feed type
      await purchasesPage.filterByType('Krmivo');

      // Should see Feed purchases
      await expect(purchasesPage.getPurchaseCard('Feed Feb 2024')).toBeVisible();
      await expect(purchasesPage.getPurchaseCard('Feed Apr 2024')).toBeVisible();

      // Should not see Vitamins purchase
      await expect(purchasesPage.getPurchaseCard('Vitamins Mar 2024')).not.toBeVisible();
    });

    test('should clear type filter and show all purchases', async () => {
      // First filter by type
      await purchasesPage.filterByType('Krmivo');
      await expect(purchasesPage.getPurchaseCard('Vitamins Mar 2024')).not.toBeVisible();

      // Clear filter
      await purchasesPage.clearTypeFilter();

      // Should see all purchases again
      await expect(purchasesPage.getPurchaseCard('Feed Feb 2024')).toBeVisible();
      await expect(purchasesPage.getPurchaseCard('Vitamins Mar 2024')).toBeVisible();
      await expect(purchasesPage.getPurchaseCard('Feed Apr 2024')).toBeVisible();
    });

    test('should combine date and type filters', async () => {
      // Filter by date range (Feb-Mar) and type (Feed)
      await purchasesPage.filterByDateRange('2024-02-01', '2024-03-31');
      await purchasesPage.filterByType('Krmivo');

      // Should see only Feed Feb 2024
      await expect(purchasesPage.getPurchaseCard('Feed Feb 2024')).toBeVisible();

      // Should not see others
      await expect(purchasesPage.getPurchaseCard('Vitamins Mar 2024')).not.toBeVisible();
      await expect(purchasesPage.getPurchaseCard('Feed Apr 2024')).not.toBeVisible();
    });
  });

  test.describe('Scenario 3: Edit Purchase', () => {
    let originalPurchase: { type: string; name: string; amount: number; quantity: number; unit: string };

    test.beforeEach(async ({}, testInfo) => {
      // Use workerIndex to create unique names and avoid parallel test interference
      const uniqueSuffix = testInfo.workerIndex;
      originalPurchase = {
        type: 'Krmivo',
        name: `Purchase to Edit ${uniqueSuffix}`,
        amount: 100,
        quantity: 10,
        unit: 'kg',
      };
      // Create a purchase to edit
      await purchasesPage.openCreatePurchaseModal();
      await purchaseFormModal.createPurchase(originalPurchase);
      await purchaseFormModal.waitForClose();
      await purchasesPage.waitForPurchasesToLoad();
    });

    test('should edit purchase name', async ({}, testInfo) => {
      // Open edit modal
      await purchasesPage.clickEditPurchase(originalPurchase.name);
      await expect(purchaseFormModal.modalTitle).toBeVisible();

      // Verify current values are pre-filled
      const currentValues = await purchaseFormModal.getCurrentValues();
      expect(currentValues.name).toBe(originalPurchase.name);

      // Edit the name
      const newName = `Updated Purchase Name ${testInfo.workerIndex}`;
      await purchaseFormModal.editPurchase({ name: newName });

      // Wait for modal to close
      await purchaseFormModal.waitForClose();

      // Verify updated purchase appears
      await purchasesPage.waitForPurchasesToLoad();
      await expect(purchasesPage.getPurchaseCard(newName)).toBeVisible();
      await expect(purchasesPage.getPurchaseCard(originalPurchase.name)).not.toBeVisible();
    });

    test('should edit purchase amount and quantity', async () => {
      await purchasesPage.clickEditPurchase(originalPurchase.name);

      // Update amount and quantity
      await purchaseFormModal.editPurchase({
        amount: 200,
        quantity: 20,
      });

      await purchaseFormModal.waitForClose();
      await purchasesPage.waitForPurchasesToLoad();

      // Purchase should still be visible with same name
      await expect(purchasesPage.getPurchaseCard(originalPurchase.name)).toBeVisible();
    });

    test('should edit purchase type', async () => {
      await purchasesPage.clickEditPurchase(originalPurchase.name);

      // Change type from Feed to Vitamins
      await purchaseFormModal.editPurchase({
        type: 'Vitamíny',
      });

      await purchaseFormModal.waitForClose();
      await purchasesPage.waitForPurchasesToLoad();

      // Purchase should still be visible
      await expect(purchasesPage.getPurchaseCard(originalPurchase.name)).toBeVisible();
    });

    test('should cancel purchase edit', async () => {
      await purchasesPage.clickEditPurchase(originalPurchase.name);
      await expect(purchaseFormModal.modalTitle).toBeVisible();

      // Make changes but cancel
      await purchaseFormModal.fillPartialForm({
        name: 'Should Not Save',
      });

      await purchaseFormModal.cancel();

      // Modal should close
      await expect(purchaseFormModal.modal).not.toBeVisible();

      // Original purchase should still be visible
      await expect(purchasesPage.getPurchaseCard(originalPurchase.name)).toBeVisible();
      await expect(purchasesPage.getPurchaseCard('Should Not Save')).not.toBeVisible();
    });

    test('should show validation errors on invalid edit data', async () => {
      await purchasesPage.clickEditPurchase(originalPurchase.name);

      // Try to set empty name
      await purchaseFormModal.nameInput.clear();
      await purchaseFormModal.nameInput.blur();

      // Submit should be disabled
      await expect(purchaseFormModal.submitButton).toBeDisabled();
    });

    test('should edit multiple fields at once', async () => {
      await purchasesPage.clickEditPurchase(originalPurchase.name);

      const updatedData = {
        name: 'Fully Updated Purchase',
        amount: 300,
        quantity: 30,
        type: 'Podestýlka',
      };

      await purchaseFormModal.editPurchase(updatedData);
      await purchaseFormModal.waitForClose();
      await purchasesPage.waitForPurchasesToLoad();

      // Verify updated purchase is visible
      await expect(purchasesPage.getPurchaseCard(updatedData.name)).toBeVisible();
    });
  });

  test.describe('Scenario 4: Delete Purchase', () => {
    const testPurchase = {
      type: 'Krmivo',
      name: 'Purchase to Delete',
      amount: 100,
      quantity: 10,
      unit: 'kg',
    };

    test.beforeEach(async () => {
      // Create a purchase to delete
      await purchasesPage.openCreatePurchaseModal();
      await purchaseFormModal.createPurchase(testPurchase);
      await purchaseFormModal.waitForClose();
      await purchasesPage.waitForPurchasesToLoad();
    });

    test('should delete purchase after confirmation', async () => {
      // Verify purchase exists
      await expect(purchasesPage.getPurchaseCard(testPurchase.name)).toBeVisible();

      const initialCount = await purchasesPage.getPurchaseCount();

      // Click delete
      await purchasesPage.clickDeletePurchase(testPurchase.name);
      await expect(deletePurchaseDialog.dialogTitle).toBeVisible();

      // Confirm deletion
      await deletePurchaseDialog.confirm();

      // Wait for deletion to complete
      await purchasesPage.page.waitForTimeout(1000);

      // Purchase should not be visible
      await expect(purchasesPage.getPurchaseCard(testPurchase.name)).not.toBeVisible();

      // Count should decrease
      const finalCount = await purchasesPage.getPurchaseCount();
      expect(finalCount).toBe(initialCount - 1);
    });

    test('should cancel purchase deletion', async () => {
      const initialCount = await purchasesPage.getPurchaseCount();

      // Click delete
      await purchasesPage.clickDeletePurchase(testPurchase.name);
      await expect(deletePurchaseDialog.dialogTitle).toBeVisible();

      // Cancel deletion
      await deletePurchaseDialog.cancel();

      // Purchase should still be visible
      await expect(purchasesPage.getPurchaseCard(testPurchase.name)).toBeVisible();

      // Count should remain the same
      const finalCount = await purchasesPage.getPurchaseCount();
      expect(finalCount).toBe(initialCount);
    });

    test('should display confirmation dialog with purchase details', async () => {
      await purchasesPage.clickDeletePurchase(testPurchase.name);
      await deletePurchaseDialog.waitForDialog();

      // Verify dialog content
      await expect(deletePurchaseDialog.dialogTitle).toBeVisible();
      await expect(deletePurchaseDialog.dialogContent).toBeVisible();

      // Verify both buttons are visible
      await expect(deletePurchaseDialog.confirmButton).toBeVisible();
      await expect(deletePurchaseDialog.cancelButton).toBeVisible();

      await deletePurchaseDialog.cancel();
    });
  });

  test.describe('Scenario 5: Autocomplete Functionality', () => {
    test('should show autocomplete suggestions based on existing purchases', async () => {
      // Create some purchases with similar names
      const purchases = [
        { name: 'Premium Chicken Feed', amount: 200, quantity: 20, unit: 'kg' },
        { name: 'Premium Vitamins', amount: 150, quantity: 15, unit: 'ks' },
        { name: 'Standard Chicken Feed', amount: 180, quantity: 18, unit: 'kg' },
      ];

      for (const purchase of purchases) {
        await purchasesPage.openCreatePurchaseModal();
        await purchaseFormModal.createPurchase({
          type: 'Krmivo',
          ...purchase,
        });
        await purchaseFormModal.waitForClose();
        await purchasesPage.waitForPurchasesToLoad();
      }

      // Open create modal and type partial name
      await purchasesPage.openCreatePurchaseModal();
      await purchaseFormModal.nameInput.fill('Premium');

      // Wait for autocomplete suggestions
      await purchasesPage.page.waitForTimeout(500);

      // Autocomplete dropdown should be visible
      const autocompleteOptions = purchasesPage.page.locator('[role="option"]');
      const optionsCount = await autocompleteOptions.count();

      // Should show at least the matching purchases
      expect(optionsCount).toBeGreaterThan(0);

      // Cancel to close modal
      await purchaseFormModal.cancel();
    });

    test('should allow free text entry when no suggestions match', async () => {
      await purchasesPage.openCreatePurchaseModal();

      const uniqueName = 'Completely Unique Purchase Name ' + Date.now();
      await purchaseFormModal.nameInput.fill(uniqueName);

      // Should still be able to submit with unique name
      await purchaseFormModal.fillPartialForm({
        type: 'Krmivo',
        amount: 100,
        quantity: 10,
        unit: 'kg',
      });

      await expect(purchaseFormModal.submitButton).not.toBeDisabled();
      await purchaseFormModal.cancel();
    });

    test('should populate name field when selecting autocomplete suggestion', async () => {
      // Create a purchase first
      const existingPurchase = {
        type: 'Krmivo',
        name: 'Existing Test Purchase',
        amount: 100,
        quantity: 10,
        unit: 'kg',
      };

      await purchasesPage.openCreatePurchaseModal();
      await purchaseFormModal.createPurchase(existingPurchase);
      await purchaseFormModal.waitForClose();
      await purchasesPage.waitForPurchasesToLoad();

      // Open create modal and use autocomplete
      await purchasesPage.openCreatePurchaseModal();
      await purchaseFormModal.nameInput.fill('Existing');

      // Wait for suggestions
      await purchasesPage.page.waitForTimeout(500);

      // Select the suggestion (if available)
      const autocompleteOptions = purchasesPage.page.locator('[role="option"]');
      const optionsCount = await autocompleteOptions.count();

      if (optionsCount > 0) {
        await autocompleteOptions.first().click();

        // Name field should be populated
        const nameValue = await purchaseFormModal.nameInput.inputValue();
        expect(nameValue).toContain('Existing');
      }

      await purchaseFormModal.cancel();
    });
  });

  test.describe('Mobile Viewport Tests', () => {
    test('should work on mobile viewport (iPhone SE)', async ({ page }) => {
      // Set iPhone SE viewport (375x667)
      await page.setViewportSize({ width: 375, height: 667 });

      // Navigate to page
      await purchasesPage.goto();

      // Create a purchase on mobile
      await purchasesPage.openCreatePurchaseModal();
      await expect(purchaseFormModal.modalTitle).toBeVisible();

      const mobileData = {
        type: 'Krmivo',
        name: 'Mobile Purchase',
        amount: 100,
        quantity: 10,
        unit: 'kg',
      };

      await purchaseFormModal.createPurchase(mobileData);
      await purchaseFormModal.waitForClose();

      // Verify purchase appears
      await purchasesPage.waitForPurchasesToLoad();
      await expect(purchasesPage.getPurchaseCard(mobileData.name)).toBeVisible();

      // Test edit on mobile
      await purchasesPage.clickEditPurchase(mobileData.name);
      await expect(purchaseFormModal.modalTitle).toBeVisible();
      await purchaseFormModal.cancel();

      // Verify FAB button is accessible
      await expect(purchasesPage.addPurchaseButton).toBeVisible();

      // Check touch target size (minimum 44x44px)
      const fabBox = await purchasesPage.addPurchaseButton.boundingBox();
      expect(fabBox).not.toBeNull();
      if (fabBox) {
        expect(fabBox.width).toBeGreaterThanOrEqual(44);
        expect(fabBox.height).toBeGreaterThanOrEqual(44);
      }
    });

    test('should work on mobile viewport (Pixel 5)', async ({ page }) => {
      // Set Pixel 5 viewport (393x851)
      await page.setViewportSize({ width: 393, height: 851 });

      await purchasesPage.goto();

      // Test responsive layout
      await expect(purchasesPage.pageTitle).toBeVisible();
      await expect(purchasesPage.addPurchaseButton).toBeVisible();

      // Create purchase
      await purchasesPage.openCreatePurchaseModal();

      const purchaseData = {
        type: 'Krmivo',
        name: 'Pixel Purchase',
        amount: 150,
        quantity: 15,
        unit: 'kg',
      };

      await purchaseFormModal.createPurchase(purchaseData);
      await purchaseFormModal.waitForClose();

      await purchasesPage.waitForPurchasesToLoad();
      await expect(purchasesPage.getPurchaseCard(purchaseData.name)).toBeVisible();
    });
  });

  test.describe('Desktop Chrome Tests', () => {
    test('should work on desktop viewport', async ({ page }) => {
      // Set desktop viewport (1280x720)
      await page.setViewportSize({ width: 1280, height: 720 });

      await purchasesPage.goto();

      // Verify page elements
      await expect(purchasesPage.pageTitle).toBeVisible();
      await expect(purchasesPage.addPurchaseButton).toBeVisible();

      // Create purchase
      await purchasesPage.openCreatePurchaseModal();

      const desktopData = {
        type: 'Krmivo',
        name: 'Desktop Purchase',
        amount: 250,
        quantity: 25,
        unit: 'kg',
        notes: 'Created on desktop',
      };

      await purchaseFormModal.createPurchase(desktopData);
      await purchaseFormModal.waitForClose();

      await purchasesPage.waitForPurchasesToLoad();
      await expect(purchasesPage.getPurchaseCard(desktopData.name)).toBeVisible();

      // Test filters on desktop
      await purchasesPage.filterByType('Krmivo');
      await expect(purchasesPage.getPurchaseCard(desktopData.name)).toBeVisible();

      // Clear filter
      await purchasesPage.clearTypeFilter();
      await expect(purchasesPage.getPurchaseCard(desktopData.name)).toBeVisible();
    });
  });

  test.describe('Empty State', () => {
    test('should show empty state when no purchases exist', async () => {
      // Check if empty state is visible
      const isEmpty = await purchasesPage.isEmptyStateVisible();

      if (isEmpty) {
        await expect(purchasesPage.emptyStateMessage).toBeVisible();
        expect(await purchasesPage.getPurchaseCount()).toBe(0);
      }
    });

    test('should hide empty state after creating first purchase', async () => {
      // Create a purchase
      await purchasesPage.openCreatePurchaseModal();

      await purchaseFormModal.createPurchase({
        type: 'Krmivo',
        name: 'First Purchase',
        amount: 100,
        quantity: 10,
        unit: 'kg',
      });

      await purchaseFormModal.waitForClose();
      await purchasesPage.waitForPurchasesToLoad();

      // Empty state should not be visible
      await expect(purchasesPage.emptyStateMessage).not.toBeVisible();
      expect(await purchasesPage.getPurchaseCount()).toBeGreaterThan(0);
    });
  });
});
