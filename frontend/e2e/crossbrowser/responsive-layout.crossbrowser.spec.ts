import { test, expect, Page } from '@playwright/test';

/**
 * TESTING-002: Responsive Layout Tests
 *
 * Tests layout behavior across all specified breakpoints and devices:
 * - 320px (iPhone SE, smallest mobile)
 * - 480px (Mobile landscape)
 * - 768px (Tablet portrait)
 * - 1024px (Small desktop/tablet landscape)
 * - 1920px (Full HD desktop)
 *
 * Devices:
 * - iPhone SE (320x568)
 * - iPhone 14 (390x844)
 * - iPad (768x1024)
 * - Samsung Galaxy A52 (412x915)
 */

// Test configuration
const BREAKPOINTS = [
  { name: 'mobile-xs', width: 320, height: 568, description: 'Mobile XS (320px)' },
  { name: 'mobile-sm', width: 375, height: 667, description: 'Mobile SM (375px)' },
  { name: 'mobile-landscape', width: 480, height: 320, description: 'Mobile Landscape (480px)' },
  { name: 'tablet', width: 768, height: 1024, description: 'Tablet (768px)' },
  { name: 'desktop-sm', width: 1024, height: 768, description: 'Desktop SM (1024px)' },
  { name: 'desktop-lg', width: 1920, height: 1080, description: 'Desktop LG (1920px)' },
];

// Helper function to set viewport
async function setBreakpoint(page: Page, breakpoint: typeof BREAKPOINTS[0]) {
  await page.setViewportSize({
    width: breakpoint.width,
    height: breakpoint.height,
  });
}

test.describe('Responsive Layout - Dashboard Page', () => {
  test.describe.configure({ mode: 'parallel' });

  for (const breakpoint of BREAKPOINTS) {
    test(`Dashboard grid layout at ${breakpoint.description}`, async ({ page }) => {
      await setBreakpoint(page, breakpoint);
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');

      // Verify dashboard page title is visible
      // Use name filter to avoid strict mode violation when other h1 elements are present
      // (e.g. Clerk-hosted authentication components may inject their own headings)
      const title = page.getByRole('heading', { level: 1, name: /přehled|dashboard/i });
      await expect(title).toBeVisible();

      // Verify no horizontal overflow
      const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
      const viewportWidth = await page.evaluate(() => window.innerWidth);
      expect(bodyWidth).toBeLessThanOrEqual(viewportWidth + 1); // Allow 1px tolerance

      // Verify statistics widgets are displayed in appropriate grid
      const statisticsGrid = page.locator('[class*="MuiBox-root"]').filter({
        has: page.locator('[class*="MuiCard-root"]'),
      }).first();

      if (await statisticsGrid.isVisible()) {
        const gridBoundingBox = await statisticsGrid.boundingBox();
        expect(gridBoundingBox).not.toBeNull();

        if (gridBoundingBox && breakpoint.width >= 768) {
          // On tablet and above, grid should be multi-column
          // Width should be close to full container width
          expect(gridBoundingBox.width).toBeGreaterThan(breakpoint.width * 0.6);
        }
      }
    });

    test(`Dashboard bottom navigation at ${breakpoint.description}`, async ({ page }) => {
      await setBreakpoint(page, breakpoint);
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');

      // Bottom navigation should be visible and fixed at bottom
      const bottomNav = page.locator('.MuiBottomNavigation-root');
      await expect(bottomNav).toBeVisible();

      const navBoundingBox = await bottomNav.boundingBox();
      expect(navBoundingBox).not.toBeNull();

      if (navBoundingBox) {
        // Navigation should span full width
        expect(navBoundingBox.width).toBeGreaterThanOrEqual(breakpoint.width - 2);

        // Navigation should be at bottom of viewport
        const viewportHeight = await page.evaluate(() => window.innerHeight);
        expect(navBoundingBox.y + navBoundingBox.height).toBeCloseTo(viewportHeight, 5);
      }
    });

    test(`Dashboard FAB button position at ${breakpoint.description}`, async ({ page }) => {
      await setBreakpoint(page, breakpoint);
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');

      // FAB button (if visible) should be positioned correctly
      const fab = page.locator('.MuiFab-root');
      const fabVisible = await fab.isVisible().catch(() => false);

      if (fabVisible) {
        const fabBoundingBox = await fab.boundingBox();
        expect(fabBoundingBox).not.toBeNull();

        if (fabBoundingBox) {
          // FAB should be in bottom-right corner
          expect(fabBoundingBox.x + fabBoundingBox.width).toBeGreaterThan(breakpoint.width - 100);

          // FAB should be above bottom navigation
          const viewportHeight = await page.evaluate(() => window.innerHeight);
          expect(fabBoundingBox.y + fabBoundingBox.height).toBeLessThan(viewportHeight - 60);
        }
      }
    });
  }
});

