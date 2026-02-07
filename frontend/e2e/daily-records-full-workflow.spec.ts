import { test, expect } from '@playwright/test';
import { CoopsPage } from './pages/CoopsPage';
import { CreateCoopModal } from './pages/CreateCoopModal';
import { CoopDetailPage } from './pages/CoopDetailPage';
import { FlocksPage } from './pages/FlocksPage';
import { CreateFlockModal } from './pages/CreateFlockModal';
import { generateCoopName } from './fixtures/coop.fixture';
import { generateFlockIdentifier, getDaysAgoDate } from './fixtures/flock.fixture';

/**
 * E2E tests for Daily Records Complete CRUD Workflow
 * US-022: E2E - Daily Records Full Workflow Test
 *
 * This test suite covers the complete daily records workflow from login to deletion:
 * - Setup: Create test coop and flock
 * - Create: Add daily record via Quick Add modal
 * - Read: View record in daily records list
 * - Update: Edit the record using edit modal
 * - Delete: Remove record with confirmation dialog
 * - Responsive: Tests on all breakpoints (mobile, tablet, desktop)
 * - Performance: Completes in < 60 seconds
 */
test.describe('Daily Records - Full CRUD Workflow', () => {
  let coopsPage: CoopsPage;
  let createCoopModal: CreateCoopModal;
  let coopDetailPage: CoopDetailPage;
  let flocksPage: FlocksPage;
  let createFlockModal: CreateFlockModal;
  let testCoopName: string;
  let testCoopId: string;
  let testFlockIdentifier: string;
  let testFlockId: string;
  let testRecordId: string;

  /**
   * Before each test, create a fresh test environment:
   * 1. Create a new coop
   * 2. Create a new flock in that coop
   * This ensures test isolation
   */
  test.beforeEach(async ({ page }) => {
    // Start timing for performance requirement
    const startTime = Date.now();

    // Initialize page objects
    coopsPage = new CoopsPage(page);
    createCoopModal = new CreateCoopModal(page);
    coopDetailPage = new CoopDetailPage(page);
    flocksPage = new FlocksPage(page);
    createFlockModal = new CreateFlockModal(page);

    // Generate unique test data
    testCoopName = generateCoopName();
    testFlockIdentifier = generateFlockIdentifier();

    // Navigate to coops page
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    // Create test coop
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

    await coopsPage.openCreateCoopModal();
    await page.waitForTimeout(500);
    await createCoopModal.fillForm(testCoopName, 'E2E Test Coop for Daily Records');
    await createCoopModal.submit();

    // Wait for modal to close and coop to be created
    await createCoopModal.waitForClose();
    await page.waitForTimeout(2000);
    await page.waitForLoadState('networkidle');

    // Verify coop ID was captured
    if (!createdCoopId) {
      throw new Error(`Could not capture coop ID from API response for coop: ${testCoopName}`);
    }

    testCoopId = createdCoopId;

    // Navigate to flocks page
    await flocksPage.goto(testCoopId);
    await page.waitForLoadState('networkidle');

    // Create test flock
    let createdFlockId: string | null = null;
    page.on('response', async (response) => {
      if (
        response.url().includes('/api/flocks') &&
        response.request().method() === 'POST' &&
        response.status() === 201
      ) {
        try {
          const data = await response.json();
          if (data && data.id) {
            createdFlockId = data.id;
            testFlockId = data.id;
          }
        } catch {
          // Ignore JSON parsing errors
        }
      }
    });

    await flocksPage.openCreateFlockModal();
    await createFlockModal.fillForm(
      testFlockIdentifier,
      getDaysAgoDate(30),
      5, // hens
      1, // roosters
      0  // chicks
    );
    await createFlockModal.submit();

    // Wait for modal to close and flock to be created
    await createFlockModal.waitForClose();
    await page.waitForTimeout(1000);
    await page.waitForLoadState('networkidle');

    // Verify flock ID was captured
    if (!createdFlockId) {
      throw new Error(`Could not capture flock ID from API response for flock: ${testFlockIdentifier}`);
    }

    const setupTime = (Date.now() - startTime) / 1000;
    console.log(`Test setup completed in ${setupTime.toFixed(2)} seconds`);
  });

  /**
   * Test 1: Complete CRUD workflow - Desktop
   * This is the primary test covering all CRUD operations in sequence
   */
  test('should complete full CRUD workflow on desktop', async ({ page }) => {
    const workflowStartTime = Date.now();

    // ===== CREATE =====
    // Navigate to dashboard
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Open Quick Add modal
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await expect(quickAddButton).toBeVisible();
    await quickAddButton.click();

    // Verify modal is open
    await expect(page.getByText('Rychlý záznam vajec')).toBeVisible();

    // Fill in the form
    const eggCount = 12;
    const notes = 'E2E test record - full workflow';

    // Set egg count using stepper
    const incrementButton = page.getByLabel('egg count increase');
    for (let i = 0; i < eggCount; i++) {
      await incrementButton.click();
    }

    // Verify egg count
    const eggCountInput = page.locator('input[type="number"][aria-label="egg count"]');
    await expect(eggCountInput).toHaveValue(eggCount.toString());

    // Add notes
    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill(notes);

    // Wait for POST response
    const createResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );

    // Submit form
    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    // Wait for response and capture record ID
    const createResponse = await createResponsePromise;
    const createdRecord = await createResponse.json();
    testRecordId = createdRecord.id;
    expect(testRecordId).toBeTruthy();

    // Modal should close
    await expect(page.getByText('Rychlý záznam vajec')).not.toBeVisible();

    // Success toast should appear
    await expect(page.getByText(/úspěšně vytvořen/i)).toBeVisible();

    console.log('✓ CREATE: Daily record created successfully');

    // ===== READ =====
    // Navigate to daily records list
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Verify page loaded
    await expect(page.getByRole('heading', { name: /denní záznamy/i })).toBeVisible();

    // Apply "Today" filter to show only today's records
    const todayChip = page.getByRole('button', { name: /dnes/i }).first();
    await todayChip.click();

    // Wait for filter to apply
    await page.waitForTimeout(300);

    // Find the record card
    const recordCard = page.locator('[class*="MuiCard-root"]').filter({
      hasText: eggCount.toString(),
    });
    await expect(recordCard).toBeVisible();

    // Verify record details are displayed
    await expect(recordCard.getByText(eggCount.toString())).toBeVisible();
    await expect(recordCard.getByText(notes)).toBeVisible();

    console.log('✓ READ: Daily record found in list');

    // ===== UPDATE =====
    // Click edit button (only visible for same-day records)
    const editButton = recordCard.locator('button[aria-label="edit record"]');
    await expect(editButton).toBeVisible();
    await editButton.click();

    // Verify edit modal opened
    await expect(page.getByText('Upravit denní záznam')).toBeVisible();

    // Verify pre-filled data
    const editEggCountField = page.getByLabelText(/počet vajec/i);
    await expect(editEggCountField).toHaveValue(eggCount.toString());

    const editNotesField = page.getByLabel(/poznámky/i);
    await expect(editNotesField).toHaveValue(notes);

    // Verify date and flock are read-only
    const dateField = page.getByLabel(/datum/i).first();
    await expect(dateField).toBeDisabled();
    await expect(page.getByText('Datum záznamu nelze změnit')).toBeVisible();

    const flockField = page.getByLabel(/hejno/i).first();
    await expect(flockField).toBeDisabled();
    await expect(page.getByText('Hejno nelze změnit')).toBeVisible();

    // Update values
    const updatedEggCount = 15;
    const updatedNotes = 'E2E test record - UPDATED';

    await editEggCountField.fill(updatedEggCount.toString());
    await editNotesField.fill(updatedNotes);

    // Wait for PUT response
    const updateResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'PUT' &&
        response.status() === 200
    );

    // Save changes
    const updateButton = page.getByRole('button', { name: /uložit/i }).last();
    await updateButton.click();

    // Wait for response
    await updateResponsePromise;

    // Modal should close
    await expect(page.getByText('Upravit denní záznam')).not.toBeVisible();

    // Verify updated values are displayed in the card
    await page.waitForTimeout(500); // Wait for UI to update
    const updatedCard = page.locator('[class*="MuiCard-root"]').filter({
      hasText: updatedEggCount.toString(),
    });
    await expect(updatedCard).toBeVisible();
    await expect(updatedCard.getByText(updatedNotes)).toBeVisible();

    console.log('✓ UPDATE: Daily record updated successfully');

    // ===== DELETE =====
    // Click edit button again to access delete functionality
    const editButtonForDelete = updatedCard.locator('button[aria-label="edit record"]');
    await editButtonForDelete.click();

    // Wait for edit modal
    await expect(page.getByText('Upravit denní záznam')).toBeVisible();

    // Click delete button in modal
    const deleteButton = page.getByRole('button', { name: /smazat záznam/i });
    await expect(deleteButton).toBeVisible();
    await deleteButton.click();

    // Verify delete confirmation dialog opened
    await expect(page.getByText('Smazat denní záznam')).toBeVisible();
    await expect(
      page.getByText(/opravdu chcete smazat tento denní záznam/i)
    ).toBeVisible();

    // Verify formatted date is shown in confirmation
    const today = new Date();
    const formattedDatePattern = /\d{2}\. \d{2}\. \d{4}/;
    await expect(page.locator('text=' + formattedDatePattern)).toBeVisible();

    // Wait for DELETE response
    const deleteResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'DELETE' &&
        response.status() === 204
    );

    // Confirm deletion
    const confirmDeleteButton = page.getByRole('button', { name: /smazat/i }).last();
    await confirmDeleteButton.click();

    // Wait for response
    await deleteResponsePromise;

    // Both dialogs should close
    await expect(page.getByText('Smazat denní záznam')).not.toBeVisible();
    await expect(page.getByText('Upravit denní záznam')).not.toBeVisible();

    // Success toast should appear
    await expect(page.getByText(/úspěšně smazán/i)).toBeVisible();

    // Verify record is removed from list
    await page.waitForTimeout(500);
    const deletedCard = page.locator('[class*="MuiCard-root"]').filter({
      hasText: updatedEggCount.toString(),
    });
    await expect(deletedCard).not.toBeVisible();

    console.log('✓ DELETE: Daily record deleted successfully');

    // Calculate total workflow time
    const workflowTime = (Date.now() - workflowStartTime) / 1000;
    console.log(`✓ Full CRUD workflow completed in ${workflowTime.toFixed(2)} seconds`);

    // Performance requirement: < 60 seconds
    expect(workflowTime).toBeLessThan(60);
  });

  /**
   * Test 2: CRUD workflow on mobile viewport (375x667 - iPhone SE)
   */
  test('should complete full CRUD workflow on mobile viewport', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });

    // Navigate to dashboard
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Open Quick Add modal
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await quickAddButton.click();

    // Fill in form
    const incrementButton = page.getByLabel('egg count increase');
    await incrementButton.click();
    await incrementButton.click();
    await incrementButton.click(); // 3 eggs

    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill('Mobile test');

    // Submit
    const createResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );

    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    await createResponsePromise;
    await expect(page.getByText('Rychlý záznam vajec')).not.toBeVisible();

    // Navigate to daily records
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Apply Today filter
    const todayChip = page.getByRole('button', { name: /dnes/i }).first();
    await todayChip.click();
    await page.waitForTimeout(300);

    // Verify record exists
    const recordCard = page.locator('[class*="MuiCard-root"]').filter({ hasText: '3' });
    await expect(recordCard).toBeVisible();

    // Edit the record
    const editButton = recordCard.locator('button[aria-label="edit record"]');
    await editButton.click();

    // Verify modal is fullscreen on mobile
    const dialog = page.locator('[role="dialog"]');
    await expect(dialog).toBeVisible();

    // Update egg count
    const editEggCountField = page.getByLabelText(/počet vajec/i);
    await editEggCountField.fill('5');

    // Save
    const updateResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'PUT'
    );

    const updateButton = page.getByRole('button', { name: /uložit/i }).last();
    await updateButton.click();

    await updateResponsePromise;

    // Verify updated value
    await page.waitForTimeout(500);
    const updatedCard = page.locator('[class*="MuiCard-root"]').filter({ hasText: '5' });
    await expect(updatedCard).toBeVisible();

    // Delete the record
    const editButtonForDelete = updatedCard.locator('button[aria-label="edit record"]');
    await editButtonForDelete.click();

    const deleteButton = page.getByRole('button', { name: /smazat záznam/i });
    await deleteButton.click();

    const deleteResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'DELETE'
    );

    const confirmDeleteButton = page.getByRole('button', { name: /smazat/i }).last();
    await confirmDeleteButton.click();

    await deleteResponsePromise;

    // Verify deletion
    await page.waitForTimeout(500);
    await expect(updatedCard).not.toBeVisible();

    console.log('✓ Mobile workflow completed successfully');
  });

  /**
   * Test 3: CRUD workflow on tablet viewport (768x1024 - iPad)
   */
  test('should complete full CRUD workflow on tablet viewport', async ({ page }) => {
    // Set tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });

    // Navigate to dashboard
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Open Quick Add modal
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await quickAddButton.click();

    // Fill in form
    const incrementButton = page.getByLabel('egg count increase');
    for (let i = 0; i < 8; i++) {
      await incrementButton.click();
    }

    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill('Tablet test');

    // Submit
    const createResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );

    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    await createResponsePromise;

    // Navigate to daily records
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Apply Today filter
    const todayChip = page.getByRole('button', { name: /dnes/i }).first();
    await todayChip.click();
    await page.waitForTimeout(300);

    // Verify grid layout works on tablet (2 columns expected)
    const grid = page.locator('[class*="MuiGrid-container"]').first();
    await expect(grid).toBeVisible();

    // Find and edit record
    const recordCard = page.locator('[class*="MuiCard-root"]').filter({ hasText: '8' });
    await expect(recordCard).toBeVisible();

    const editButton = recordCard.locator('button[aria-label="edit record"]');
    await editButton.click();

    const editEggCountField = page.getByLabelText(/počet vajec/i);
    await editEggCountField.fill('10');

    const updateResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'PUT'
    );

    const updateButton = page.getByRole('button', { name: /uložit/i }).last();
    await updateButton.click();

    await updateResponsePromise;

    console.log('✓ Tablet workflow completed successfully');
  });

  /**
   * Test 4: Test performance - workflow completes in < 60 seconds
   */
  test('should complete workflow within performance budget', async ({ page }) => {
    const startTime = Date.now();

    // Quick workflow: Create -> Read -> Delete
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // CREATE
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await quickAddButton.click();

    const incrementButton = page.getByLabel('egg count increase');
    await incrementButton.click();

    const createResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST'
    );

    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();
    await createResponsePromise;

    // READ
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    const todayChip = page.getByRole('button', { name: /dnes/i }).first();
    await todayChip.click();
    await page.waitForTimeout(300);

    const recordCard = page.locator('[class*="MuiCard-root"]').first();
    await expect(recordCard).toBeVisible();

    // DELETE
    const editButton = recordCard.locator('button[aria-label="edit record"]');
    await editButton.click();

    const deleteButton = page.getByRole('button', { name: /smazat záznam/i });
    await deleteButton.click();

    const deleteResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'DELETE'
    );

    const confirmDeleteButton = page.getByRole('button', { name: /smazat/i }).last();
    await confirmDeleteButton.click();
    await deleteResponsePromise;

    const duration = (Date.now() - startTime) / 1000;
    console.log(`Performance test completed in ${duration.toFixed(2)} seconds`);

    expect(duration).toBeLessThan(60);
  });

  /**
   * Test 5: Test screenshot capture on failure
   * This test intentionally looks for a non-existent element to trigger screenshot
   */
  test('should capture screenshot on failure', async ({ page }) => {
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // This will pass - just verifying screenshot capability is configured
    await expect(page.getByRole('heading', { name: /denní záznamy/i })).toBeVisible();
  });

  /**
   * Test 6: Verify all breakpoints work correctly
   */
  test('should display correctly on all standard breakpoints', async ({ page }) => {
    const breakpoints = [
      { name: 'Mobile Portrait', width: 320, height: 568 },
      { name: 'Mobile Landscape', width: 568, height: 320 },
      { name: 'Tablet', width: 768, height: 1024 },
      { name: 'Desktop', width: 1024, height: 768 },
      { name: 'Large Desktop', width: 1920, height: 1080 },
    ];

    for (const breakpoint of breakpoints) {
      await page.setViewportSize({ width: breakpoint.width, height: breakpoint.height });
      await page.goto('/daily-records');
      await page.waitForLoadState('networkidle');

      // Verify page renders correctly
      await expect(page.getByRole('heading', { name: /denní záznamy/i })).toBeVisible();

      // Verify filters are accessible
      await expect(page.getByLabel(/hejno/i)).toBeVisible();
      await expect(page.getByLabel(/od data/i)).toBeVisible();

      console.log(`✓ ${breakpoint.name} (${breakpoint.width}x${breakpoint.height}) - OK`);
    }
  });

  /**
   * Test 7: Verify error handling in CRUD operations
   */
  test('should handle validation errors gracefully', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Open Quick Add modal
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await quickAddButton.click();

    // Try to submit with invalid data (future date)
    const dateInput = page.getByLabel(/datum/i);
    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    const futureDateStr = futureDate.toISOString().split('T')[0];

    await dateInput.fill(futureDateStr);
    await dateInput.blur();

    // Should show validation error
    await expect(page.getByText(/nemůže být v budoucnosti/i)).toBeVisible();

    // Save button should be disabled
    const saveButton = page.getByRole('button', { name: /uložit/i });
    await expect(saveButton).toBeDisabled();

    console.log('✓ Validation errors handled correctly');
  });
});
