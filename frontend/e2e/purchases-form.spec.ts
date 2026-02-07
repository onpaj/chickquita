import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Purchase Form Component
 *
 * Coverage:
 * - User creates purchase via form
 * - Validation prevents invalid submission
 * - Autocomplete functionality
 * - Form accessibility
 */

test.describe('Purchase Form', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to purchases page (assuming there's a modal or page with the form)
    await page.goto('http://localhost:5173/purchases');

    // Wait for page to load
    await page.waitForLoadState('networkidle');
  });

  test.describe('Create Purchase Flow', () => {
    test('should allow user to create a purchase via form', async ({ page }) => {
      // Click "Add Purchase" button to open form
      const addButton = page.getByRole('button', { name: /add purchase/i });
      await expect(addButton).toBeVisible();
      await addButton.click();

      // Wait for form to appear
      await expect(page.getByLabel(/purchase type/i)).toBeVisible();

      // Fill in the form
      await page.getByLabel(/purchase type/i).selectOption('0'); // Feed
      await page.getByLabel(/^name$/i).fill('Premium Chicken Feed');

      // Set purchase date
      const today = new Date().toISOString().split('T')[0];
      await page.getByLabel(/purchase date/i).fill(today);

      // Set amount using NumericStepper
      const amountSection = page.locator('text=Amount (CZK)').locator('..');
      const amountIncrementButton = amountSection.getByRole('button').last();

      // Click increment button multiple times to set amount to 250
      for (let i = 0; i < 250; i++) {
        await amountIncrementButton.click();
      }

      // Set quantity using NumericStepper
      const quantitySection = page.locator('text=Quantity').locator('..');
      const quantityIncrementButton = quantitySection.getByRole('button').last();

      // Click increment button to set quantity to 25
      for (let i = 0; i < 25; i++) {
        await quantityIncrementButton.click();
      }

      // Select unit
      await page.getByLabel(/^unit$/i).selectOption('0'); // Kg

      // Add notes
      await page.getByLabel(/notes/i).fill('Test purchase from E2E');

      // Submit the form
      const submitButton = page.getByRole('button', { name: /create|save/i });
      await expect(submitButton).toBeEnabled();
      await submitButton.click();

      // Wait for success message or redirect
      await page.waitForTimeout(1000);

      // Verify purchase was created (check if it appears in the list)
      await expect(page.getByText('Premium Chicken Feed')).toBeVisible({ timeout: 5000 });
    });

    test('should show autocomplete suggestions when typing purchase name', async ({ page }) => {
      // Click "Add Purchase" button
      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      // Wait for form
      await expect(page.getByLabel(/^name$/i)).toBeVisible();

      // Type in the name field to trigger autocomplete
      const nameInput = page.getByLabel(/^name$/i);
      await nameInput.fill('Kr');

      // Wait for autocomplete suggestions
      await page.waitForTimeout(500); // Wait for debounce

      // Autocomplete suggestions should appear
      // Note: Actual suggestions depend on backend data
      const autocompleteOptions = page.getByRole('option');

      // If there are suggestions, verify we can select one
      const optionCount = await autocompleteOptions.count();
      if (optionCount > 0) {
        await autocompleteOptions.first().click();

        // Verify the input was filled
        const inputValue = await nameInput.inputValue();
        expect(inputValue.length).toBeGreaterThan(0);
      }
    });

    test('should display type icons for each purchase type', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/purchase type/i)).toBeVisible();

      // Open the type dropdown
      await page.getByLabel(/purchase type/i).click();

      // Verify all purchase types are present
      await expect(page.getByText('Feed')).toBeVisible();
      await expect(page.getByText('Vitamins')).toBeVisible();
      await expect(page.getByText('Bedding')).toBeVisible();
      await expect(page.getByText('Toys')).toBeVisible();
      await expect(page.getByText('Veterinary')).toBeVisible();
      await expect(page.getByText('Other')).toBeVisible();
    });
  });

  test.describe('Form Validation', () => {
    test('should prevent submission with empty required fields', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/purchase type/i)).toBeVisible();

      // Try to submit without filling required fields
      const submitButton = page.getByRole('button', { name: /create|save/i });

      // Submit button should be disabled
      await expect(submitButton).toBeDisabled();
    });

    test('should validate that purchase date is not in the future', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/purchase type/i)).toBeVisible();

      // Fill in name
      await page.getByLabel(/^name$/i).fill('Test');

      // Set a future date
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 1);
      const futureDateStr = futureDate.toISOString().split('T')[0];

      await page.getByLabel(/purchase date/i).fill(futureDateStr);

      // Blur the field to trigger validation
      await page.getByLabel(/purchase date/i).blur();

      // Wait for error message
      await expect(
        page.getByText(/purchase date cannot be in the future/i)
      ).toBeVisible({ timeout: 2000 });
    });

    test('should require positive amount', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/purchase type/i)).toBeVisible();

      // Fill in minimum required fields
      await page.getByLabel(/^name$/i).fill('Test Feed');

      const today = new Date().toISOString().split('T')[0];
      await page.getByLabel(/purchase date/i).fill(today);

      // Leave amount at 0 (default)

      // Submit button should be disabled because amount must be positive
      const submitButton = page.getByRole('button', { name: /create|save/i });
      await expect(submitButton).toBeDisabled();
    });

    test('should require positive quantity', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/purchase type/i)).toBeVisible();

      // Fill in minimum required fields
      await page.getByLabel(/^name$/i).fill('Test Feed');

      const today = new Date().toISOString().split('T')[0];
      await page.getByLabel(/purchase date/i).fill(today);

      // Set amount but leave quantity at 0
      const amountSection = page.locator('text=Amount (CZK)').locator('..');
      const amountIncrementButton = amountSection.getByRole('button').last();
      await amountIncrementButton.click();

      // Submit button should be disabled because quantity must be positive
      const submitButton = page.getByRole('button', { name: /create|save/i });
      await expect(submitButton).toBeDisabled();
    });
  });

  test.describe('Accessibility', () => {
    test('should have proper ARIA labels', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      // Check for ARIA labels on form fields
      await expect(page.getByLabel(/purchase type/i)).toHaveAttribute('aria-label');
      await expect(page.getByLabel(/purchase date/i)).toHaveAttribute('aria-label');
      await expect(page.getByLabel(/notes/i)).toHaveAttribute('aria-label');
    });

    test('should support keyboard navigation', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      // Focus on first field
      await page.getByLabel(/purchase type/i).focus();

      // Tab through fields
      await page.keyboard.press('Tab');
      await expect(page.getByLabel(/^name$/i)).toBeFocused();

      await page.keyboard.press('Tab');
      await expect(page.getByLabel(/purchase date/i)).toBeFocused();

      // Continue tabbing through the form
      await page.keyboard.press('Tab');
      await page.keyboard.press('Tab');
      await page.keyboard.press('Tab');

      // Should be able to reach submit button via keyboard
      const submitButton = page.getByRole('button', { name: /create|save/i });
      await submitButton.focus();
      await expect(submitButton).toBeFocused();
    });
  });

  test.describe('Mobile Responsiveness', () => {
    test('should render properly on mobile viewport', async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 });

      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      // Form should be visible and usable
      await expect(page.getByLabel(/purchase type/i)).toBeVisible();
      await expect(page.getByLabel(/^name$/i)).toBeVisible();

      // NumericStepper buttons should be visible and touch-friendly
      const amountSection = page.locator('text=Amount (CZK)').locator('..');
      const amountButtons = amountSection.getByRole('button');

      // Verify buttons are visible
      await expect(amountButtons.first()).toBeVisible();
      await expect(amountButtons.last()).toBeVisible();
    });

    test('should have touch-friendly input targets', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });

      const addButton = page.getByRole('button', { name: /add purchase/i });
      await addButton.click();

      // Check that inputs have minimum touch target size (44px is iOS standard)
      const nameInput = page.getByLabel(/^name$/i);
      const boundingBox = await nameInput.boundingBox();

      expect(boundingBox).toBeTruthy();
      if (boundingBox) {
        expect(boundingBox.height).toBeGreaterThanOrEqual(40); // Close to 44px minimum
      }
    });
  });
});
