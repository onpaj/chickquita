import { test, expect } from '@playwright/test';
import { CoopsPage } from './pages/CoopsPage';
import { CreateCoopModal } from './pages/CreateCoopModal';
import { EditCoopModal } from './pages/EditCoopModal';
import { DashboardPage } from './pages/DashboardPage';

/**
 * E2E Tests for Milestone 2: Coop Management
 *
 * Coverage:
 * - Create coop with name and optional location
 * - List coops
 * - Edit coop details
 * - Archive coop
 * - Delete empty coop
 * - Form validation
 * - Mobile responsiveness
 * - Tenant isolation (implicit through authentication)
 */

test.describe('M2: Coop Management', () => {
  let coopsPage: CoopsPage;
  let createCoopModal: CreateCoopModal;
  let editCoopModal: EditCoopModal;
  let dashboardPage: DashboardPage;

  // Verify backend is responding before running any tests
  test.beforeAll(async ({ request }) => {
    // Check backend health
    const healthResponse = await request.get('http://localhost:5100/health');
    if (!healthResponse.ok()) {
      throw new Error(
        'Backend is not responding! Make sure the backend is running:\n' +
        'cd backend && dotnet run --project src/Chickquita.Api'
      );
    }
  });

  test.beforeEach(async ({ page }) => {
    coopsPage = new CoopsPage(page);
    createCoopModal = new CreateCoopModal(page);
    editCoopModal = new EditCoopModal(page);
    dashboardPage = new DashboardPage(page);

    // Navigate to coops page before each test
    await coopsPage.goto();
  });

  test.describe('API Integration', () => {
    test('should not return 4xx validation errors when loading coops', async ({ page }) => {
      // Track all API responses to catch any 400 errors
      const failedRequests: Array<{ url: string; status: number; statusText: string }> = [];

      page.on('response', response => {
        if (response.url().includes('/api/coops') && response.status() >= 400 && response.status() < 500) {
          failedRequests.push({
            url: response.url(),
            status: response.status(),
            statusText: response.statusText()
          });
        }
      });

      // Navigate to coops page
      await page.waitForLoadState('networkidle');

      // CRITICAL: Test will fail if backend returns 4xx errors
      if (failedRequests.length > 0) {
        const errorDetails = failedRequests.map(r => `${r.status} ${r.statusText} - ${r.url}`).join('\n');
        throw new Error(`Backend returned client errors:\n${errorDetails}\n\nThis typically means:\n- Required query parameters are missing default values\n- Request validation is too strict\n- Endpoint configuration is incorrect`);
      }

      // Verify no validation errors shown to user
      const validationError = page.getByText(/validation error|formulář obsahuje chyby/i);
      await expect(validationError).not.toBeVisible();

      // Verify page loaded successfully
      await expect(coopsPage.pageTitle).toBeVisible();
    });

    test('should successfully GET /api/coops and return valid data structure', async ({ page }) => {
      // Intercept the API call
      const responsePromise = page.waitForResponse(
        response => response.url().includes('/api/coops') &&
                   response.request().method() === 'GET' &&
                   !response.url().includes('/coops/'), // Exclude /coops/{id} endpoints
        { timeout: 10000 }
      );

      // Reload to trigger fresh API call
      await page.reload();

      // Wait for response
      const response = await responsePromise;

      // CRITICAL: Must be 200 OK (catches missing default parameters, validation issues)
      expect(response.status(), `Expected 200 OK but got ${response.status()} ${response.statusText()}`).toBe(200);

      // Verify response structure
      const data = await response.json();
      expect(Array.isArray(data), 'API should return an array of coops').toBeTruthy();

      // If data exists, verify structure
      if (data.length > 0) {
        const firstCoop = data[0];
        expect(firstCoop).toHaveProperty('id');
        expect(firstCoop).toHaveProperty('name');
        expect(firstCoop).toHaveProperty('isActive');
      }
    });

    test('should handle empty coops list gracefully', async ({ page }) => {
      await page.waitForLoadState('networkidle');

      // Page should display proper UI regardless of data
      const hasEmptyState = await coopsPage.isEmptyStateVisible();
      const coopCount = await coopsPage.getCoopCount();

      // Either show empty state or list (not error page)
      expect(hasEmptyState || coopCount > 0).toBeTruthy();

      // Should not show generic error messages
      const errorHeading = page.getByRole('heading', { name: /error|chyba/i });
      await expect(errorHeading).not.toBeVisible();
    });

    test('should not fail with missing optional query parameters', async ({ page }) => {
      // Monitor console errors
      const consoleErrors: string[] = [];
      page.on('console', msg => {
        if (msg.type() === 'error' && msg.text().includes('400')) {
          consoleErrors.push(msg.text());
        }
      });

      // Check network for failed API calls
      let apiCallFailed = false;
      page.on('response', async response => {
        if (response.url().includes('/api/coops') && response.request().method() === 'GET') {
          if (response.status() === 400) {
            const body = await response.text();
            apiCallFailed = true;
            throw new Error(
              `API returned 400 Bad Request for ${response.url()}\n` +
              `This usually means a required parameter is missing a default value.\n` +
              `Response body: ${body}`
            );
          }
        }
      });

      await page.waitForLoadState('networkidle');

      // Test fails if 400 error detected
      expect(apiCallFailed, 'API should not return 400 errors for GET requests without query params').toBe(false);
      expect(consoleErrors.length, `Console should not have 400 errors: ${consoleErrors.join(', ')}`).toBe(0);
    });
  });

  test.describe('Create Coop', () => {
    test('should create a coop with name only', async ({ page }) => {
      const coopName = `Test Coop ${Date.now()}`;

      // Open create coop modal
      await coopsPage.openCreateCoopModal();
      await expect(createCoopModal.modal).toBeVisible();

      // Fill and submit form
      await createCoopModal.createCoop(coopName);

      // Modal should close
      await createCoopModal.waitForClose();

      // Verify coop appears in the list
      await expect(page.getByText(coopName)).toBeVisible();

      // Verify empty state is not shown
      expect(await coopsPage.isEmptyStateVisible()).toBe(false);
    });

    test('should create a coop with name and location', async ({ page }) => {
      const coopName = `Coop with Location ${Date.now()}`;
      const location = 'Behind the house';

      await coopsPage.openCreateCoopModal();
      await createCoopModal.createCoop(coopName, location);
      await createCoopModal.waitForClose();

      // Verify coop with location appears
      const coopCard = await coopsPage.getCoopCard(coopName);
      await expect(coopCard).toBeVisible();
      await expect(coopCard).toContainText(location);
    });

    test('should show validation error when name is empty', async () => {
      await coopsPage.openCreateCoopModal();

      // Try to submit with empty name
      await createCoopModal.nameInput.clear();
      await createCoopModal.submit();

      // Modal should remain open with error
      await expect(createCoopModal.modal).toBeVisible();
      await expect(createCoopModal.errorMessage).toBeVisible();
    });

    test('should cancel coop creation', async () => {
      await coopsPage.openCreateCoopModal();

      // Fill form but cancel
      await createCoopModal.fillForm('Cancelled Coop', 'Some location');
      await createCoopModal.cancel();

      // Modal should close
      await createCoopModal.waitForClose();

      // Coop should not appear
      expect(await coopsPage.isCoopVisible('Cancelled Coop')).toBe(false);
    });

    test('should enforce name max length validation', async () => {
      await coopsPage.openCreateCoopModal();

      // Try to create coop with name longer than 100 characters
      const longName = 'A'.repeat(101);
      await createCoopModal.fillForm(longName);
      await createCoopModal.submit();

      // Should show validation error
      await expect(createCoopModal.errorMessage).toBeVisible();
      await expect(createCoopModal.modal).toBeVisible();
    });
  });

  test.describe('List Coops', () => {
    test('should display empty state when no coops exist', async () => {
      // This test assumes a fresh user with no coops
      // In a real scenario, you might need to delete all coops first
      if (await coopsPage.getCoopCount() === 0) {
        await expect(coopsPage.emptyStateMessage).toBeVisible();
        await expect(coopsPage.createCoopButton).toBeVisible();
      }
    });

    test('should display list of coops', async ({ page }) => {
      // Create multiple coops
      const coops = [
        { name: `Coop A ${Date.now()}`, location: 'North side' },
        { name: `Coop B ${Date.now()}`, location: 'South side' },
        { name: `Coop C ${Date.now()}` },
      ];

      for (const coop of coops) {
        await coopsPage.openCreateCoopModal();
        await createCoopModal.createCoop(coop.name, coop.location);
        await createCoopModal.waitForClose();
      }

      // Verify all coops are visible
      for (const coop of coops) {
        await expect(page.getByText(coop.name)).toBeVisible();
      }

      // Verify count
      const count = await coopsPage.getCoopCount();
      expect(count).toBeGreaterThanOrEqual(coops.length);
    });

    test('should navigate to coops page from dashboard', async ({ page }) => {
      await dashboardPage.goto();
      await dashboardPage.navigateToCoops();

      // Should be on coops page
      await expect(page).toHaveURL('/coops');
      await expect(coopsPage.pageTitle).toBeVisible();
    });
  });

  test.describe('Edit Coop', () => {
    let testCoopName: string;

    test.beforeEach(async () => {
      // Create a coop to edit
      testCoopName = `Editable Coop ${Date.now()}`;
      await coopsPage.openCreateCoopModal();
      await createCoopModal.createCoop(testCoopName, 'Original location');
      await createCoopModal.waitForClose();
    });

    test('should edit coop name', async ({ page }) => {
      const newName = `Updated ${testCoopName}`;

      await coopsPage.clickEditCoop(testCoopName);
      await expect(editCoopModal.modal).toBeVisible();

      // Verify form is pre-filled
      expect(await editCoopModal.getCurrentName()).toBe(testCoopName);

      await editCoopModal.editCoop(newName);
      await editCoopModal.waitForClose();

      // Verify updated name appears
      await expect(page.getByText(newName)).toBeVisible();
      await expect(page.getByText(testCoopName)).not.toBeVisible();
    });

    test('should edit coop location', async ({ page }) => {
      const newLocation = 'New location';

      await coopsPage.clickEditCoop(testCoopName);
      await editCoopModal.editCoop(testCoopName, newLocation);
      await editCoopModal.waitForClose();

      // Verify updated location
      const coopCard = await coopsPage.getCoopCard(testCoopName);
      await expect(coopCard).toContainText(newLocation);
    });

    test('should cancel edit', async ({ page }) => {
      await coopsPage.clickEditCoop(testCoopName);

      await editCoopModal.fillForm('Different Name', 'Different Location');
      await editCoopModal.cancel();
      await editCoopModal.waitForClose();

      // Original name should still be visible
      await expect(page.getByText(testCoopName)).toBeVisible();
      await expect(page.getByText('Different Name')).not.toBeVisible();
    });

    test('should show validation error when editing to empty name', async () => {
      await coopsPage.clickEditCoop(testCoopName);

      await editCoopModal.nameInput.clear();
      await editCoopModal.submit();

      // Should show error
      await expect(editCoopModal.errorMessage).toBeVisible();
      await expect(editCoopModal.modal).toBeVisible();
    });
  });

  test.describe('Archive Coop', () => {
    let testCoopName: string;

    test.beforeEach(async () => {
      // Create a coop to archive
      testCoopName = `Archivable Coop ${Date.now()}`;
      await coopsPage.openCreateCoopModal();
      await createCoopModal.createCoop(testCoopName);
      await createCoopModal.waitForClose();
    });

    test('should archive a coop', async ({ page }) => {
      await coopsPage.clickArchiveCoop(testCoopName);

      // Confirm archive dialog
      const confirmDialog = page.getByRole('dialog');
      await expect(confirmDialog).toBeVisible();

      const confirmButton = confirmDialog.getByRole('button', { name: /archive|confirm|yes|ano/i });
      await confirmButton.click();

      // Wait for dialog to close
      await expect(confirmDialog).not.toBeVisible();

      // Coop should no longer be visible in active list
      // Note: Depending on implementation, archived coops might be filtered out
      // or shown with an "archived" badge
      await page.waitForTimeout(1000); // Wait for UI update
    });

    test('should cancel archive', async ({ page }) => {
      await coopsPage.clickArchiveCoop(testCoopName);

      const confirmDialog = page.getByRole('dialog');
      await expect(confirmDialog).toBeVisible();

      const cancelButton = confirmDialog.getByRole('button', { name: /cancel|no|ne|zrušit/i });
      await cancelButton.click();

      await expect(confirmDialog).not.toBeVisible();

      // Coop should still be visible
      await expect(page.getByText(testCoopName)).toBeVisible();
    });
  });

  test.describe('Delete Coop', () => {
    let testCoopName: string;

    test.beforeEach(async () => {
      // Create a coop to delete
      testCoopName = `Deletable Coop ${Date.now()}`;
      await coopsPage.openCreateCoopModal();
      await createCoopModal.createCoop(testCoopName);
      await createCoopModal.waitForClose();
    });

    test('should delete an empty coop', async ({ page }) => {
      await coopsPage.clickDeleteCoop(testCoopName);

      // Confirm delete dialog
      const confirmDialog = page.getByRole('dialog');
      await expect(confirmDialog).toBeVisible();

      const confirmButton = confirmDialog.getByRole('button', { name: /delete|confirm|yes|ano|smazat/i });
      await confirmButton.click();

      // Wait for deletion
      await expect(confirmDialog).not.toBeVisible();

      // Coop should no longer be visible
      await expect(page.getByText(testCoopName)).not.toBeVisible();
    });

    test('should cancel delete', async ({ page }) => {
      await coopsPage.clickDeleteCoop(testCoopName);

      const confirmDialog = page.getByRole('dialog');
      const cancelButton = confirmDialog.getByRole('button', { name: /cancel|no|ne|zrušit/i });
      await cancelButton.click();

      await expect(confirmDialog).not.toBeVisible();

      // Coop should still be visible
      await expect(page.getByText(testCoopName)).toBeVisible();
    });

    // Note: Test for "cannot delete coop with active flocks" requires M3 implementation
  });

  test.describe('Mobile Responsiveness', () => {
    test('should work on mobile viewport', async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE

      const coopName = `Mobile Coop ${Date.now()}`;

      // Create coop on mobile
      await coopsPage.openCreateCoopModal();
      await createCoopModal.createCoop(coopName, 'Mobile location');
      await createCoopModal.waitForClose();

      // Verify coop appears
      await expect(page.getByText(coopName)).toBeVisible();

      // Edit on mobile
      await coopsPage.clickEditCoop(coopName);
      await editCoopModal.editCoop(`${coopName} Updated`);
      await editCoopModal.waitForClose();

      await expect(page.getByText(`${coopName} Updated`)).toBeVisible();
    });

    test('should have touch-friendly buttons on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });

      const coopName = `Touch Test ${Date.now()}`;
      await coopsPage.openCreateCoopModal();
      await createCoopModal.createCoop(coopName);
      await createCoopModal.waitForClose();

      // Verify buttons are large enough (minimum 44x44px)
      const coopCard = await coopsPage.getCoopCard(coopName);
      const editButton = coopCard.getByRole('button', { name: /edit/i });

      const boundingBox = await editButton.boundingBox();
      expect(boundingBox).not.toBeNull();

      if (boundingBox) {
        expect(boundingBox.width).toBeGreaterThanOrEqual(44);
        expect(boundingBox.height).toBeGreaterThanOrEqual(44);
      }
    });
  });

  test.describe('Tenant Isolation', () => {
    test('should only show coops for authenticated user', async ({ page }) => {
      // Create a coop with unique identifier
      const uniqueCoopName = `Tenant Test ${Date.now()}-${Math.random()}`;

      await coopsPage.openCreateCoopModal();
      await createCoopModal.createCoop(uniqueCoopName);
      await createCoopModal.waitForClose();

      // Verify coop is visible
      await expect(page.getByText(uniqueCoopName)).toBeVisible();

      // Note: Testing that another user CANNOT see this coop requires
      // a second authenticated session, which is typically handled in
      // integration tests or with multiple browser contexts
      // This test documents the requirement
    });
  });
});
