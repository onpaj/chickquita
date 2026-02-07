import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Purchases Page & Routing
 * US-040: Frontend - Purchase Page & Routing
 *
 * Tests the complete user flow for the Purchases page:
 * - User navigates to /purchases
 * - User opens create modal
 * - User submits and sees new purchase in list
 * - User opens edit modal with pre-filled data
 * - Modal closes on submit
 */
test.describe('Purchases Page & Routing', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to purchases page
    await page.goto('/purchases');
    await page.waitForLoadState('networkidle');
  });

  test.describe('Page Navigation', () => {
    test('should navigate to /purchases route successfully', async ({ page }) => {
      // Verify URL
      expect(page.url()).toContain('/purchases');

      // Verify page title
      await expect(page.getByRole('heading', { name: /nákupy/i })).toBeVisible();
    });

    test('should navigate to purchases via bottom navigation', async ({ page }) => {
      // First go to a different page (e.g., dashboard)
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');

      // Find and click the purchases navigation button
      const purchasesNavButton = page.getByRole('button', { name: /nákupy/i });
      await expect(purchasesNavButton).toBeVisible();
      await purchasesNavButton.click();

      // Wait for navigation
      await page.waitForLoadState('networkidle');

      // Verify we're on the purchases page
      expect(page.url()).toContain('/purchases');
      await expect(page.getByRole('heading', { name: /nákupy/i })).toBeVisible();
    });

    test('should be protected with authentication', async ({ page, context }) => {
      // Clear authentication cookies/storage
      await context.clearCookies();
      await page.evaluate(() => localStorage.clear());

      // Try to access /purchases
      await page.goto('/purchases');
      await page.waitForLoadState('networkidle');

      // Should redirect to sign-in page
      expect(page.url()).toMatch(/sign-in|sign-up/);
    });
  });

  test.describe('Create Modal - Mobile FAB', () => {
    test('should show mobile FAB button', async ({ page }) => {
      // Check for FAB button
      const fabButton = page.getByLabel(/přidat nákup/i);
      await expect(fabButton).toBeVisible();
    });

    test('should open create modal when FAB is clicked', async ({ page }) => {
      // Click FAB button
      const fabButton = page.getByLabel(/přidat nákup/i);
      await fabButton.click();

      // Verify modal opened with create title
      await expect(page.getByText(/vytvořit nákup/i)).toBeVisible();

      // Verify form is present
      await expect(page.getByLabel(/název/i)).toBeVisible();
    });

    test('should close modal when cancel is clicked', async ({ page }) => {
      // Open modal
      const fabButton = page.getByLabel(/přidat nákup/i);
      await fabButton.click();
      await expect(page.getByText(/vytvořit nákup/i)).toBeVisible();

      // Click cancel button
      const cancelButton = page.getByRole('button', { name: /zrušit/i });
      await cancelButton.click();

      // Verify modal closed
      await expect(page.getByText(/vytvořit nákup/i)).not.toBeVisible();
    });
  });

  test.describe('Create Purchase Flow', () => {
    test('should create purchase and see it in list', async ({ page }) => {
      // Open create modal
      const fabButton = page.getByLabel(/přidat nákup/i);
      await fabButton.click();

      // Wait for modal
      await expect(page.getByText(/vytvořit nákup/i)).toBeVisible();

      // Fill in the form
      // Type selection
      const typeSelect = page.getByLabel(/typ nákupu/i);
      await typeSelect.click();
      await page.getByRole('option', { name: /krmivo/i }).first().click();

      // Name
      await page.getByLabel(/název/i).fill('E2E Test Feed Purchase');

      // Purchase date
      const today = new Date().toISOString().split('T')[0];
      await page.getByLabel(/datum nákupu/i).fill(today);

      // Amount - use NumericStepper
      const amountInput = page.getByLabel(/částka/i);
      await amountInput.fill('250.50');

      // Quantity - use NumericStepper
      const quantityInput = page.getByLabel(/množství/i);
      await quantityInput.fill('25');

      // Unit selection
      const unitSelect = page.getByLabel(/jednotka/i);
      await unitSelect.click();
      await page.getByRole('option', { name: /kg/i }).first().click();

      // Submit form
      const submitButton = page.getByRole('button', { name: /vytvořit/i });
      await expect(submitButton).toBeEnabled();
      await submitButton.click();

      // Wait for modal to close
      await expect(page.getByText(/vytvořit nákup/i)).not.toBeVisible({ timeout: 5000 });

      // Verify purchase appears in list
      await expect(page.getByText('E2E Test Feed Purchase')).toBeVisible({ timeout: 5000 });
    });

    test('should show validation errors for invalid data', async ({ page }) => {
      // Open create modal
      const fabButton = page.getByLabel(/přidat nákup/i);
      await fabButton.click();

      // Try to submit without filling required fields
      const submitButton = page.getByRole('button', { name: /vytvořit/i });

      // Submit button should be disabled when form is invalid
      await expect(submitButton).toBeDisabled();
    });
  });

  test.describe('Edit Purchase Flow', () => {
    test.beforeEach(async ({ page }) => {
      // Create a test purchase first
      const fabButton = page.getByLabel(/přidat nákup/i);
      await fabButton.click();

      // Fill in the form quickly
      const typeSelect = page.getByLabel(/typ nákupu/i);
      await typeSelect.click();
      await page.getByRole('option', { name: /krmivo/i }).first().click();

      await page.getByLabel(/název/i).fill('Purchase to Edit');

      const today = new Date().toISOString().split('T')[0];
      await page.getByLabel(/datum nákupu/i).fill(today);

      await page.getByLabel(/částka/i).fill('100');
      await page.getByLabel(/množství/i).fill('10');

      const unitSelect = page.getByLabel(/jednotka/i);
      await unitSelect.click();
      await page.getByRole('option', { name: /kg/i }).first().click();

      const submitButton = page.getByRole('button', { name: /vytvořit/i });
      await submitButton.click();

      // Wait for modal to close
      await expect(page.getByText(/vytvořit nákup/i)).not.toBeVisible({ timeout: 5000 });
    });

    test('should open edit modal with pre-filled data', async ({ page }) => {
      // Find the purchase in the list and click edit
      const purchaseCard = page.locator('[role="article"]', { hasText: 'Purchase to Edit' });
      await expect(purchaseCard).toBeVisible();

      const editButton = purchaseCard.getByLabel(/upravit/i);
      await editButton.click();

      // Verify edit modal opened
      await expect(page.getByText(/upravit nákup/i)).toBeVisible();

      // Verify form is pre-filled
      await expect(page.getByLabel(/název/i)).toHaveValue('Purchase to Edit');
      await expect(page.getByLabel(/částka/i)).toHaveValue('100');
      await expect(page.getByLabel(/množství/i)).toHaveValue('10');
    });

    test('should update purchase and close modal on submit', async ({ page }) => {
      // Find the purchase and click edit
      const purchaseCard = page.locator('[role="article"]', { hasText: 'Purchase to Edit' });
      const editButton = purchaseCard.getByLabel(/upravit/i);
      await editButton.click();

      // Wait for modal
      await expect(page.getByText(/upravit nákup/i)).toBeVisible();

      // Update the name
      const nameInput = page.getByLabel(/název/i);
      await nameInput.clear();
      await nameInput.fill('Updated Purchase Name');

      // Submit the form
      const submitButton = page.getByRole('button', { name: /uložit/i });
      await submitButton.click();

      // Wait for modal to close
      await expect(page.getByText(/upravit nákup/i)).not.toBeVisible({ timeout: 5000 });

      // Verify updated purchase appears in list
      await expect(page.getByText('Updated Purchase Name')).toBeVisible({ timeout: 5000 });
    });

    test('should close edit modal when cancel is clicked', async ({ page }) => {
      // Find the purchase and click edit
      const purchaseCard = page.locator('[role="article"]', { hasText: 'Purchase to Edit' });
      const editButton = purchaseCard.getByLabel(/upravit/i);
      await editButton.click();

      // Wait for modal
      await expect(page.getByText(/upravit nákup/i)).toBeVisible();

      // Click cancel
      const cancelButton = page.getByRole('button', { name: /zrušit/i });
      await cancelButton.click();

      // Verify modal closed
      await expect(page.getByText(/upravit nákup/i)).not.toBeVisible();
    });
  });

  test.describe('Responsive Layout', () => {
    test('should display properly on mobile viewport', async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 });

      // Navigate to page
      await page.goto('/purchases');
      await page.waitForLoadState('networkidle');

      // Verify page title is visible
      await expect(page.getByRole('heading', { name: /nákupy/i })).toBeVisible();

      // Verify FAB button is visible
      const fabButton = page.getByLabel(/přidat nákup/i);
      await expect(fabButton).toBeVisible();

      // Verify bottom navigation is visible
      const bottomNav = page.locator('[class*="MuiBottomNavigation"]');
      await expect(bottomNav).toBeVisible();
    });

    test('should display desktop button on larger screens', async ({ page }) => {
      // Set desktop viewport
      await page.setViewportSize({ width: 1280, height: 720 });

      // Navigate to page
      await page.goto('/purchases');
      await page.waitForLoadState('networkidle');

      // Check for desktop create button
      const createButton = page.getByRole('button', { name: /přidat nákup/i });
      await expect(createButton).toBeVisible();
    });
  });

  test.describe('PurchaseList Integration', () => {
    test('should render PurchaseList component', async ({ page }) => {
      // Verify filters are present (part of PurchaseList)
      await expect(page.getByText(/filtry/i)).toBeVisible();
    });

    test('should interact with PurchaseList filters', async ({ page }) => {
      // Test date range filter
      const fromDateInput = page.getByLabel(/od data/i);
      await expect(fromDateInput).toBeVisible();

      const toDateInput = page.getByLabel(/do data/i);
      await expect(toDateInput).toBeVisible();

      // Test type filter
      const typeFilter = page.getByLabel(/typ/i);
      await expect(typeFilter).toBeVisible();
    });
  });

  test.describe('Bottom Navigation Integration', () => {
    test('should highlight purchases tab in bottom navigation', async ({ page }) => {
      // Verify bottom navigation exists
      const bottomNav = page.locator('[class*="MuiBottomNavigation"]');
      await expect(bottomNav).toBeVisible();

      // Find purchases tab
      const purchasesTab = page.getByRole('button', { name: /nákupy/i });
      await expect(purchasesTab).toBeVisible();

      // Verify it's selected (has Mui-selected class)
      const isSelected = await purchasesTab.evaluate((el) =>
        el.classList.contains('Mui-selected')
      );
      expect(isSelected).toBeTruthy();
    });
  });
});