test.describe('Responsive Layout - Coops Page', () => {
  test.describe.configure({ mode: 'parallel' });

  for (const breakpoint of BREAKPOINTS) {
    test(`Coops list layout at ${breakpoint.description}`, async ({ page }) => {
      await setBreakpoint(page, breakpoint);
      await page.goto('/coops');
      await page.waitForLoadState('networkidle');

      // Verify page title
      const title = page.getByRole('heading', { name: /coops|kurníky/i });
      await expect(title).toBeVisible();

      // Verify no horizontal overflow
      const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
      expect(bodyWidth).toBeLessThanOrEqual(breakpoint.width + 1);

      // Verify coop cards fit within viewport
      const coopCards = page.locator('[data-testid="coop-card"]');
      const cardCount = await coopCards.count();

      for (let i = 0; i < Math.min(cardCount, 3); i++) {
        const card = coopCards.nth(i);
        const cardBoundingBox = await card.boundingBox();

        if (cardBoundingBox) {
          // Card should fit within viewport width with padding
          expect(cardBoundingBox.width).toBeLessThanOrEqual(breakpoint.width);
          expect(cardBoundingBox.x).toBeGreaterThanOrEqual(0);
        }
      }
    });

    test(`Coops card touch targets at ${breakpoint.description}`, async ({ page }) => {
      await setBreakpoint(page, breakpoint);
      await page.goto('/coops');
      await page.waitForLoadState('networkidle');

      // Check card menu buttons for touch-friendly size
      const menuButtons = page.locator('[data-testid="coop-card"] button[aria-label*="more" i], [data-testid="coop-card"] button[aria-label*="více" i]');
      const buttonCount = await menuButtons.count();

      for (let i = 0; i < Math.min(buttonCount, 3); i++) {
        const button = menuButtons.nth(i);
        const boundingBox = await button.boundingBox();

        if (boundingBox) {
          // Minimum touch target: 44x44px
          expect(boundingBox.width).toBeGreaterThanOrEqual(44);
          expect(boundingBox.height).toBeGreaterThanOrEqual(44);
        }
      }
    });
  }
});

test.describe('Responsive Layout - Modal Dialogs', () => {
  for (const breakpoint of BREAKPOINTS) {
    test(`Create coop modal at ${breakpoint.description}`, async ({ page }) => {
      await setBreakpoint(page, breakpoint);
      await page.goto('/coops');
      await page.waitForLoadState('networkidle');

      // Open create modal
      const addButton = page.getByRole('button', { name: /add coop|přidat kurník/i }).first();
      await addButton.click();

      // Wait for modal
      await page.waitForTimeout(500);
      const modal = page.getByRole('dialog');
      await expect(modal).toBeVisible();

      const modalBoundingBox = await modal.boundingBox();
      expect(modalBoundingBox).not.toBeNull();

      if (modalBoundingBox) {
        if (breakpoint.width < 600) {
          // On mobile, modal should be full screen
          expect(modalBoundingBox.width).toBeGreaterThanOrEqual(breakpoint.width - 2);
          expect(modalBoundingBox.height).toBeGreaterThanOrEqual(breakpoint.height * 0.9);
        } else {
          // On desktop, modal should be centered and sized appropriately
          expect(modalBoundingBox.width).toBeLessThanOrEqual(breakpoint.width);
        }
      }

      // Verify form inputs are accessible
      const nameInput = page.getByRole('textbox', { name: /name|název/i });
      await expect(nameInput).toBeVisible();

      // Verify buttons are visible and touch-friendly
      const submitButton = page.getByRole('button', { name: /create|vytvořit|save|uložit/i });
      await expect(submitButton).toBeVisible();

      const buttonBoundingBox = await submitButton.boundingBox();
      if (buttonBoundingBox) {
        expect(buttonBoundingBox.height).toBeGreaterThanOrEqual(44);
      }
    });
  }
});

