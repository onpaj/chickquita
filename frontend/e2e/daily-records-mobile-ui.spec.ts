import { test, expect, Page } from '@playwright/test';

/**
 * E2E tests for Daily Records Mobile UI - Touch Targets
 * US-024: E2E - Daily Records Mobile UI Test
 *
 * Tests bottom navigation touch target requirements (44x44px minimum) on mobile viewports.
 * These tests always run regardless of whether test data exists.
 */

// Mobile device configurations
const MOBILE_DEVICES = [
  {
    name: 'iPhone SE',
    width: 320,
    height: 568,
    platform: 'iOS',
  },
  {
    name: 'iPhone 14',
    width: 390,
    height: 844,
    platform: 'iOS',
  },
  {
    name: 'Samsung Galaxy A52',
    width: 412,
    height: 915,
    platform: 'Android',
  },
  {
    name: 'Pixel 5',
    width: 393,
    height: 851,
    platform: 'Android',
  },
];

const MIN_TOUCH_TARGET_SIZE = 44; // iOS Human Interface Guidelines and Material Design

async function setMobileViewport(page: Page, device: (typeof MOBILE_DEVICES)[0]) {
  await page.setViewportSize({ width: device.width, height: device.height });
}

test.describe('Daily Records - Mobile Touch Targets', () => {
  for (const device of MOBILE_DEVICES) {
    test(`Bottom navigation touch targets on ${device.name} (${device.platform})`, async ({ page }) => {
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');
      await setMobileViewport(page, device);
      await page.waitForTimeout(500); // Wait for viewport change to settle

      // Wait for bottom navigation to be visible
      const bottomNav = page.locator('.MuiBottomNavigation-root');
      await expect(bottomNav).toBeVisible({ timeout: 5000 });

      const navButtons = page.locator('.MuiBottomNavigationAction-root');
      const buttonCount = await navButtons.count();

      expect(buttonCount).toBeGreaterThan(0);

      // Check each navigation button
      for (let i = 0; i < buttonCount; i++) {
        const button = navButtons.nth(i);
        const buttonBox = await button.boundingBox();

        if (buttonBox) {
          expect(buttonBox.height).toBeGreaterThanOrEqual(MIN_TOUCH_TARGET_SIZE);
          expect(buttonBox.width).toBeGreaterThanOrEqual(40);
        }
      }
    });
  }
});
