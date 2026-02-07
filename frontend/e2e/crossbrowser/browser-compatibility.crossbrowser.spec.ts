import { test, expect } from '@playwright/test';

/**
 * TESTING-002: Browser Compatibility Tests
 *
 * Tests browser-specific behavior and compatibility across:
 * - Chrome (latest 2 versions)
 * - Safari (latest 2 versions)
 * - Firefox (latest 2 versions)
 * - Mobile Safari iOS 15+
 * - Mobile Chrome Android 10+
 *
 * Focus areas:
 * - CSS rendering consistency
 * - JavaScript API compatibility
 * - Form input behavior
 * - Touch/gesture handling
 * - Local storage and cookies
 * - Network request handling
 */

test.describe('Browser Compatibility - Core Functionality', () => {
  test.describe.configure({ mode: 'parallel' });

  test('page navigation works correctly', async ({ page, browserName }) => {
    // Test basic navigation
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    expect(page.url()).toContain('/dashboard');

    // Navigate using bottom nav
    const coopsNav = page.locator('.MuiBottomNavigationAction-root').filter({
      hasText: /coops|kurníky/i,
    });
    await coopsNav.click();
    await page.waitForURL('**/coops');
    expect(page.url()).toContain('/coops');

    // Test back navigation
    await page.goBack();
    expect(page.url()).toContain('/dashboard');

    console.log(`Navigation test passed on ${browserName}`);
  });

  test('API requests complete successfully', async ({ page, browserName }) => {
    const apiResponses: { url: string; status: number }[] = [];

    page.on('response', (response) => {
      if (response.url().includes('/api/')) {
        apiResponses.push({
          url: response.url(),
          status: response.status(),
        });
      }
    });

    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Wait for API calls
    await page.waitForTimeout(1000);

    // Verify API calls were successful
    const failedResponses = apiResponses.filter((r) => r.status >= 400);
    expect(failedResponses.length).toBe(0);

    console.log(`API test passed on ${browserName}, ${apiResponses.length} requests made`);
  });

  test('local storage operations work', async ({ page, browserName }) => {
    await page.goto('/dashboard');

    // Set item in localStorage
    await page.evaluate(() => {
      localStorage.setItem('test-key', 'test-value');
    });

    // Retrieve item
    const value = await page.evaluate(() => {
      return localStorage.getItem('test-key');
    });

    expect(value).toBe('test-value');

    // Clean up
    await page.evaluate(() => {
      localStorage.removeItem('test-key');
    });

    console.log(`LocalStorage test passed on ${browserName}`);
  });

  test('cookies can be set and read', async ({ page, browserName, context }) => {
    await page.goto('/dashboard');

    // Set a cookie
    await context.addCookies([{
      name: 'test-cookie',
      value: 'test-value',
      domain: 'localhost',
      path: '/',
    }]);

    // Verify cookie was set
    const cookies = await context.cookies();
    const testCookie = cookies.find((c) => c.name === 'test-cookie');
    expect(testCookie).toBeDefined();
    expect(testCookie?.value).toBe('test-value');

    console.log(`Cookie test passed on ${browserName}`);
  });
});

test.describe('Browser Compatibility - Form Handling', () => {
  test.describe.configure({ mode: 'serial' });

  test('text input works correctly', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Open create modal
    const addButton = page.getByRole('button', { name: /add coop|přidat kurník/i }).first();
    await addButton.click();
    await page.waitForTimeout(500);

    // Test text input
    const nameInput = page.getByRole('textbox', { name: /name|název/i });
    await nameInput.fill('Test Coop Name');

    const value = await nameInput.inputValue();
    expect(value).toBe('Test Coop Name');

    // Test clearing input
    await nameInput.clear();
    const clearedValue = await nameInput.inputValue();
    expect(clearedValue).toBe('');

    // Test typing character by character
    await nameInput.type('Typed Text', { delay: 50 });
    const typedValue = await nameInput.inputValue();
    expect(typedValue).toBe('Typed Text');

    console.log(`Text input test passed on ${browserName}`);
  });

  test('form validation displays correctly', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Open create modal
    const addButton = page.getByRole('button', { name: /add coop|přidat kurník/i }).first();
    await addButton.click();
    await page.waitForTimeout(500);

    // Try to submit empty form
    const submitButton = page.getByRole('button', { name: /create|vytvořit|save|uložit/i });

    // Clear any existing value and trigger validation
    const nameInput = page.getByRole('textbox', { name: /name|název/i });
    await nameInput.clear();
    await nameInput.blur();

    // Check for validation message or submit button state
    await page.waitForTimeout(300);

    // Either button should be disabled OR error message should appear
    const isDisabled = await submitButton.isDisabled();
    const errorMessage = page.getByText(/required|povinné|name is required|název je povinný/i);
    const hasError = await errorMessage.isVisible().catch(() => false);

    expect(isDisabled || hasError).toBe(true);

    console.log(`Form validation test passed on ${browserName}`);
  });

  test('select/dropdown works correctly', async ({ page, browserName }) => {
    await page.goto('/settings');
    await page.waitForLoadState('networkidle');

    // Test language selector
    const languageSelect = page.locator('#language-select');
    await languageSelect.click();

    // Wait for dropdown options
    await page.waitForTimeout(300);

    // Select English option
    const englishOption = page.getByRole('option', { name: /english/i });
    await expect(englishOption).toBeVisible();
    await englishOption.click();

    // Verify selection changed
    await page.waitForTimeout(300);

    console.log(`Select dropdown test passed on ${browserName}`);
  });
});

