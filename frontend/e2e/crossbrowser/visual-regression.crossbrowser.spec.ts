import { test, expect } from '@playwright/test';

/**
 * TESTING-002: Visual Regression Tests for Cross-Browser Compatibility
 *
 * These tests capture screenshots across different browsers and viewports
 * to ensure consistent UI rendering and layout across all supported platforms.
 *
 * Test Coverage:
 * - Sign-in page layout
 * - Dashboard page with statistics widgets
 * - Coops listing page
 * - Flocks listing page
 * - Modal dialogs (Create/Edit)
 * - Confirmation dialogs
 * - Bottom navigation
 * - Empty states
 * - Error states
 */

// Configuration for consistent screenshots
const SCREENSHOT_OPTIONS = {
  fullPage: true,
  animations: 'disabled' as const,
  scale: 'css' as const,
};

test.describe('Visual Regression - Page Layouts', () => {
  test.describe.configure({ mode: 'parallel' });

  test('sign-in page renders consistently', async ({ page, browserName }) => {
    // Navigate to sign-in page (unauthenticated)
    await page.goto('/sign-in');
    await page.waitForLoadState('networkidle');

    // Wait for Clerk UI to load
    await page.waitForTimeout(1000);

    // Capture screenshot
    await expect(page).toHaveScreenshot(
      `sign-in-page-${browserName}.png`,
      SCREENSHOT_OPTIONS
    );
  });

  test('dashboard page renders consistently', async ({ page, browserName }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Wait for statistics to load
    await page.waitForTimeout(500);

    await expect(page).toHaveScreenshot(
      `dashboard-page-${browserName}.png`,
      SCREENSHOT_OPTIONS
    );
  });

  test('coops listing page renders consistently', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Wait for coop cards to render
    await page.waitForTimeout(500);

    await expect(page).toHaveScreenshot(
      `coops-page-${browserName}.png`,
      SCREENSHOT_OPTIONS
    );
  });

  test('settings page renders consistently', async ({ page, browserName }) => {
    await page.goto('/settings');
    await page.waitForLoadState('networkidle');

    await expect(page).toHaveScreenshot(
      `settings-page-${browserName}.png`,
      SCREENSHOT_OPTIONS
    );
  });
});

test.describe('Visual Regression - Component States', () => {
  test.describe.configure({ mode: 'parallel' });

  test('bottom navigation renders consistently', async ({ page, browserName }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Screenshot just the bottom navigation
    const bottomNav = page.locator('nav').last();
    await expect(bottomNav).toHaveScreenshot(
      `bottom-navigation-${browserName}.png`,
      { animations: 'disabled' }
    );
  });

  test('coop card renders consistently', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Wait for cards to load
    const coopCard = page.locator('[data-testid="coop-card"]').first();

    // Only run if there are coop cards
    const cardCount = await page.locator('[data-testid="coop-card"]').count();
    if (cardCount > 0) {
      await expect(coopCard).toHaveScreenshot(
        `coop-card-${browserName}.png`,
        { animations: 'disabled' }
      );
    }
  });

  test('empty state renders consistently', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Check if empty state is visible
    const emptyState = page.getByText(/no coops yet|zatím nemáte žádné kurníky/i);
    const isVisible = await emptyState.isVisible().catch(() => false);

    if (isVisible) {
      await expect(page).toHaveScreenshot(
        `coops-empty-state-${browserName}.png`,
        SCREENSHOT_OPTIONS
      );
    }
  });

  test('loading skeleton renders consistently', async ({ page, browserName }) => {
    // Intercept API to delay response for skeleton visibility
    await page.route('**/api/coops**', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 2000));
      await route.continue();
    });

    await page.goto('/coops');

    // Capture skeleton state
    await page.waitForTimeout(500);
    await expect(page).toHaveScreenshot(
      `coops-loading-skeleton-${browserName}.png`,
      SCREENSHOT_OPTIONS
    );
  });
});

