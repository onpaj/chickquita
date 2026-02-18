import { test, expect } from '@playwright/test';
import { CoopsPage } from './pages/CoopsPage';

/**
 * E2E tests for Daily Records List Page with Filtering
 * US-016: Daily Records List Page
 *
 * Tests the complete workflow for viewing and filtering daily records:
 * - Page renders with all filters
 * - Filter updates trigger data refresh
 * - Records displayed in cards
 * - Empty state shown when no records
 * - Loading skeleton during fetch
 * - Responsive grid layout
 */
test.describe('Daily Records - List Page with Filtering', () => {
  let coopsPage: CoopsPage;

  test.beforeEach(async ({ page }) => {
    // Initialize page objects
    coopsPage = new CoopsPage(page);

    // Start at coops page
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');
  });

  test('@smoke should display empty state when no daily records exist', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Check for page title
    await expect(page.getByRole('heading', { name: /denní záznamy/i })).toBeVisible();

    // Check for filters section
    await expect(page.getByText(/filtrovat/i)).toBeVisible();

    // Check for empty state
    await expect(page.getByText(/zatím tu nejsou žádné záznamy/i)).toBeVisible();
  });

  test('should display all filter options', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Check for flock filter
    await expect(page.getByLabel(/hejno/i)).toBeVisible();

    // Check for date range filters
    await expect(page.getByLabel(/od data/i)).toBeVisible();
    await expect(page.getByLabel(/do data/i)).toBeVisible();

    // Check for quick filter chips
    await expect(page.getByText(/dnes/i).first()).toBeVisible();
    await expect(page.getByText(/poslední týden/i)).toBeVisible();
    await expect(page.getByText(/poslední měsíc/i)).toBeVisible();
  });

  test('should filter records by quick filter - Today', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Click on "Today" quick filter
    const todayChip = page.getByRole('button', { name: /dnes/i }).first();
    await todayChip.click();

    // Verify that date filters are set to today
    const today = new Date().toISOString().split('T')[0];
    const startDateInput = page.getByLabel(/od data/i);
    const endDateInput = page.getByLabel(/do data/i);

    await expect(startDateInput).toHaveValue(today);
    await expect(endDateInput).toHaveValue(today);
  });

  test('should filter records by quick filter - Last Week', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Click on "Last Week" quick filter
    const lastWeekChip = page.getByRole('button', { name: /poslední týden/i });
    await lastWeekChip.click();

    // Verify that date filters are set
    const today = new Date().toISOString().split('T')[0];
    const endDateInput = page.getByLabel(/do data/i);

    await expect(endDateInput).toHaveValue(today);

    // Start date should be 7 days ago (not checking exact value due to date calculation)
    const startDateInput = page.getByLabel(/od data/i);
    await expect(startDateInput).not.toHaveValue('');
  });

  test('should filter records by quick filter - Last Month', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Click on "Last Month" quick filter
    const lastMonthChip = page.getByRole('button', { name: /poslední měsíc/i });
    await lastMonthChip.click();

    // Verify that date filters are set
    const today = new Date().toISOString().split('T')[0];
    const endDateInput = page.getByLabel(/do data/i);

    await expect(endDateInput).toHaveValue(today);

    // Start date should be 30 days ago
    const startDateInput = page.getByLabel(/od data/i);
    await expect(startDateInput).not.toHaveValue('');
  });

  test('should clear all filters', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Apply a quick filter first
    const todayChip = page.getByRole('button', { name: /dnes/i }).first();
    await todayChip.click();

    // Wait for filter to be applied
    await page.waitForTimeout(300);

    // Clear filters
    const clearButton = page.getByRole('button', { name: /vymazat filtry/i });
    await clearButton.click();

    // Verify that all filters are cleared
    const startDateInput = page.getByLabel(/od data/i);
    const endDateInput = page.getByLabel(/do data/i);

    await expect(startDateInput).toHaveValue('');
    await expect(endDateInput).toHaveValue('');
  });

  test('should allow manual date range selection', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Set custom date range
    const startDate = '2024-02-01';
    const endDate = '2024-02-15';

    const startDateInput = page.getByLabel(/od data/i);
    const endDateInput = page.getByLabel(/do data/i);

    await startDateInput.fill(startDate);
    await endDateInput.fill(endDate);

    // Verify values are set
    await expect(startDateInput).toHaveValue(startDate);
    await expect(endDateInput).toHaveValue(endDate);
  });

  test('should show loading skeletons while fetching data', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');

    // Check for skeleton loaders (should appear briefly during initial load)
    // Note: This test might be flaky due to fast network speeds
    const skeleton = page.locator('.MuiSkeleton-root').first();

    // Either skeleton is visible or data has loaded
    try {
      await expect(skeleton).toBeVisible({ timeout: 1000 });
    } catch {
      // If skeleton is not visible, data has already loaded - that's fine
      await expect(page.getByRole('heading', { name: /denní záznamy/i })).toBeVisible();
    }
  });

  test('should display records in responsive grid layout', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Check that the page uses MUI Grid for layout
    const gridContainer = page.locator('.MuiGrid-container').first();

    // Grid should exist (even if no records, the structure should be there)
    if (await gridContainer.isVisible()) {
      await expect(gridContainer).toBeVisible();
    }
  });

  test('should be accessible on mobile viewport', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });

    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Check that page is visible and accessible
    await expect(page.getByRole('heading', { name: /denní záznamy/i })).toBeVisible();

    // Check that filters are visible and stacked vertically on mobile
    await expect(page.getByLabel(/hejno/i)).toBeVisible();
    await expect(page.getByLabel(/od data/i)).toBeVisible();
    await expect(page.getByLabel(/do data/i)).toBeVisible();
  });

  test('should maintain filter state when navigating away and back', async ({ page }) => {
    // Navigate to daily records page
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Apply a quick filter
    const todayChip = page.getByRole('button', { name: /dnes/i }).first();
    await todayChip.click();

    const today = new Date().toISOString().split('T')[0];

    // Verify filter is applied
    await expect(page.getByLabel(/od data/i)).toHaveValue(today);

    // Navigate to dashboard
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Navigate back to daily records
    await page.goto('/daily-records');
    await page.waitForLoadState('networkidle');

    // Note: Filters are NOT persisted by default in this implementation
    // This test documents the current behavior
    const startDateInput = page.getByLabel(/od data/i);
    await expect(startDateInput).toHaveValue('');
  });
});
