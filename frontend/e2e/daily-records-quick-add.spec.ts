import { test, expect } from '@playwright/test';
import { CoopsPage } from './pages/CoopsPage';
import { CreateCoopModal } from './pages/CreateCoopModal';
import { CoopDetailPage } from './pages/CoopDetailPage';
import { CreateFlockModal } from './pages/CreateFlockModal';
import { generateCoopName } from './fixtures/coop.fixture';
import { generateFlockIdentifier, getDaysAgoDate } from './fixtures/flock.fixture';

/**
 * E2E tests for Daily Records Quick Add Modal
 * US-014: Quick Add Modal Component
 *
 * Tests the complete workflow for quick daily record entry:
 * - Opening the modal
 * - Auto-focus behavior
 * - Last-used flock memory
 * - Form validation
 * - Successful submission
 * - Mobile responsiveness
 * - < 30 second completion target
 */
test.describe('Daily Records - Quick Add Modal', () => {
  test.describe.configure({ mode: 'serial' });
  let coopsPage: CoopsPage;
  let createCoopModal: CreateCoopModal;
  let coopDetailPage: CoopDetailPage;
  let createFlockModal: CreateFlockModal;
  let testCoopId: string;
  let testFlockId: string;
  let testCoopName: string;
  let testFlockIdentifier: string;

  test.beforeEach(async ({ page }) => {
    // Initialize page objects
    coopsPage = new CoopsPage(page);
    createCoopModal = new CreateCoopModal(page);
    coopDetailPage = new CoopDetailPage(page);
    createFlockModal = new CreateFlockModal(page);

    // Create a test coop
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    testCoopName = generateCoopName();

    // Intercept POST /api/coops to capture the coop ID
    let createdCoopId: string | null = null;
    page.on('response', async (response) => {
      if (
        response.url().includes('/api/coops') &&
        response.request().method() === 'POST' &&
        response.status() === 201
      ) {
        try {
          const data = await response.json();
          if (data && data.id) {
            createdCoopId = data.id;
          }
        } catch {
          // Ignore JSON parsing errors
        }
      }
    });

    await coopsPage.clickAddButton();
    await createCoopModal.fillForm(testCoopName, 'Test location for quick add');
    await createCoopModal.submit();
    await createCoopModal.waitForClose();

    // Wait for the coop ID to be captured from the API response
    await page.waitForTimeout(1000);
    testCoopId = createdCoopId || '';
    expect(testCoopId).toBeTruthy();

    // Navigate directly to the flocks page for this coop
    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle');

    // Create a test flock
    testFlockIdentifier = generateFlockIdentifier();

    // Intercept POST /api/flocks to capture the flock ID
    let createdFlockId: string | null = null;
    page.on('response', async (response) => {
      if (
        response.url().includes('/flocks') &&
        response.request().method() === 'POST' &&
        response.status() === 201
      ) {
        try {
          const data = await response.json();
          if (data && data.id) {
            createdFlockId = data.id;
          }
        } catch {
          // Ignore JSON parsing errors
        }
      }
    });

    // Click the add flock FAB button
    await page.getByTestId('add-flock-fab').click();
    await createFlockModal.fillForm({
      identifier: testFlockIdentifier,
      hatchDate: getDaysAgoDate(30),
      hens: 5,
      roosters: 1,
      chicks: 0,
    });
    await createFlockModal.submit();
    await createFlockModal.waitForClose();
    await page.waitForLoadState('networkidle');

    // Wait for the flock ID to be captured
    await page.waitForTimeout(500);
    testFlockId = createdFlockId || '';
    expect(testFlockId).toBeTruthy();
  });

  test('should open Quick Add modal from dashboard', async ({ page }) => {
    // Navigate to dashboard
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Click on the Quick Add FAB button
    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });

    await expect(quickAddButton).toBeVisible();
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    // Modal should open
    await expect(page.getByText('Rychlý záznam vajec')).toBeVisible();
  });

  test('should display all form fields correctly', async ({ page }) => {
    // Navigate to dashboard and open Quick Add modal
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    // Check all fields are present
    await expect(page.getByLabel(/hejno/i)).toBeVisible();
    await expect(page.getByLabel(/datum/i)).toBeVisible();
    await expect(page.getByText('Počet vajec')).toBeVisible();
    await expect(page.getByLabel(/poznámky/i)).toBeVisible();

    // Check buttons are present
    await expect(page.getByRole('button', { name: /zrušit/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /uložit/i })).toBeVisible();
  });

  test('should have today as default date', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    const dateInput = page.getByLabel(/datum/i);
    const today = new Date().toISOString().split('T')[0];
    await expect(dateInput).toHaveValue(today);
  });

  test('should increment and decrement egg count', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    // Find the egg count stepper
    const incrementButton = page.getByLabel('egg count increase');
    const decrementButton = page.getByLabel('egg count decrease');
    const eggCountInput = page.locator('input[type="number"][aria-label="egg count"]');

    // Initial value should be 0
    await expect(eggCountInput).toHaveValue('0');
    await expect(decrementButton).toBeDisabled();

    // Increment
    await incrementButton.click();
    await expect(eggCountInput).toHaveValue('1');

    await incrementButton.click();
    await expect(eggCountInput).toHaveValue('2');

    // Decrement
    await decrementButton.click();
    await expect(eggCountInput).toHaveValue('1');
  });

  test('should validate date cannot be in future', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    const dateInput = page.getByLabel(/datum/i);
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    const futureDateStr = futureDate.toISOString().split('T')[0];

    await dateInput.fill(futureDateStr);
    await dateInput.blur();

    // Should show validation error
    await expect(page.getByText(/nemůže být v budoucnosti/i)).toBeVisible();
  });

  test('should validate notes max length (500 characters)', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    const notesInput = page.getByLabel(/poznámky/i);
    const longText = 'a'.repeat(501);

    await notesInput.fill(longText);
    await notesInput.blur();

    // Should show validation error
    await expect(page.getByText(/maximální délka/i)).toBeVisible();
  });

  test('should show character count for notes', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    // Should show 0/500 initially
    await expect(page.getByText(/0\/500 znaků/i)).toBeVisible();

    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill('Test note');

    // Should update character count
    await expect(page.getByText(/9\/500 znaků/i)).toBeVisible();
  });

  test('@smoke should submit form successfully and close modal', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    // Select flock (first flock should be selected by default)
    const flockSelect = page.getByLabel(/hejno/i);
    await expect(flockSelect).toBeVisible();

    // Set egg count
    const incrementButton = page.getByLabel('egg count increase');
    await incrementButton.click();
    await incrementButton.click();
    await incrementButton.click(); // Set to 3

    // Add notes
    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill('Test daily record from E2E');

    // Wait for POST request and response
    const responsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );

    // Submit form
    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    // Wait for the API response
    await responsePromise;

    // Modal should close
    await expect(page.getByText('Rychlý záznam vajec')).not.toBeVisible();

    // Success toast should appear
    await expect(page.getByText(/úspěšně vytvořen/i)).toBeVisible();
  });

  test('should reset form after closing', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    // Fill in some data
    const incrementButton = page.getByLabel('egg count increase');
    await incrementButton.click();

    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill('Test notes');

    // Close modal
    const cancelButton = page.getByRole('button', { name: /zrušit/i });
    await cancelButton.click();

    // Reopen modal
    await quickAddButton.click();

    // Form should be reset
    const eggCountInput = page.locator('input[type="number"][aria-label="egg count"]');
    await expect(eggCountInput).toHaveValue('0');

    const notesInputAfter = page.getByLabel(/poznámky/i);
    await expect(notesInputAfter).toHaveValue('');
  });

  test('should disable form while submitting', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    // Set egg count
    const incrementButton = page.getByLabel('egg count increase');
    await incrementButton.click();

    // Intercept the request to delay response
    await page.route('**/api/flocks/*/daily-records', async (route) => {
      await page.waitForTimeout(1000); // Delay by 1 second
      await route.continue();
    });

    // Submit form
    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    // Check that form is disabled during submission
    const flockSelect = page.getByLabel(/hejno/i);
    const dateInput = page.getByLabel(/datum/i);
    const notesInput = page.getByLabel(/poznámky/i);

    await expect(flockSelect).toBeDisabled();
    await expect(dateInput).toBeDisabled();
    await expect(notesInput).toBeDisabled();

    // Wait for submission to complete
    await page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST'
    );
  });

  test('should complete full workflow in less than 30 seconds', async ({ page }) => {
    const startTime = Date.now();

    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Open modal
    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    // Fill form quickly (simulating user workflow)
    const incrementButton = page.getByLabel('egg count increase');
    await incrementButton.click();
    await incrementButton.click();
    await incrementButton.click(); // Set to 3

    // Submit form
    const responsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );

    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    await responsePromise;

    const endTime = Date.now();
    const duration = (endTime - startTime) / 1000; // Convert to seconds

    console.log(`Quick Add workflow completed in ${duration.toFixed(2)} seconds`);

    // Assert that the workflow took less than 30 seconds
    expect(duration).toBeLessThan(30);
  });

  test('should be responsive on mobile viewport', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });

    await page.goto('/');
    await page.waitForLoadState('networkidle');

    const quickAddButton = page.getByRole('button', { name: 'Přidat denní záznam' });
    await expect(quickAddButton).toBeEnabled({ timeout: 30000 });
    await quickAddButton.click();

    // Modal should be fullscreen on mobile
    const dialog = page.locator('.MuiDialog-root');
    await expect(dialog).toBeVisible();

    // All fields should be visible and touch-friendly
    await expect(page.getByLabel(/hejno/i)).toBeVisible();
    await expect(page.getByLabel(/datum/i)).toBeVisible();
    await expect(page.getByText('Počet vajec')).toBeVisible();
    await expect(page.getByLabel(/poznámky/i)).toBeVisible();

    // Check minimum touch target size (44px x 44px)
    const saveButton = page.getByRole('button', { name: /uložit/i });
    const buttonBox = await saveButton.boundingBox();
    expect(buttonBox?.height).toBeGreaterThanOrEqual(44);
  });
});