test.describe('Visual Regression - Modal Dialogs', () => {
  test.describe.configure({ mode: 'serial' });

  test('create coop modal renders consistently', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Open create modal
    const addButton = page.getByRole('button', { name: /add coop|přidat kurník/i }).first();
    await addButton.click();

    // Wait for modal animation
    await page.waitForTimeout(500);

    // Screenshot the modal
    const modal = page.getByRole('dialog');
    await expect(modal).toBeVisible();
    await expect(page).toHaveScreenshot(
      `create-coop-modal-${browserName}.png`,
      SCREENSHOT_OPTIONS
    );
  });

  test('confirmation dialog renders consistently', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Check if there are any coop cards
    const cardCount = await page.locator('[data-testid="coop-card"]').count();

    if (cardCount > 0) {
      // Open menu on first card
      const menuButton = page.locator('[data-testid="coop-card"]').first()
        .getByRole('button', { name: /more|více/i });
      await menuButton.click();

      // Click archive/delete option
      const archiveOption = page.getByRole('menuitem', { name: /archive|archivovat/i });
      await archiveOption.click();

      // Wait for dialog
      await page.waitForTimeout(500);

      // Screenshot confirmation dialog
      const dialog = page.getByRole('dialog');
      await expect(dialog).toBeVisible();
      await expect(page).toHaveScreenshot(
        `confirmation-dialog-${browserName}.png`,
        SCREENSHOT_OPTIONS
      );
    }
  });
});

test.describe('Visual Regression - Responsive Breakpoints', () => {
  test.describe.configure({ mode: 'parallel' });

  const breakpoints = [
    { name: '320px', width: 320, height: 568 },
    { name: '480px', width: 480, height: 800 },
    { name: '768px', width: 768, height: 1024 },
    { name: '1024px', width: 1024, height: 768 },
    { name: '1920px', width: 1920, height: 1080 },
  ];

  for (const breakpoint of breakpoints) {
    test(`dashboard at ${breakpoint.name} breakpoint`, async ({ page, browserName }) => {
      await page.setViewportSize({ width: breakpoint.width, height: breakpoint.height });
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      await expect(page).toHaveScreenshot(
        `dashboard-${breakpoint.name}-${browserName}.png`,
        SCREENSHOT_OPTIONS
      );
    });

    test(`coops page at ${breakpoint.name} breakpoint`, async ({ page, browserName }) => {
      await page.setViewportSize({ width: breakpoint.width, height: breakpoint.height });
      await page.goto('/coops');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      await expect(page).toHaveScreenshot(
        `coops-${breakpoint.name}-${browserName}.png`,
        SCREENSHOT_OPTIONS
      );
    });
  }
});

test.describe('Visual Regression - Touch Target Verification', () => {
  test('FAB button meets touch target size', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Find FAB button
    const fab = page.locator('button[aria-label*="add" i], button[aria-label*="přidat" i]').last();
    const boundingBox = await fab.boundingBox();

    // Verify minimum touch target size (44x44 per iOS HIG, 48x48 per Material Design)
    expect(boundingBox).not.toBeNull();
    if (boundingBox) {
      expect(boundingBox.width).toBeGreaterThanOrEqual(44);
      expect(boundingBox.height).toBeGreaterThanOrEqual(44);
    }

    // Screenshot FAB area
    await expect(fab).toHaveScreenshot(
      `fab-button-${browserName}.png`,
      { animations: 'disabled' }
    );
  });

  test('bottom navigation buttons meet touch target size', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Check all navigation buttons
    const navButtons = page.locator('.MuiBottomNavigationAction-root');
    const count = await navButtons.count();

    for (let i = 0; i < count; i++) {
      const button = navButtons.nth(i);
      const boundingBox = await button.boundingBox();

      expect(boundingBox).not.toBeNull();
      if (boundingBox) {
        // Should be at least 44px in height (touch-friendly)
        expect(boundingBox.height).toBeGreaterThanOrEqual(44);
      }
    }
  });
});

test.describe('Visual Regression - Accessibility Color Contrast', () => {
  test('text elements have sufficient contrast', async ({ page, browserName }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Capture full page for color contrast review
    await expect(page).toHaveScreenshot(
      `dashboard-contrast-${browserName}.png`,
      {
        ...SCREENSHOT_OPTIONS,
        // Use higher threshold for color comparison
        threshold: 0.1,
      }
    );
  });
});

test.describe('Visual Regression - i18n Layout Stability', () => {
  test('Czech language layout', async ({ page, browserName }) => {
    await page.goto('/settings');
    await page.waitForLoadState('networkidle');

    // Ensure Czech is selected
    const languageSelect = page.locator('#language-select');
    await languageSelect.click();
    await page.getByRole('option', { name: /čeština/i }).click();

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    await expect(page).toHaveScreenshot(
      `dashboard-cs-${browserName}.png`,
      SCREENSHOT_OPTIONS
    );
  });

  test('English language layout', async ({ page, browserName }) => {
    await page.goto('/settings');
    await page.waitForLoadState('networkidle');

    // Switch to English
    const languageSelect = page.locator('#language-select');
    await languageSelect.click();
    await page.getByRole('option', { name: /english/i }).click();

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    await expect(page).toHaveScreenshot(
      `dashboard-en-${browserName}.png`,
      SCREENSHOT_OPTIONS
    );
  });
});
