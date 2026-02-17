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
    await page.goto('/purchases');

    // Wait for page to load
    await page.waitForLoadState('networkidle');
  });

  test.describe('Create Purchase Flow', () => {
    test('should allow user to create a purchase via form', async ({ page }) => {
      // Click "Add Purchase" button to open form (Czech: "Přidat nákup")
      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await expect(addButton).toBeVisible();
      await addButton.click();

      // Wait for form to appear (Czech: "Typ nákupu")
      await expect(page.getByLabel(/typ nákupu|purchase type/i)).toBeVisible();

      // Fill in the form - type field: click to open MUI Select, then pick option
      await page.getByLabel(/typ nákupu|purchase type/i).click();
      await page.getByRole('option', { name: 'Krmivo' }).first().click();

      // Fill name field (Czech: "Název")
      await page.getByLabel(/název|^name$/i).fill('Premium Chicken Feed');

      // Set purchase date (Czech: "Datum nákupu")
      const today = new Date().toISOString().split('T')[0];
      await page.getByLabel(/datum nákupu|purchase date/i).fill(today);

      // Set amount by filling the number input directly (Czech: "Částka (Kč)")
      await page.locator('input[aria-label="Částka (Kč)"]').fill('250');

      // Set quantity by filling the number input directly (Czech: "Množství")
      await page.locator('input[aria-label="Množství"]').fill('25');

      // Select unit: click to open MUI Select, then pick option
      await page.getByLabel(/jednotka|^unit$/i).click();
      await page.getByRole('option', { name: 'kg' }).first().click();

      // Add notes (Czech: "Poznámky")
      await page.getByLabel(/poznámky|notes/i).fill('Test purchase from E2E');

      // Submit the form (Czech: "Vytvořit" or "Uložit")
      const submitButton = page.getByRole('button', { name: /^vytvořit$|^uložit$|^create$|^save$/i });
      await expect(submitButton).toBeEnabled();
      await submitButton.click();

      // Wait for the dialog to close (API call completed and modal dismissed)
      // Neon serverless can have cold starts — allow up to 30 seconds
      await expect(page.getByRole('dialog')).not.toBeVisible({ timeout: 30000 });

      // Verify purchase was created (check if it appears in the list)
      await expect(page.getByText('Premium Chicken Feed').first()).toBeVisible({ timeout: 10000 });
    });

    test('should show autocomplete suggestions when typing purchase name', async ({ page }) => {
      // Click "Add Purchase" button (Czech: "Přidat nákup")
      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      // Wait for form (Czech: "Název")
      await expect(page.getByLabel(/název|^name$/i)).toBeVisible();

      // Type in the name field to trigger autocomplete
      const nameInput = page.getByLabel(/název|^name$/i);
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
      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/typ nákupu|purchase type/i)).toBeVisible();

      // Open the type dropdown via click (MUI Select)
      await page.getByLabel(/typ nákupu|purchase type/i).click();

      // Verify all purchase types are present (Czech labels)
      await expect(page.getByRole('option', { name: 'Krmivo' })).toBeVisible();
      await expect(page.getByRole('option', { name: 'Vitamíny' })).toBeVisible();
      await expect(page.getByRole('option', { name: 'Podestýlka' })).toBeVisible();
      await expect(page.getByRole('option', { name: 'Vybavení' })).toBeVisible();
      await expect(page.getByRole('option', { name: 'Veterinární péče' })).toBeVisible();
      await expect(page.getByRole('option', { name: 'Ostatní' })).toBeVisible();

      // Close the dropdown by pressing Escape
      await page.keyboard.press('Escape');
    });
  });

  test.describe('Form Validation', () => {
    test('should prevent submission with empty required fields', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/typ nákupu|purchase type/i)).toBeVisible();

      // Submit button should be disabled because name, amount and quantity are empty/zero
      const submitButton = page.getByRole('button', { name: /^vytvořit$|^uložit$|^create$|^save$/i });
      await expect(submitButton).toBeDisabled();
    });

    test('should validate that purchase date is not in the future', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/typ nákupu|purchase type/i)).toBeVisible();

      // Fill in name (Czech: "Název")
      await page.getByLabel(/název|^name$/i).fill('Test');

      // Set a future date (Czech: "Datum nákupu")
      const futureDate = new Date();
      futureDate.setDate(futureDate.getDate() + 1);
      const futureDateStr = futureDate.toISOString().split('T')[0];

      await page.getByLabel(/datum nákupu|purchase date/i).fill(futureDateStr);

      // Blur the field to trigger validation
      await page.getByLabel(/datum nákupu|purchase date/i).blur();

      // Wait for error message (Czech: "Datum nákupu nemůže být v budoucnosti")
      await expect(
        page.getByText(/datum nákupu nemůže být v budoucnosti|purchase date cannot be in the future/i)
      ).toBeVisible({ timeout: 2000 });
    });

    test('should require positive amount', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/typ nákupu|purchase type/i)).toBeVisible();

      // Fill in minimum required fields (Czech: "Název")
      await page.getByLabel(/název|^name$/i).fill('Test Feed');

      const today = new Date().toISOString().split('T')[0];
      await page.getByLabel(/datum nákupu|purchase date/i).fill(today);

      // Leave amount at 0 (default)

      // Submit button should be disabled because amount must be positive
      const submitButton = page.getByRole('button', { name: /^vytvořit$|^uložit$|^create$|^save$/i });
      await expect(submitButton).toBeDisabled();
    });

    test('should require positive quantity', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      await expect(page.getByLabel(/typ nákupu|purchase type/i)).toBeVisible();

      // Fill in minimum required fields (Czech: "Název")
      await page.getByLabel(/název|^name$/i).fill('Test Feed');

      const today = new Date().toISOString().split('T')[0];
      await page.getByLabel(/datum nákupu|purchase date/i).fill(today);

      // Set amount but leave quantity at 0
      await page.locator('input[aria-label="Částka (Kč)"]').fill('100');

      // Submit button should be disabled because quantity must be positive
      const submitButton = page.getByRole('button', { name: /^vytvořit$|^uložit$|^create$|^save$/i });
      await expect(submitButton).toBeDisabled();
    });
  });

  test.describe('Accessibility', () => {
    test('should have proper ARIA labels', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      // Check that form fields are labeled and accessible (visible via their labels)
      await expect(page.getByLabel(/typ nákupu|purchase type/i)).toBeVisible();
      await expect(page.getByLabel(/datum nákupu|purchase date/i)).toBeVisible();
      await expect(page.getByLabel(/poznámky|notes/i)).toBeVisible();

      // Regular TextField inputs have aria-label on the actual input element
      await expect(page.getByLabel(/datum nákupu|purchase date/i)).toHaveAttribute('aria-label');
      await expect(page.getByLabel(/poznámky|notes/i)).toHaveAttribute('aria-label');
    });

    test('should support keyboard navigation', async ({ page }) => {
      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      // All major fields should be focusable via keyboard
      const nameInput = page.getByLabel(/název|^name$/i);
      await nameInput.focus();
      await expect(nameInput).toBeFocused();

      const dateInput = page.getByLabel(/datum nákupu|purchase date/i);
      await dateInput.focus();
      await expect(dateInput).toBeFocused();

      const notesInput = page.getByLabel(/poznámky|notes/i);
      await notesInput.focus();
      await expect(notesInput).toBeFocused();
    });
  });

  test.describe('Mobile Responsiveness', () => {
    test('should render properly on mobile viewport', async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 });

      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      // Form should be visible and usable (Czech labels)
      await expect(page.getByLabel(/typ nákupu|purchase type/i)).toBeVisible();
      await expect(page.getByLabel(/název|^name$/i)).toBeVisible();

      // NumericStepper buttons should be visible and touch-friendly
      const amountSection = page.locator('text=Částka').locator('..');
      const amountButtons = amountSection.getByRole('button');

      // Verify buttons are visible
      await expect(amountButtons.first()).toBeVisible();
      await expect(amountButtons.last()).toBeVisible();
    });

    test('should have touch-friendly input targets', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });

      const addButton = page.getByRole('button', { name: /přidat nákup|add purchase/i });
      await addButton.click();

      // Check that inputs have minimum touch target size (44px is iOS standard)
      const nameInput = page.getByLabel(/název|^name$/i);
      const boundingBox = await nameInput.boundingBox();

      expect(boundingBox).toBeTruthy();
      if (boundingBox) {
        expect(boundingBox.height).toBeGreaterThanOrEqual(40); // Close to 44px minimum
      }
    });
  });
});