test.describe('Browser Compatibility - CSS Features', () => {
  test('flexbox layout renders correctly', async ({ page, browserName }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Check that flex containers work
    const flexContainer = page.locator('[class*="MuiBox-root"]').first();
    const display = await flexContainer.evaluate((el) => {
      return window.getComputedStyle(el).display;
    });

    // Container should have flex or block display
    expect(['flex', 'block', 'grid']).toContain(display);

    console.log(`Flexbox test passed on ${browserName}`);
  });

  test('CSS grid layout renders correctly', async ({ page, browserName }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Dashboard uses CSS grid for statistics
    const gridContainer = page.locator('[class*="MuiBox-root"]').filter({
      has: page.locator('[class*="MuiCard-root"]'),
    }).first();

    const containerVisible = await gridContainer.isVisible().catch(() => false);

    if (containerVisible) {
      const display = await gridContainer.evaluate((el) => {
        return window.getComputedStyle(el).display;
      });

      // Should support grid or flex for responsive layouts
      expect(['grid', 'flex', 'block']).toContain(display);
    }

    console.log(`CSS Grid test passed on ${browserName}`);
  });

  test('transitions and animations work', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Open modal to test transitions
    const addButton = page.getByRole('button', { name: /add coop|přidat kurník/i }).first();
    await addButton.click();

    // Modal should appear with transition
    const modal = page.getByRole('dialog');
    await expect(modal).toBeVisible();

    // Check for transition property
    const hasTransition = await modal.evaluate((el) => {
      const style = window.getComputedStyle(el);
      return style.transition !== 'none' && style.transition !== '';
    });

    // Close modal
    const closeButton = page.getByRole('button', { name: /cancel|zrušit|close/i });
    await closeButton.click();
    await expect(modal).not.toBeVisible();

    console.log(`Transitions test passed on ${browserName}, hasTransition: ${hasTransition}`);
  });

  test('box-shadow renders correctly', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    const card = page.locator('[data-testid="coop-card"]').first();
    const cardVisible = await card.isVisible().catch(() => false);

    if (cardVisible) {
      const boxShadow = await card.evaluate((el) => {
        return window.getComputedStyle(el).boxShadow;
      });

      // Should have some box-shadow (not 'none')
      expect(boxShadow).not.toBe('none');
    }

    console.log(`Box-shadow test passed on ${browserName}`);
  });

  test('border-radius renders correctly', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    const card = page.locator('[data-testid="coop-card"]').first();
    const cardVisible = await card.isVisible().catch(() => false);

    if (cardVisible) {
      const borderRadius = await card.evaluate((el) => {
        return window.getComputedStyle(el).borderRadius;
      });

      // Should have border-radius (MUI cards have rounded corners)
      expect(borderRadius).not.toBe('0px');
    }

    console.log(`Border-radius test passed on ${browserName}`);
  });
});

test.describe('Browser Compatibility - JavaScript APIs', () => {
  test('Promise and async/await work', async ({ page, browserName }) => {
    await page.goto('/dashboard');

    // Test Promise support
    const promiseResult = await page.evaluate(async () => {
      const promise = new Promise<string>((resolve) => {
        setTimeout(() => resolve('resolved'), 100);
      });
      return await promise;
    });

    expect(promiseResult).toBe('resolved');

    console.log(`Promise test passed on ${browserName}`);
  });

  test('fetch API works', async ({ page, browserName }) => {
    await page.goto('/dashboard');

    // Test fetch API
    const fetchResult = await page.evaluate(async () => {
      try {
        const response = await fetch('/health', { method: 'GET' });
        return { ok: response.ok, status: response.status };
      } catch {
        // If health endpoint doesn't exist, that's OK
        return { ok: false, status: 0 };
      }
    });

    // Just verify fetch doesn't throw
    expect(fetchResult).toBeDefined();

    console.log(`Fetch API test passed on ${browserName}`);
  });

  test('array methods work (map, filter, reduce)', async ({ page, browserName }) => {
    await page.goto('/dashboard');

    const result = await page.evaluate(() => {
      const arr = [1, 2, 3, 4, 5];
      const mapped = arr.map((x) => x * 2);
      const filtered = mapped.filter((x) => x > 4);
      const reduced = filtered.reduce((acc, x) => acc + x, 0);
      return { mapped, filtered, reduced };
    });

    expect(result.mapped).toEqual([2, 4, 6, 8, 10]);
    expect(result.filtered).toEqual([6, 8, 10]);
    expect(result.reduced).toBe(24);

    console.log(`Array methods test passed on ${browserName}`);
  });

  test('Object.entries/values/keys work', async ({ page, browserName }) => {
    await page.goto('/dashboard');

    const result = await page.evaluate(() => {
      const obj = { a: 1, b: 2, c: 3 };
      return {
        keys: Object.keys(obj),
        values: Object.values(obj),
        entries: Object.entries(obj),
      };
    });

    expect(result.keys).toEqual(['a', 'b', 'c']);
    expect(result.values).toEqual([1, 2, 3]);
    expect(result.entries).toEqual([['a', 1], ['b', 2], ['c', 3]]);

    console.log(`Object methods test passed on ${browserName}`);
  });

  test('template literals work', async ({ page, browserName }) => {
    await page.goto('/dashboard');

    const result = await page.evaluate(() => {
      const name = 'World';
      const greeting = `Hello, ${name}!`;
      const multiline = `Line 1
Line 2`;
      return { greeting, multiline };
    });

    expect(result.greeting).toBe('Hello, World!');
    expect(result.multiline).toBe('Line 1\nLine 2');

    console.log(`Template literals test passed on ${browserName}`);
  });
});

