import { test, expect, Page } from '@playwright/test';

/**
 * E2E tests for Daily Records Mobile UI
 * US-024: E2E - Daily Records Mobile UI Test
 *
 * Tests mobile-specific UI behavior for daily records:
 * - FAB positioning on mobile viewports
 * - Modal fullscreen behavior on mobile
 * - Touch target size requirements (44x44px minimum)
 * - Tests on both iOS and Android viewports
 *
 * Note: These tests assume that test data (coops and flocks) already exists
 * from previous test runs or setup. They focus on UI behavior validation.
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

test.describe('Daily Records - Mobile UI Tests', () => {
  test.describe('FAB Positioning on Mobile', () => {
    for (const device of MOBILE_DEVICES) {
      test(`FAB positioning on ${device.name} (${device.platform})`, async ({ page }) => {
        await setMobileViewport(page, device);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        // Locate FAB (may or may not be visible depending on data)
        const fab = page.locator('[aria-label*="Přidat denní záznam"]');
        const fabVisible = await fab.isVisible().catch(() => false);

        if (fabVisible) {
          const fabBox = await fab.boundingBox();
          expect(fabBox).not.toBeNull();

          if (fabBox) {
            // FAB should be in bottom-right corner (within 100px of right edge)
            const distanceFromRight = device.width - (fabBox.x + fabBox.width);
            expect(distanceFromRight).toBeGreaterThanOrEqual(0);
            expect(distanceFromRight).toBeLessThanOrEqual(100);

            // FAB should be above bottom navigation (60-90px from bottom)
            const viewportHeight = await page.evaluate(() => window.innerHeight);
            const distanceFromBottom = viewportHeight - (fabBox.y + fabBox.height);
            expect(distanceFromBottom).toBeGreaterThanOrEqual(60);
            expect(distanceFromBottom).toBeLessThanOrEqual(90);

            // FAB size must meet minimum touch target requirements
            expect(fabBox.width).toBeGreaterThanOrEqual(MIN_TOUCH_TARGET_SIZE);
            expect(fabBox.height).toBeGreaterThanOrEqual(MIN_TOUCH_TARGET_SIZE);
          }
        }
      });

      test(`FAB remains fixed during scroll on ${device.name}`, async ({ page }) => {
        await setMobileViewport(page, device);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        const fab = page.locator('[aria-label*="Přidat denní záznam"]');
        const fabVisible = await fab.isVisible().catch(() => false);

        if (fabVisible) {
          const initialFabBox = await fab.boundingBox();
          expect(initialFabBox).not.toBeNull();

          // Scroll down
          await page.evaluate(() => window.scrollBy(0, 300));
          await page.waitForTimeout(300);

          const afterScrollFabBox = await fab.boundingBox();
          expect(afterScrollFabBox).not.toBeNull();

          if (initialFabBox && afterScrollFabBox) {
            // FAB should remain in same position (fixed positioning)
            expect(afterScrollFabBox.x).toBeCloseTo(initialFabBox.x, 2);
            expect(afterScrollFabBox.y).toBeCloseTo(initialFabBox.y, 2);
          }

          // Reset scroll
          await page.evaluate(() => window.scrollTo(0, 0));
        }
      });
    }
  });

  test.describe('Modal Fullscreen Behavior', () => {
    for (const device of MOBILE_DEVICES) {
      test(`Quick Add Modal fullscreen on ${device.name} (${device.platform})`, async ({ page }) => {
        await setMobileViewport(page, device);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        const fab = page.locator('[aria-label*="Přidat denní záznam"]');
        const fabVisible = await fab.isVisible().catch(() => false);

        if (fabVisible) {
          await fab.click();

          const modal = page.getByRole('dialog');
          await expect(modal).toBeVisible({ timeout: 3000 });

          const modalBox = await modal.boundingBox();
          expect(modalBox).not.toBeNull();

          if (modalBox) {
            // On mobile (< 600px), modal should be fullscreen
            expect(modalBox.width).toBeGreaterThanOrEqual(device.width - 2);
            expect(modalBox.height).toBeGreaterThanOrEqual(device.height * 0.9);

            // Modal should start near top and left edge
            expect(modalBox.y).toBeLessThanOrEqual(10);
            expect(modalBox.x).toBeLessThanOrEqual(2);
          }

          // Close modal
          const cancelButton = page.getByRole('button', { name: /zrušit/i });
          if (await cancelButton.isVisible().catch(() => false)) {
            await cancelButton.click();
            await expect(modal).not.toBeVisible();
          }
        }
      });
    }
  });

  test.describe('Touch Target Validation', () => {
    for (const device of MOBILE_DEVICES) {
      test(`FAB touch target size on ${device.name} (${device.platform})`, async ({ page }) => {
        await setMobileViewport(page, device);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        const fab = page.locator('[aria-label*="Přidat denní záznam"]');
        const fabVisible = await fab.isVisible().catch(() => false);

        if (fabVisible) {
          const fabBox = await fab.boundingBox();
          expect(fabBox).not.toBeNull();

          if (fabBox) {
            // FAB must meet minimum touch target size (44x44px)
            expect(fabBox.width).toBeGreaterThanOrEqual(MIN_TOUCH_TARGET_SIZE);
            expect(fabBox.height).toBeGreaterThanOrEqual(MIN_TOUCH_TARGET_SIZE);

            // Material Design recommends 56x56px for FABs
            expect(fabBox.width).toBeGreaterThanOrEqual(56);
            expect(fabBox.height).toBeGreaterThanOrEqual(56);
          }
        }
      });

      test(`Modal button touch targets on ${device.name} (${device.platform})`, async ({ page }) => {
        await setMobileViewport(page, device);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        const fab = page.locator('[aria-label*="Přidat denní záznam"]');
        const fabVisible = await fab.isVisible().catch(() => false);

        if (fabVisible) {
          await fab.click();

          const modal = page.getByRole('dialog');
          await expect(modal).toBeVisible({ timeout: 3000 });

          // Check submit button
          const submitButton = page.getByRole('button', { name: /uložit/i });
          if (await submitButton.isVisible().catch(() => false)) {
            const submitBox = await submitButton.boundingBox();
            if (submitBox) {
              expect(submitBox.height).toBeGreaterThanOrEqual(MIN_TOUCH_TARGET_SIZE);
            }
          }

          // Check cancel button
          const cancelButton = page.getByRole('button', { name: /zrušit/i });
          if (await cancelButton.isVisible().catch(() => false)) {
            const cancelBox = await cancelButton.boundingBox();
            if (cancelBox) {
              expect(cancelBox.height).toBeGreaterThanOrEqual(MIN_TOUCH_TARGET_SIZE);
            }

            // Close modal
            await cancelButton.click();
            await expect(modal).not.toBeVisible();
          }
        }
      });

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

  test.describe('iOS vs Android Consistency', () => {
    test('FAB behavior consistency across iOS devices', async ({ page }) => {
      const iosDevices = MOBILE_DEVICES.filter((d) => d.platform === 'iOS');

      for (const device of iosDevices) {
        await setMobileViewport(page, device);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        const fab = page.locator('[aria-label*="Přidat denní záznam"]');
        const fabVisible = await fab.isVisible().catch(() => false);

        if (fabVisible) {
          const fabBox = await fab.boundingBox();
          if (fabBox) {
            // Consistent size
            expect(fabBox.width).toBeGreaterThanOrEqual(56);
            expect(fabBox.height).toBeGreaterThanOrEqual(56);
          }
        }
      }
    });

    test('FAB behavior consistency across Android devices', async ({ page }) => {
      const androidDevices = MOBILE_DEVICES.filter((d) => d.platform === 'Android');

      for (const device of androidDevices) {
        await setMobileViewport(page, device);
        await page.goto('/dashboard');
        await page.waitForLoadState('networkidle');

        const fab = page.locator('[aria-label*="Přidat denní záznam"]');
        const fabVisible = await fab.isVisible().catch(() => false);

        if (fabVisible) {
          const fabBox = await fab.boundingBox();
          if (fabBox) {
            // Consistent size
            expect(fabBox.width).toBeGreaterThanOrEqual(56);
            expect(fabBox.height).toBeGreaterThanOrEqual(56);
          }
        }
      }
    });
  });
});