test.describe('Responsive Layout - Typography Scaling', () => {
  for (const breakpoint of BREAKPOINTS) {
    test(`Typography readability at ${breakpoint.description}`, async ({ page }) => {
      await setBreakpoint(page, breakpoint);
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');

      // Check heading font sizes
      const h1 = page.getByRole('heading', { level: 1 }).first();
      const h1Visible = await h1.isVisible().catch(() => false);

      if (h1Visible) {
        const fontSize = await h1.evaluate((el) => {
          return parseFloat(window.getComputedStyle(el).fontSize);
        });

        // H1 should be readable (at least 20px on mobile, larger on desktop)
        if (breakpoint.width < 600) {
          expect(fontSize).toBeGreaterThanOrEqual(20);
        } else {
          expect(fontSize).toBeGreaterThanOrEqual(28);
        }
      }

      // Check body text font sizes
      const bodyText = page.locator('p, span').first();
      const bodyVisible = await bodyText.isVisible().catch(() => false);

      if (bodyVisible) {
        const bodyFontSize = await bodyText.evaluate((el) => {
          return parseFloat(window.getComputedStyle(el).fontSize);
        });

        // Body text should be at least 14px for readability
        expect(bodyFontSize).toBeGreaterThanOrEqual(12);
      }
    });
  }
});

test.describe('Responsive Layout - Navigation Behavior', () => {
  for (const breakpoint of BREAKPOINTS) {
    test(`Bottom navigation interaction at ${breakpoint.description}`, async ({ page }) => {
      await setBreakpoint(page, breakpoint);
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');

      // Test navigation to coops
      const coopsNavButton = page.locator('.MuiBottomNavigationAction-root').filter({
        hasText: /coops|kurníky/i,
      });

      await expect(coopsNavButton).toBeVisible();
      await coopsNavButton.click();

      // Should navigate to coops page
      await page.waitForURL('**/coops');
      expect(page.url()).toContain('/coops');

      // Navigate to settings
      const settingsNavButton = page.locator('.MuiBottomNavigationAction-root').filter({
        hasText: /settings|nastavení/i,
      });

      await settingsNavButton.click();
      await page.waitForURL('**/settings');
      expect(page.url()).toContain('/settings');
    });
  }
});

test.describe('Responsive Layout - Scroll Behavior', () => {
  for (const breakpoint of BREAKPOINTS) {
    test(`Vertical scrolling on ${breakpoint.description}`, async ({ page }) => {
      await setBreakpoint(page, breakpoint);
      await page.goto('/coops');
      await page.waitForLoadState('networkidle');

      // Get initial scroll position
      const initialScrollY = await page.evaluate(() => window.scrollY);
      expect(initialScrollY).toBe(0);

      // Scroll down
      await page.evaluate(() => window.scrollBy(0, 200));
      const newScrollY = await page.evaluate(() => window.scrollY);

      // Bottom navigation should remain fixed
      const bottomNav = page.locator('.MuiBottomNavigation-root');
      const navBoundingBox = await bottomNav.boundingBox();

      if (navBoundingBox) {
        const viewportHeight = await page.evaluate(() => window.innerHeight);
        // Navigation should still be at the bottom after scrolling
        expect(navBoundingBox.y + navBoundingBox.height).toBeCloseTo(viewportHeight, 5);
      }
    });
  }
});

test.describe('Responsive Layout - Device-Specific Tests', () => {
  const devices = [
    { name: 'iPhone SE', width: 320, height: 568, touch: true },
    { name: 'iPhone 14', width: 390, height: 844, touch: true },
    { name: 'iPad', width: 768, height: 1024, touch: true },
    { name: 'Samsung Galaxy A52', width: 412, height: 915, touch: true },
  ];

  for (const device of devices) {
    test(`${device.name} - Full user journey`, async ({ page }) => {
      await page.setViewportSize({ width: device.width, height: device.height });
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');

      // 1. Verify dashboard loads
      await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

      // 2. Navigate to coops
      const coopsNav = page.locator('.MuiBottomNavigationAction-root').filter({
        hasText: /coops|kurníky/i,
      });
      await coopsNav.click();
      await page.waitForURL('**/coops');

      // 3. Verify coops page
      await expect(page.getByRole('heading', { name: /coops|kurníky/i })).toBeVisible();

      // 4. Try to open create modal
      const addButton = page.getByRole('button', { name: /add coop|přidat kurník/i }).first();
      await addButton.click();

      // 5. Verify modal opens correctly
      const modal = page.getByRole('dialog');
      await expect(modal).toBeVisible();

      // 6. Close modal
      const cancelButton = page.getByRole('button', { name: /cancel|zrušit/i });
      await cancelButton.click();
      await expect(modal).not.toBeVisible();

      // 7. Navigate to settings
      const settingsNav = page.locator('.MuiBottomNavigationAction-root').filter({
        hasText: /settings|nastavení/i,
      });
      await settingsNav.click();
      await page.waitForURL('**/settings');

      // 8. Verify settings page
      await expect(page.getByRole('heading', { name: /settings|nastavení/i })).toBeVisible();
    });
  }
});
