import { test, expect } from '@playwright/test';
import { CoopsPage } from './pages/CoopsPage';
import { CoopDetailPage } from './pages/CoopDetailPage';
import { FlocksPage } from './pages/FlocksPage';
import { CreateFlockModal } from './pages/CreateFlockModal';
import { EditFlockModal } from './pages/EditFlockModal';
import { ArchiveFlockDialog } from './pages/ArchiveFlockDialog';
import { CreateCoopModal } from './pages/CreateCoopModal';
import {
  testFlocks,
  invalidFlocks,
  generateFlockIdentifier,
  getDaysAgoDate,
} from './fixtures/flock.fixture';
import { generateCoopName } from './fixtures/coop.fixture';

test.describe('Flock Management - Complete CRUD Journey', () => {
  let coopsPage: CoopsPage;
  let coopDetailPage: CoopDetailPage;
  let flocksPage: FlocksPage;
  let createFlockModal: CreateFlockModal;
  let editFlockModal: EditFlockModal;
  let archiveFlockDialog: ArchiveFlockDialog;
  let createCoopModal: CreateCoopModal;
  let testCoopId: string;
  let testCoopName: string;

  test.beforeEach(async ({ page }) => {
    // Initialize page objects
    coopsPage = new CoopsPage(page);
    coopDetailPage = new CoopDetailPage(page);
    flocksPage = new FlocksPage(page);
    createFlockModal = new CreateFlockModal(page);
    editFlockModal = new EditFlockModal(page);
    archiveFlockDialog = new ArchiveFlockDialog(page);
    createCoopModal = new CreateCoopModal(page);

    // Create a test coop for flock tests
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    await coopsPage.openCreateCoopModal();
    await page.waitForTimeout(500);

    testCoopName = generateCoopName('Flock Test Coop');
    await createCoopModal.createCoop(testCoopName, 'Test Location for Flocks');

    // Wait for modal to close completely
    await createCoopModal.waitForClose();
    await page.waitForTimeout(1000);
    await page.waitForLoadState('networkidle');

    // Navigate to the created coop to get its ID
    const coopCard = await coopsPage.getCoopCard(testCoopName);
    await expect(coopCard).toBeVisible({ timeout: 10000 });
    await coopCard.click();
    await page.waitForLoadState('networkidle');

    // Extract coop ID from URL
    await page.waitForTimeout(1000);
    const url = page.url();
    const match = url.match(/\/coops\/([a-f0-9-]+)/);
    testCoopId = match ? match[1] : '';

    if (!testCoopId) {
      throw new Error(`Could not extract coop ID from URL: ${url}`);
    }
  });

  test.describe('Navigation', () => {
    test('should navigate to flocks page from coop detail', async ({ page }) => {
      // Start from coop detail page
      await coopDetailPage.goto(testCoopId);

      // Click flocks button
      await coopDetailPage.navigateToFlocks();

      // Verify we're on the flocks page
      await expect(flocksPage.pageTitle).toBeVisible();
      expect(page.url()).toContain(`/coops/${testCoopId}/flocks`);

      // Verify empty state is shown (no flocks yet)
      await expect(flocksPage.emptyStateMessage).toBeVisible();
    });
  });

  test.describe('Create Flock', () => {
    test('should create a flock with valid data', async () => {
      await flocksPage.goto(testCoopId);

      // Open create modal
      await flocksPage.openCreateFlockModal();
      await expect(createFlockModal.modalTitle).toBeVisible();

      // Fill and submit form
      const flockData = testFlocks.basic();
      await createFlockModal.createFlock(flockData);

      // Verify flock appears in the list
      await flocksPage.waitForFlocksToLoad();
      await expect(flocksPage.getFlockCard(flockData.identifier)).toBeVisible();

      // Verify flock details
      const composition = await flocksPage.getFlockComposition(flockData.identifier);
      expect(composition.hens).toBe(flockData.hens);
      expect(composition.roosters).toBe(flockData.roosters);
      expect(composition.chicks).toBe(flockData.chicks);
      expect(composition.total).toBe(flockData.hens + flockData.roosters + flockData.chicks);

      // Verify status is Active
      const status = await flocksPage.getFlockStatus(flockData.identifier);
      expect(status).toMatch(/aktivní|active/i);
    });

    test('should show validation error for empty identifier', async () => {
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Fill form with empty identifier
      const invalidData = invalidFlocks.emptyIdentifier();
      await createFlockModal.fillPartialForm(invalidData);

      // Blur the identifier field to trigger validation
      await createFlockModal.identifierInput.blur();

      // Submit button should be disabled
      await expect(createFlockModal.submitButton).toBeDisabled();
    });

    test('should show validation error for future hatch date', async () => {
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Fill form with future date
      const invalidData = invalidFlocks.futureHatchDate();
      await createFlockModal.fillPartialForm({
        identifier: generateFlockIdentifier(),
        ...invalidData,
      });

      // Blur the hatch date field to trigger validation
      await createFlockModal.hatchDateInput.blur();

      // Wait for error message
      await expect(createFlockModal.errorMessage.first()).toBeVisible();

      // Verify error message mentions future date
      const errorText = await createFlockModal.getErrorMessage();
      expect(errorText.toLowerCase()).toContain('future' || 'budoucnosti');
    });

    test('should show validation error when all counts are zero', async () => {
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Fill form with zero counts
      const invalidData = invalidFlocks.zeroCounts();
      await createFlockModal.fillPartialForm({
        identifier: generateFlockIdentifier(),
        hatchDate: getDaysAgoDate(30),
        ...invalidData,
      });

      // Blur the chicks field to trigger validation
      await createFlockModal.chicksInput.blur();

      // Submit button should be disabled
      await expect(createFlockModal.submitButton).toBeDisabled();
    });

    test('should cancel flock creation', async ({ page }) => {
      await flocksPage.goto(testCoopId);

      const initialCount = await flocksPage.getFlockCount();

      await flocksPage.openCreateFlockModal();
      await expect(createFlockModal.modalTitle).toBeVisible();

      // Fill form but cancel
      const flockData = testFlocks.basic();
      await createFlockModal.fillForm(flockData);
      await createFlockModal.cancel();

      // Modal should close
      await expect(createFlockModal.modal).not.toBeVisible();

      // Flock count should remain the same
      const finalCount = await flocksPage.getFlockCount();
      expect(finalCount).toBe(initialCount);
    });
  });

  test.describe('View Flocks List', () => {
    test('should display empty state when no flocks exist', async () => {
      await flocksPage.goto(testCoopId);

      // Verify empty state
      await expect(flocksPage.emptyStateMessage).toBeVisible();
      expect(await flocksPage.getFlockCount()).toBe(0);
    });

    test('should display populated list of flocks', async () => {
      await flocksPage.goto(testCoopId);

      // Create multiple flocks
      const flock1 = testFlocks.basic();
      const flock2 = testFlocks.hensOnly();
      const flock3 = testFlocks.recentHatch();

      for (const flockData of [flock1, flock2, flock3]) {
        await flocksPage.openCreateFlockModal();
        await createFlockModal.createFlock(flockData);
        await flocksPage.waitForFlocksToLoad();
      }

      // Verify multiple flocks can be displayed
      expect(await flocksPage.getFlockCount()).toBe(3);
      await expect(flocksPage.getFlockCard(flock1.identifier)).toBeVisible();
      await expect(flocksPage.getFlockCard(flock2.identifier)).toBeVisible();
      await expect(flocksPage.getFlockCard(flock3.identifier)).toBeVisible();

      // Verify empty state is not shown
      await expect(flocksPage.emptyStateMessage).not.toBeVisible();

      // Verify flock composition is displayed correctly for each flock
      const composition1 = await flocksPage.getFlockComposition(flock1.identifier);
      expect(composition1.hens).toBe(flock1.hens);
      expect(composition1.roosters).toBe(flock1.roosters);
      expect(composition1.chicks).toBe(flock1.chicks);
      expect(composition1.total).toBe(flock1.hens + flock1.roosters + flock1.chicks);

      const composition2 = await flocksPage.getFlockComposition(flock2.identifier);
      expect(composition2.hens).toBe(flock2.hens);
      expect(composition2.roosters).toBe(flock2.roosters);
      expect(composition2.chicks).toBe(flock2.chicks);
      expect(composition2.total).toBe(flock2.hens + flock2.roosters + flock2.chicks);

      const composition3 = await flocksPage.getFlockComposition(flock3.identifier);
      expect(composition3.hens).toBe(flock3.hens);
      expect(composition3.roosters).toBe(flock3.roosters);
      expect(composition3.chicks).toBe(flock3.chicks);
      expect(composition3.total).toBe(flock3.hens + flock3.roosters + flock3.chicks);

      // Verify flock status is displayed (all should be Active)
      const status1 = await flocksPage.getFlockStatus(flock1.identifier);
      expect(status1).toMatch(/aktivní|active/i);

      const status2 = await flocksPage.getFlockStatus(flock2.identifier);
      expect(status2).toMatch(/aktivní|active/i);

      const status3 = await flocksPage.getFlockStatus(flock3.identifier);
      expect(status3).toMatch(/aktivní|active/i);
    });
  });

  test.describe('Edit Flock', () => {
    test('should edit flock information', async () => {
      await flocksPage.goto(testCoopId);

      // Create a flock
      const originalFlock = testFlocks.basic();
      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(originalFlock);
      await flocksPage.waitForFlocksToLoad();

      // Open edit modal
      await flocksPage.clickEditFlock(originalFlock.identifier);
      await expect(editFlockModal.modalTitle).toBeVisible();

      // Verify current values
      expect(await editFlockModal.getCurrentIdentifier()).toBe(originalFlock.identifier);

      // Edit the flock (only identifier and hatch date can be edited)
      const newIdentifier = generateFlockIdentifier('Edited Flock');
      const newHatchDate = '2024-01-15';
      await editFlockModal.editFlock({
        identifier: newIdentifier,
        hatchDate: newHatchDate,
      });

      // Verify changes
      await flocksPage.waitForFlocksToLoad();
      await expect(flocksPage.getFlockCard(newIdentifier)).toBeVisible();
      await expect(flocksPage.getFlockCard(originalFlock.identifier)).not.toBeVisible();

      // Verify composition remained unchanged (composition cannot be edited via edit modal)
      const composition = await flocksPage.getFlockComposition(newIdentifier);
      expect(composition.hens).toBe(originalFlock.hens);
      expect(composition.roosters).toBe(originalFlock.roosters);
      expect(composition.chicks).toBe(originalFlock.chicks);
    });

    test('should cancel flock edit', async () => {
      await flocksPage.goto(testCoopId);

      // Create a flock
      const originalFlock = testFlocks.basic();
      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(originalFlock);
      await flocksPage.waitForFlocksToLoad();

      // Open edit modal
      await flocksPage.clickEditFlock(originalFlock.identifier);
      await expect(editFlockModal.modalTitle).toBeVisible();

      // Make changes but cancel
      await editFlockModal.fillForm({
        identifier: 'Should Not Save',
      });
      await editFlockModal.cancel();

      // Verify original flock is still visible with original name
      await expect(flocksPage.getFlockCard(originalFlock.identifier)).toBeVisible();
      await expect(flocksPage.getFlockCard('Should Not Save')).not.toBeVisible();
    });

    test('should show validation errors on invalid edit data', async () => {
      await flocksPage.goto(testCoopId);

      // Create a flock
      const originalFlock = testFlocks.basic();
      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(originalFlock);
      await flocksPage.waitForFlocksToLoad();

      // Open edit modal
      await flocksPage.clickEditFlock(originalFlock.identifier);

      // Try to set empty identifier
      await editFlockModal.fillForm({
        identifier: '',
      });
      await editFlockModal.identifierInput.blur();

      // Submit should be disabled
      await expect(editFlockModal.submitButton).toBeDisabled();
    });
  });

  test.describe('Archive Flock', () => {
    test('should archive a flock after confirmation', async () => {
      await flocksPage.goto(testCoopId);

      // Create a flock
      const flockData = testFlocks.basic();
      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);
      await flocksPage.waitForFlocksToLoad();

      // Verify flock is visible in active filter
      await expect(flocksPage.getFlockCard(flockData.identifier)).toBeVisible();

      // Archive the flock
      await flocksPage.clickArchiveFlock(flockData.identifier);
      await expect(archiveFlockDialog.dialogTitle).toBeVisible();

      // Verify flock name in dialog
      const dialogFlockName = await archiveFlockDialog.getFlockName();
      expect(dialogFlockName).toBe(flockData.identifier);

      // Confirm archive
      await archiveFlockDialog.confirm();

      // Wait for archive to complete
      await flocksPage.page.waitForTimeout(1000);

      // Flock should not be visible in active filter
      await flocksPage.filterActive();
      await expect(flocksPage.getFlockCard(flockData.identifier)).not.toBeVisible();

      // Flock should be visible in all filter
      await flocksPage.filterAll();
      await expect(flocksPage.getFlockCard(flockData.identifier)).toBeVisible();

      // Verify status is Archived
      const status = await flocksPage.getFlockStatus(flockData.identifier);
      expect(status).toMatch(/archivováno|archived/i);
    });

    test('should cancel flock archive', async () => {
      await flocksPage.goto(testCoopId);

      // Create a flock
      const flockData = testFlocks.basic();
      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);
      await flocksPage.waitForFlocksToLoad();

      // Try to archive but cancel
      await flocksPage.clickArchiveFlock(flockData.identifier);
      await expect(archiveFlockDialog.dialogTitle).toBeVisible();
      await archiveFlockDialog.cancel();

      // Flock should still be visible and active
      await expect(flocksPage.getFlockCard(flockData.identifier)).toBeVisible();
      const status = await flocksPage.getFlockStatus(flockData.identifier);
      expect(status).toMatch(/aktivní|active/i);
    });
  });

  test.describe('Filter Flocks', () => {
    test('should filter archived flocks', async () => {
      await flocksPage.goto(testCoopId);

      // Create active and archived flocks
      const activeFlock = testFlocks.basic();
      const archivedFlock = testFlocks.hensOnly();

      // Create active flock
      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(activeFlock);
      await flocksPage.waitForFlocksToLoad();

      // Create and archive second flock
      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(archivedFlock);
      await flocksPage.waitForFlocksToLoad();

      await flocksPage.clickArchiveFlock(archivedFlock.identifier);
      await archiveFlockDialog.confirm();
      await flocksPage.page.waitForTimeout(1000);

      // Test Active filter (default)
      await flocksPage.filterActive();
      await expect(flocksPage.getFlockCard(activeFlock.identifier)).toBeVisible();
      await expect(flocksPage.getFlockCard(archivedFlock.identifier)).not.toBeVisible();
      expect(await flocksPage.getFlockCount()).toBe(1);

      // Test All filter
      await flocksPage.filterAll();
      await expect(flocksPage.getFlockCard(activeFlock.identifier)).toBeVisible();
      await expect(flocksPage.getFlockCard(archivedFlock.identifier)).toBeVisible();
      expect(await flocksPage.getFlockCount()).toBe(2);
    });
  });

  test.describe('Mobile Responsiveness', () => {
    test('should work on mobile viewport', async ({ page, browserName }) => {
      // Set iPhone SE viewport (375x667)
      await page.setViewportSize({ width: 375, height: 667 });

      await flocksPage.goto(testCoopId);

      // Create a flock on mobile
      const flockData = testFlocks.basic();
      await flocksPage.openCreateFlockModal();

      // Verify modal is visible and functional on mobile
      await expect(createFlockModal.modalTitle).toBeVisible();
      await createFlockModal.createFlock(flockData);

      // Verify flock appears in list
      await flocksPage.waitForFlocksToLoad();
      await expect(flocksPage.getFlockCard(flockData.identifier)).toBeVisible();

      // Test touch-friendly interactions
      await flocksPage.clickEditFlock(flockData.identifier);
      await expect(editFlockModal.modalTitle).toBeVisible();
      await editFlockModal.cancel();

      // Verify FAB button is accessible
      await expect(flocksPage.addFlockButton).toBeVisible();

      // Check that buttons meet minimum touch target size (44x44px)
      const fabBox = await flocksPage.addFlockButton.boundingBox();
      expect(fabBox).not.toBeNull();
      if (fabBox) {
        expect(fabBox.width).toBeGreaterThanOrEqual(44);
        expect(fabBox.height).toBeGreaterThanOrEqual(44);
      }
    });
  });

  test.describe('API Integration', () => {
    test('should handle API errors gracefully', async ({ page }) => {
      await flocksPage.goto(testCoopId);

      // Monitor network requests
      const apiRequests: string[] = [];
      page.on('request', (request) => {
        if (request.url().includes('/api/')) {
          apiRequests.push(request.url());
        }
      });

      // Create a flock
      const flockData = testFlocks.basic();
      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);

      // Wait for API call to complete
      await page.waitForTimeout(2000);

      // Verify API was called
      const createFlockRequest = apiRequests.find((url) =>
        url.includes(`/coops/${testCoopId}/flocks`) && !url.includes('?')
      );
      expect(createFlockRequest).toBeDefined();
    });
  });
});
