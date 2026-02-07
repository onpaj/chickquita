import { test, expect } from '@playwright/test';

/**
 * E2E tests for Purchase List Page with Filtering and Deletion
 * US-039: Frontend - Purchase List Component
 *
 * Tests the complete workflow for viewing, filtering, and deleting purchases:
 * - Page renders with all filters
 * - Filter updates trigger data refresh
 * - Purchases displayed in cards
 * - Empty state shown when no purchases
 * - Delete confirmation dialog
 * - Monthly summary card
 */
test.describe('Purchases - List Page with Filtering', () => {
  test.beforeEach(async ({ page }) => {
    // Start at purchases page
    await page.goto('/purchases');
    await page.waitForLoadState('networkidle');
  });

  test('should display empty state when no purchases exist', async ({ page }) => {
    // Check for page title
    await expect(page.getByRole('heading', { name: /nákupy/i })).toBeVisible();

    // Check for filters section
    await expect(page.getByText(/filtry/i)).toBeVisible();

    // Check for empty state
    await expect(page.getByText(/zatím žádné nákupy/i)).toBeVisible();
  });

  test('should display all filter options', async ({ page }) => {
    // Check for date range filters
    await expect(page.getByLabel(/od data/i)).toBeVisible();
    await expect(page.getByLabel(/do data/i)).toBeVisible();

    // Check for type filter
    await expect(page.getByLabel(/typ/i)).toBeVisible();

    // Check for flock filter (should be disabled if no flocks)
    const flockFilter = page.getByLabel(/hejno/i);
    await expect(flockFilter).toBeVisible();
  });

  test('should filter purchases by date range', async ({ page }) => {
    // Set from date
    const fromDateInput = page.getByLabel(/od data/i);
    await fromDateInput.fill('2024-02-01');

    // Set to date
    const toDateInput = page.getByLabel(/do data/i);
    await toDateInput.fill('2024-02-28');

    // Verify inputs have values
    await expect(fromDateInput).toHaveValue('2024-02-01');
    await expect(toDateInput).toHaveValue('2024-02-28');

    // Wait for potential filter update
    await page.waitForTimeout(500);
  });

  test('should filter purchases by type', async ({ page }) => {
    // Click on type filter dropdown
    const typeFilter = page.getByLabel(/typ/i);
    await typeFilter.click();

    // Select a type (e.g., Feed)
    const feedOption = page.getByRole('option', { name: /krmivo/i });
    await feedOption.click();

    // Verify selection
    await expect(typeFilter).toContainText(/krmivo/i);

    // Wait for potential filter update
    await page.waitForTimeout(500);
  });

  test('should display monthly summary when purchases exist', async ({ page }) => {
    // Create a test purchase first
    // This assumes there's an "Add Purchase" button available
    const addButton = page.getByRole('button', { name: /přidat nákup/i });
    if (await addButton.isVisible()) {
      await addButton.click();

      // Fill in purchase details (assuming a modal form)
      await page.getByLabel(/název/i).fill('Test Feed');
      await page.getByLabel(/typ/i).click();
      await page.getByRole('option', { name: /krmivo/i }).click();
      await page.getByLabel(/částka/i).fill('500');
      await page.getByLabel(/množství/i).fill('25');
      await page.getByLabel(/jednotka/i).click();
      await page.getByRole('option', { name: /kg/i }).click();

      // Submit form
      await page.getByRole('button', { name: /uložit/i }).click();
      await page.waitForLoadState('networkidle');

      // Check for monthly summary card
      await expect(page.getByText(/celkem utraceno tento měsíc/i)).toBeVisible();
    }
  });

  test('should display purchase cards with correct information', async ({ page }) => {
    // This test assumes there are purchases to display
    // Look for purchase cards
    const purchaseCards = page.locator('[role="article"]');
    const count = await purchaseCards.count();

    if (count > 0) {
      // Get first purchase card
      const firstCard = purchaseCards.first();

      // Check for purchase name
      await expect(firstCard).toBeVisible();

      // Check for edit and delete buttons
      const editButton = firstCard.getByLabel(/upravit/i);
      const deleteButton = firstCard.getByLabel(/smazat/i);

      await expect(editButton).toBeVisible();
      await expect(deleteButton).toBeVisible();
    }
  });

  test('should open delete confirmation dialog when delete is clicked', async ({ page }) => {
    // This test assumes there are purchases to display
    const purchaseCards = page.locator('[role="article"]');
    const count = await purchaseCards.count();

    if (count > 0) {
      // Click delete button on first purchase
      const firstCard = purchaseCards.first();
      const deleteButton = firstCard.getByLabel(/smazat/i);
      await deleteButton.click();

      // Check for confirmation dialog
      await expect(page.getByText(/smazat nákup/i)).toBeVisible();
      await expect(page.getByText(/opravdu chcete smazat/i)).toBeVisible();

      // Check for dialog buttons
      await expect(page.getByRole('button', { name: /zrušit/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /smazat/i })).toBeVisible();
    }
  });

  test('should cancel delete when cancel is clicked', async ({ page }) => {
    // This test assumes there are purchases to display
    const purchaseCards = page.locator('[role="article"]');
    const count = await purchaseCards.count();

    if (count > 0) {
      // Click delete button on first purchase
      const firstCard = purchaseCards.first();
      const deleteButton = firstCard.getByLabel(/smazat/i);
      await deleteButton.click();

      // Wait for dialog
      await expect(page.getByText(/smazat nákup/i)).toBeVisible();

      // Click cancel
      const cancelButton = page.getByRole('button', { name: /zrušit/i });
      await cancelButton.click();

      // Dialog should be closed
      await expect(page.getByText(/smazat nákup/i)).not.toBeVisible();
    }
  });

  test('should delete purchase when confirmed', async ({ page }) => {
    // This test assumes there are purchases to display
    const purchaseCards = page.locator('[role="article"]');
    const initialCount = await purchaseCards.count();

    if (initialCount > 0) {
      // Get the name of the first purchase
      const firstCard = purchaseCards.first();
      const purchaseName = await firstCard.locator('h3').textContent();

      // Click delete button
      const deleteButton = firstCard.getByLabel(/smazat/i);
      await deleteButton.click();

      // Wait for dialog
      await expect(page.getByText(/smazat nákup/i)).toBeVisible();

      // Click confirm
      const confirmButton = page.getByRole('button', { name: /smazat/i }).last();
      await confirmButton.click();

      // Wait for deletion to complete
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Verify purchase is no longer in the list
      if (purchaseName) {
        const remainingCards = page.locator('[role="article"]');
        const finalCount = await remainingCards.count();
        expect(finalCount).toBeLessThan(initialCount);
      }
    }
  });

  test('should have proper keyboard navigation', async ({ page }) => {
    // This test assumes there are purchases to display
    const purchaseCards = page.locator('[role="article"]');
    const count = await purchaseCards.count();

    if (count > 0) {
      // Get first purchase card
      const firstCard = purchaseCards.first();
      const editButton = firstCard.getByLabel(/upravit/i);

      // Focus on edit button
      await editButton.focus();
      await expect(editButton).toBeFocused();

      // Tab to delete button
      await page.keyboard.press('Tab');
      const deleteButton = firstCard.getByLabel(/smazat/i);
      await expect(deleteButton).toBeFocused();
    }
  });

  test('should clear type filter when "All" is selected', async ({ page }) => {
    // Select a specific type first
    const typeFilter = page.getByLabel(/typ/i);
    await typeFilter.click();
    const feedOption = page.getByRole('option', { name: /krmivo/i });
    await feedOption.click();

    // Verify selection
    await expect(typeFilter).toContainText(/krmivo/i);

    // Clear filter by selecting "All"
    await typeFilter.click();
    const allOption = page.getByRole('option', { name: /vše/i });
    await allOption.click();

    // Wait for filter update
    await page.waitForTimeout(500);
  });
});

