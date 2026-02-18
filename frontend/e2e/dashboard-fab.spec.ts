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
 * - Accessibility label
 * - Opens QuickAddModal on click
 */
test.describe('Dashboard - FAB Button', () => {
  let dashboardPage: DashboardPage;
  let coopsPage: CoopsPage;
  let createCoopModal: CreateCoopModal;
  let coopDetailPage: CoopDetailPage;
  let createFlockModal: CreateFlockModal;

  test.beforeEach(async ({ page }) => {
    dashboardPage = new DashboardPage(page);
    coopsPage = new CoopsPage(page);
    createCoopModal = new CreateCoopModal(page);
    coopDetailPage = new CoopDetailPage(page);
    createFlockModal = new CreateFlockModal(page);
  });

  async function createCoopWithFlock(page: import('@playwright/test').Page) {
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
  }

  test('should have correct accessibility label', async ({ page }) => {
    await createCoopWithFlock(page);

    await dashboardPage.goto();
    await page.waitForLoadState('networkidle');

    const ariaLabel = await dashboardPage.quickAddFab.getAttribute('aria-label');
    expect(ariaLabel).toBeTruthy();
    expect(ariaLabel).toMatch(/Přidat denní záznam|Add daily record/i);
  });

  test('@smoke should open QuickAddModal when clicked', async ({ page }) => {
    await createCoopWithFlock(page);

    await dashboardPage.goto();
    await page.waitForLoadState('networkidle');

    await dashboardPage.clickQuickAddFab();

    await expect(page.getByText(/Rychlý záznam vajec|Quick Add/i)).toBeVisible();
  });
});