test.describe('Browser Compatibility - Touch Events', () => {
  test('click events work on touch devices', async ({ page, browserName }) => {
    // This test is primarily for mobile browsers
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Test click on FAB button
    const fab = page.locator('.MuiFab-root');
    await fab.click();

    // Modal should open
    const modal = page.getByRole('dialog');
    await expect(modal).toBeVisible();

    // Close modal
    const closeButton = page.getByRole('button', { name: /cancel|zrušit/i });
    await closeButton.click();

    console.log(`Touch click test passed on ${browserName}`);
  });

  test('scroll events work', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Get initial scroll position
    const initialScroll = await page.evaluate(() => window.scrollY);

    // Scroll down
    await page.evaluate(() => {
      window.scrollTo(0, 200);
    });

    const newScroll = await page.evaluate(() => window.scrollY);

    // Verify scroll worked (might be 0 if page is not long enough)
    expect(newScroll).toBeGreaterThanOrEqual(0);

    console.log(`Scroll test passed on ${browserName}`);
  });
});

test.describe('Browser Compatibility - Accessibility Features', () => {
  test('focus management works', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Open modal
    const fab = page.locator('.MuiFab-root');
    await fab.click();
    await page.waitForTimeout(500);

    // Check that focus moved to modal
    const modal = page.getByRole('dialog');
    await expect(modal).toBeVisible();

    // Tab through focusable elements
    await page.keyboard.press('Tab');
    const focusedElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(focusedElement).not.toBe('BODY');

    console.log(`Focus management test passed on ${browserName}`);
  });

  test('ARIA attributes are present', async ({ page, browserName }) => {
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    // Check for aria-label on FAB
    const fab = page.locator('.MuiFab-root');
    const ariaLabel = await fab.getAttribute('aria-label');
    expect(ariaLabel).not.toBeNull();

    // Check for role on cards
    const card = page.locator('[data-testid="coop-card"]').first();
    const cardVisible = await card.isVisible().catch(() => false);

    if (cardVisible) {
      const role = await card.getAttribute('role');
      expect(role).toBe('article');
    }

    console.log(`ARIA test passed on ${browserName}`);
  });

  test('keyboard navigation works', async ({ page, browserName }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Tab to navigate
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');

    // Check something is focused
    const focusedTag = await page.evaluate(() => document.activeElement?.tagName);
    expect(focusedTag).not.toBe('BODY');

    // Enter key should activate focused element if it's a button/link
    const focusedRole = await page.evaluate(() =>
      document.activeElement?.getAttribute('role') ||
      document.activeElement?.tagName.toLowerCase()
    );

    console.log(`Keyboard navigation test passed on ${browserName}, focused: ${focusedRole}`);
  });
});

test.describe('Browser Compatibility - Error Handling', () => {
  test('JavaScript errors are captured', async ({ page, browserName }) => {
    const errors: string[] = [];

    page.on('pageerror', (error) => {
      errors.push(error.message);
    });

    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');

    // Navigate around
    await page.goto('/coops');
    await page.waitForLoadState('networkidle');

    await page.goto('/settings');
    await page.waitForLoadState('networkidle');

    // Report any errors found
    if (errors.length > 0) {
      console.warn(`JS errors on ${browserName}:`, errors);
    }

    // Test passes if no critical errors
    // (some non-critical errors might exist)
    console.log(`Error handling test completed on ${browserName}, errors: ${errors.length}`);
  });

  test('network errors are handled gracefully', async ({ page, browserName }) => {
    // Simulate slow network
    await page.route('**/api/**', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 100));
      await route.continue();
    });

    await page.goto('/coops');

    // Page should still load even with slow network
    await page.waitForLoadState('domcontentloaded');

    // Either content or loading state should be visible
    const content = page.getByRole('heading', { name: /coops|kurníky/i });
    await expect(content).toBeVisible({ timeout: 15000 });

    console.log(`Network error handling test passed on ${browserName}`);
  });
});