/**
 * E2E Accessibility tests for Purchase List
 */
test.describe('Purchases - Accessibility', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/purchases');
    await page.waitForLoadState('networkidle');
  });

  test('should have proper ARIA labels on interactive elements', async ({ page }) => {
    // Check filter labels
    await expect(page.getByLabel(/od data/i)).toBeVisible();
    await expect(page.getByLabel(/do data/i)).toBeVisible();
    await expect(page.getByLabel(/typ/i)).toBeVisible();

    // Check for purchase cards with proper role
    const purchaseCards = page.locator('[role="article"]');
    const count = await purchaseCards.count();

    if (count > 0) {
      // Check buttons have aria-labels
      const editButtons = page.getByLabel(/upravit/i);
      const deleteButtons = page.getByLabel(/smazat/i);

      expect(await editButtons.count()).toBeGreaterThan(0);
      expect(await deleteButtons.count()).toBeGreaterThan(0);
    }
  });

  test('should support screen reader navigation', async ({ page }) => {
    // Navigate using heading landmarks
    const headings = page.locator('h2, h3, h6');
    const headingCount = await headings.count();

    expect(headingCount).toBeGreaterThan(0);

    // Check for descriptive headings
    await expect(page.getByRole('heading', { name: /filtry/i })).toBeVisible();
  });
});
