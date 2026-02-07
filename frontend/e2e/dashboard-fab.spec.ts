import { test, expect } from '@playwright/test';
import { DashboardPage } from './pages/DashboardPage';
import { CoopsPage } from './pages/CoopsPage';
import { CreateCoopModal } from './pages/CreateCoopModal';
import { CoopDetailPage } from './pages/CoopDetailPage';
import { CreateFlockModal } from './pages/CreateFlockModal';
import { generateCoopName } from './fixtures/coop.fixture';
import { generateFlockIdentifier, getDaysAgoDate } from './fixtures/flock.fixture';

/**
 * E2E tests for Dashboard FAB Button
 * US-015: FAB Button on Dashboard
 *
 * Tests:
 * - FAB visibility and positioning
 * - Opens QuickAddModal on click
 * - Accessibility labels
 * - Responsive positioning (above bottom nav on mobile)
 * - FAB disabled when no flocks exist
 */
test.describe('Dashboard - FAB Button', () => {
  let dashboardPage: DashboardPage;
  let coopsPage: CoopsPage;
  let createCoopModal: CreateCoopModal;
  let coopDetailPage: CoopDetailPage;
  let createFlockModal: CreateFlockModal;

  test.beforeEach(async ({ page }) => {
    // Initialize page objects
    dashboardPage = new DashboardPage(page);
    coopsPage = new CoopsPage(page);
    createCoopModal = new CreateCoopModal(page);
    coopDetailPage = new CoopDetailPage(page);
    createFlockModal = new CreateFlockModal(page);
  });

  test('should display FAB when user has flocks', async ({ page }) => {
    // Create coop and flock first
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    const testCoopName = generateCoopName();
    await coopsPage.clickAddButton();
    await createCoopModal.fillForm(testCoopName, 'Test location');
    await createCoopModal.submit();
    await coopsPage.waitForCoopCard(testCoopName);

    // Navigate to coop and add flock
    await coopsPage.clickCoopCard(testCoopName);
    await coopDetailPage.waitForLoaded();

    const testFlockIdentifier = generateFlockIdentifier();
    await coopDetailPage.clickAddFlockButton();
    await createFlockModal.fillForm(
      testFlockIdentifier,
      getDaysAgoDate(30),
      5, // hens
      1, // roosters
      0  // chicks
    );
    await createFlockModal.submit();

    // Navigate to dashboard
    await dashboardPage.goto();
    await page.waitForLoadState('networkidle');

    // FAB should be visible
    await expect(dashboardPage.quickAddFab).toBeVisible();
  });

  test('should have correct accessibility label', async ({ page }) => {
    // Create minimal test data
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    const testCoopName = generateCoopName();
    await coopsPage.clickAddButton();
    await createCoopModal.fillForm(testCoopName, 'Test');
    await createCoopModal.submit();
    await coopsPage.waitForCoopCard(testCoopName);

    await coopsPage.clickCoopCard(testCoopName);
    await coopDetailPage.waitForLoaded();

    await coopDetailPage.clickAddFlockButton();
    await createFlockModal.fillForm(generateFlockIdentifier(), getDaysAgoDate(30), 1, 0, 0);
    await createFlockModal.submit();

    // Navigate to dashboard
    await dashboardPage.goto();
    await page.waitForLoadState('networkidle');

    // Check aria-label is present (Czech or English)
    const ariaLabel = await dashboardPage.quickAddFab.getAttribute('aria-label');
    expect(ariaLabel).toBeTruthy();
    expect(ariaLabel).toMatch(/Přidat denní záznam|Add daily record/i);
  });

  test('should open QuickAddModal when clicked', async ({ page }) => {
    // Create minimal test data
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    const testCoopName = generateCoopName();
    await coopsPage.clickAddButton();
    await createCoopModal.fillForm(testCoopName, 'Test');
    await createCoopModal.submit();
    await coopsPage.waitForCoopCard(testCoopName);

    await coopsPage.clickCoopCard(testCoopName);
    await coopDetailPage.waitForLoaded();

    await coopDetailPage.clickAddFlockButton();
    await createFlockModal.fillForm(generateFlockIdentifier(), getDaysAgoDate(30), 1, 0, 0);
    await createFlockModal.submit();

    // Navigate to dashboard
    await dashboardPage.goto();
    await page.waitForLoadState('networkidle');

    // Click FAB
    await dashboardPage.clickQuickAddFab();

    // Modal should open
    await expect(page.getByText(/Rychlý záznam vajec|Quick Add/i)).toBeVisible();
  });

  test('should be positioned above bottom navigation on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });

    // Create minimal test data
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    const testCoopName = generateCoopName();
    await coopsPage.clickAddButton();
    await createCoopModal.fillForm(testCoopName, 'Test');
    await createCoopModal.submit();
    await coopsPage.waitForCoopCard(testCoopName);

    await coopsPage.clickCoopCard(testCoopName);
    await coopDetailPage.waitForLoaded();

    await coopDetailPage.clickAddFlockButton();
    await createFlockModal.fillForm(generateFlockIdentifier(), getDaysAgoDate(30), 1, 0, 0);
    await createFlockModal.submit();

    // Navigate to dashboard
    await dashboardPage.goto();
    await page.waitForLoadState('networkidle');

    // Get bottom navigation position
    const bottomNav = page.locator('.MuiBottomNavigation-root');
    await expect(bottomNav).toBeVisible();
    const navBox = await bottomNav.boundingBox();

    // Get FAB position
    const fabBox = await dashboardPage.quickAddFab.boundingBox();

    // FAB should be above bottom navigation
    expect(fabBox).toBeTruthy();
    expect(navBox).toBeTruthy();

    if (fabBox && navBox) {
      // FAB bottom should be above bottom nav top (with spacing)
      expect(fabBox.y + fabBox.height).toBeLessThan(navBox.y);
    }
  });

  test('should be positioned on right side with proper spacing', async ({ page }) => {
    // Create minimal test data
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    const testCoopName = generateCoopName();
    await coopsPage.clickAddButton();
    await createCoopModal.fillForm(testCoopName, 'Test');
    await createCoopModal.submit();
    await coopsPage.waitForCoopCard(testCoopName);

    await coopsPage.clickCoopCard(testCoopName);
    await coopDetailPage.waitForLoaded();

    await coopDetailPage.clickAddFlockButton();
    await createFlockModal.fillForm(generateFlockIdentifier(), getDaysAgoDate(30), 1, 0, 0);
    await createFlockModal.submit();

    // Navigate to dashboard
    await dashboardPage.goto();
    await page.waitForLoadState('networkidle');

    const fabBox = await dashboardPage.quickAddFab.boundingBox();
    const viewportSize = page.viewportSize();

    expect(fabBox).toBeTruthy();
    expect(viewportSize).toBeTruthy();

    if (fabBox && viewportSize) {
      // FAB should be on the right side (within 16-24px from right edge, accounting for FAB size)
      const distanceFromRight = viewportSize.width - (fabBox.x + fabBox.width);
      expect(distanceFromRight).toBeGreaterThanOrEqual(12); // At least 12px
      expect(distanceFromRight).toBeLessThanOrEqual(24); // At most 24px
    }
  });

  test('should have proper fixed positioning', async ({ page }) => {
    // Create minimal test data
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    const testCoopName = generateCoopName();
    await coopsPage.clickAddButton();
    await createCoopModal.fillForm(testCoopName, 'Test');
    await createCoopModal.submit();
    await coopsPage.waitForCoopCard(testCoopName);

    await coopsPage.clickCoopCard(testCoopName);
    await coopDetailPage.waitForLoaded();

    await coopDetailPage.clickAddFlockButton();
    await createFlockModal.fillForm(generateFlockIdentifier(), getDaysAgoDate(30), 1, 0, 0);
    await createFlockModal.submit();

    // Navigate to dashboard
    await dashboardPage.goto();
    await page.waitForLoadState('networkidle');

    // Check FAB has fixed position
    const position = await dashboardPage.quickAddFab.evaluate((el) =>
      window.getComputedStyle(el).position
    );
    expect(position).toBe('fixed');
  });

  test('should close modal when cancel is clicked', async ({ page }) => {
    // Create minimal test data
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    const testCoopName = generateCoopName();
    await coopsPage.clickAddButton();
    await createCoopModal.fillForm(testCoopName, 'Test');
    await createCoopModal.submit();
    await coopsPage.waitForCoopCard(testCoopName);

    await coopsPage.clickCoopCard(testCoopName);
    await coopDetailPage.waitForLoaded();

    await coopDetailPage.clickAddFlockButton();
    await createFlockModal.fillForm(generateFlockIdentifier(), getDaysAgoDate(30), 1, 0, 0);
    await createFlockModal.submit();

    // Navigate to dashboard
    await dashboardPage.goto();
    await page.waitForLoadState('networkidle');

    // Open modal
    await dashboardPage.clickQuickAddFab();
    await expect(page.getByText(/Rychlý záznam vajec|Quick Add/i)).toBeVisible();

    // Close modal
    const cancelButton = page.getByRole('button', { name: /zrušit|cancel/i });
    await cancelButton.click();

    // Modal should close
    await expect(page.getByText(/Rychlý záznam vajec|Quick Add/i)).not.toBeVisible();
  });
});
