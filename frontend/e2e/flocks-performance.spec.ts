import { test, expect } from '@playwright/test';
import { FlocksPage } from './pages/FlocksPage';
import { CreateFlockModal } from './pages/CreateFlockModal';
import { CreateCoopModal } from './pages/CreateCoopModal';
import { CoopsPage } from './pages/CoopsPage';
import { generateFlockIdentifier, getDaysAgoDate } from './fixtures/flock.fixture';
import { generateCoopName } from './fixtures/coop.fixture';

test.describe('Flock Performance Tests', () => {
  let coopsPage: CoopsPage;
  let flocksPage: FlocksPage;
  let createFlockModal: CreateFlockModal;
  let createCoopModal: CreateCoopModal;
  let testCoopId: string;
  let testCoopName: string;

  // Performance requirements
  const MAX_RENDER_TIME_MS = 1000;

  test.beforeAll(async ({ browser }) => {
    // Create a separate context and page for setup
    const context = await browser.newContext();
    const page = await context.newPage();

    coopsPage = new CoopsPage(page);
    flocksPage = new FlocksPage(page);
    createFlockModal = new CreateFlockModal(page);
    createCoopModal = new CreateCoopModal(page);

    // Create a test coop for performance tests
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    await coopsPage.openCreateCoopModal();
    await page.waitForTimeout(500);

    testCoopName = generateCoopName('Performance Test Coop');
    await createCoopModal.createCoop(testCoopName, 'Performance Test Location');

    await page.waitForTimeout(2000);
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

    // Create 50 flocks to test rendering performance with realistic data
    await flocksPage.goto(testCoopId);

    for (let i = 0; i < 50; i++) {
      await flocksPage.openCreateFlockModal();

      const flockData = {
        identifier: generateFlockIdentifier(`PerfFlock-${i.toString().padStart(3, '0')}`),
        hatchDate: getDaysAgoDate(30 + i),
        hens: 10 + (i % 20),
        roosters: 2 + (i % 3),
        chicks: i % 10,
        notes: `Performance test flock number ${i}`,
      };

      await createFlockModal.createFlock(flockData);
      await page.waitForTimeout(500);
    }

    await context.close();
  });

  test('should render flock list within 1 second', async ({ page }) => {
    // Navigate to the page and measure load time
    const startTime = Date.now();

    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle');

    // Wait for flocks to be visible
    const flockCards = page.locator('[data-testid^="flock-card-"]');
    await expect(flockCards.first()).toBeVisible({ timeout: 5000 });

    const endTime = Date.now();
    const renderTime = endTime - startTime;

    console.log(`Flock list render time: ${renderTime}ms (target: < ${MAX_RENDER_TIME_MS}ms)`);

    // Assert render time is within acceptable limits
    expect(renderTime).toBeLessThanOrEqualTo(MAX_RENDER_TIME_MS);
  });

  test('should render flock list efficiently with performance metrics', async ({ page }) => {
    // Use Performance API to measure rendering
    await page.goto(`/coops/${testCoopId}/flocks`);

    // Wait for page to fully load
    await page.waitForLoadState('networkidle');

    // Evaluate performance metrics
    const performanceMetrics = await page.evaluate(() => {
      const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
      const paint = performance.getEntriesByType('paint');

      const firstContentfulPaint = paint.find((entry) => entry.name === 'first-contentful-paint');
      const largestContentfulPaint = performance.getEntriesByType('largest-contentful-paint')[0];

      return {
        domContentLoaded: navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart,
        loadComplete: navigation.loadEventEnd - navigation.loadEventStart,
        firstContentfulPaint: firstContentfulPaint?.startTime || 0,
        largestContentfulPaint: largestContentfulPaint?.startTime || 0,
        domInteractive: navigation.domInteractive - navigation.fetchStart,
      };
    });

    console.log('Performance Metrics:');
    console.log(`  DOM Content Loaded: ${performanceMetrics.domContentLoaded.toFixed(2)}ms`);
    console.log(`  Load Complete: ${performanceMetrics.loadComplete.toFixed(2)}ms`);
    console.log(`  First Contentful Paint: ${performanceMetrics.firstContentfulPaint.toFixed(2)}ms`);
    console.log(`  Largest Contentful Paint: ${performanceMetrics.largestContentfulPaint.toFixed(2)}ms`);
    console.log(`  DOM Interactive: ${performanceMetrics.domInteractive.toFixed(2)}ms`);

    // Assert First Contentful Paint is within limits
    expect(performanceMetrics.firstContentfulPaint).toBeLessThanOrEqualTo(MAX_RENDER_TIME_MS);
  });

  test('should handle scrolling through large list smoothly', async ({ page }) => {
    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle');

    // Wait for initial render
    const flockCards = page.locator('[data-testid^="flock-card-"]');
    await expect(flockCards.first()).toBeVisible();

    // Measure scroll performance
    const startTime = Date.now();

    // Scroll to bottom
    await page.evaluate(() => {
      window.scrollTo(0, document.body.scrollHeight);
    });

    // Wait for scroll to complete
    await page.waitForTimeout(500);

    // Verify last flock is visible
    await expect(flockCards.last()).toBeVisible({ timeout: 2000 });

    const scrollTime = Date.now() - startTime;
    console.log(`Scroll to bottom time: ${scrollTime}ms`);

    // Scrolling should be fast (< 1 second)
    expect(scrollTime).toBeLessThanOrEqualTo(1000);
  });

  test('should filter flocks without performance degradation', async ({ page }) => {
    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle');

    // Wait for initial render
    const flockCards = page.locator('[data-testid^="flock-card-"]');
    await expect(flockCards.first()).toBeVisible();

    // Measure filter change performance
    const startTime = Date.now();

    // Click "All" filter button
    const allFilterButton = page.getByRole('button', { name: /všechny|all/i });
    await allFilterButton.click();

    // Wait for re-render
    await page.waitForTimeout(100);
    await expect(flockCards.first()).toBeVisible();

    const filterTime = Date.now() - startTime;
    console.log(`Filter change time: ${filterTime}ms`);

    // Filter should be near-instant (< 500ms)
    expect(filterTime).toBeLessThanOrEqualTo(500);
  });

  test('should measure API response time for fetching flocks', async ({ page }) => {
    // Intercept API calls and measure response time
    let apiResponseTime = 0;

    page.on('response', async (response) => {
      if (response.url().includes('/api/coops/') && response.url().includes('/flocks')) {
        const request = response.request();
        const timing = response.timing();
        apiResponseTime = timing.responseEnd - timing.requestStart;
      }
    });

    // Navigate to flocks page
    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle');

    // Wait for API call to complete
    await page.waitForTimeout(1000);

    console.log(`API response time: ${apiResponseTime.toFixed(2)}ms`);

    // API should respond within performance budget
    // Note: This is end-to-end including network, so we allow more time than backend-only tests
    expect(apiResponseTime).toBeGreaterThan(0);
    expect(apiResponseTime).toBeLessThanOrEqualTo(2000); // 2 seconds for E2E including network
  });

  test('should verify no layout shift during render', async ({ page }) => {
    await page.goto(`/coops/${testCoopId}/flocks`);

    // Measure Cumulative Layout Shift (CLS)
    await page.waitForLoadState('networkidle');

    const cls = await page.evaluate(() => {
      return new Promise<number>((resolve) => {
        let clsValue = 0;
        const observer = new PerformanceObserver((list) => {
          for (const entry of list.getEntries()) {
            if ((entry as any).hadRecentInput) continue;
            clsValue += (entry as any).value;
          }
        });

        observer.observe({ type: 'layout-shift', buffered: true });

        // Wait a bit for layout shifts to occur
        setTimeout(() => {
          observer.disconnect();
          resolve(clsValue);
        }, 2000);
      });
    });

    console.log(`Cumulative Layout Shift: ${cls.toFixed(4)}`);

    // CLS should be less than 0.1 (good score)
    expect(cls).toBeLessThanOrEqualTo(0.1);
  });

  test('should render individual flock cards efficiently', async ({ page }) => {
    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle');

    // Measure time to render all flock cards
    const renderTime = await page.evaluate(() => {
      const startTime = performance.now();
      const flockCards = document.querySelectorAll('[data-testid^="flock-card-"]');
      const endTime = performance.now();

      return {
        cardCount: flockCards.length,
        renderTime: endTime - startTime,
      };
    });

    console.log(`Rendered ${renderTime.cardCount} flock cards in ${renderTime.renderTime.toFixed(2)}ms`);

    // Should have rendered all 50 flocks
    expect(renderTime.cardCount).toBeGreaterThanOrEqualTo(50);

    // Render time should be fast
    expect(renderTime.renderTime).toBeLessThanOrEqualTo(MAX_RENDER_TIME_MS);
  });

  test('should handle rapid navigation without performance issues', async ({ page }) => {
    // Measure time for repeated navigation
    const navigationTimes: number[] = [];

    for (let i = 0; i < 5; i++) {
      const startTime = Date.now();

      await page.goto(`/coops/${testCoopId}/flocks`);
      await page.waitForLoadState('networkidle');

      const flockCards = page.locator('[data-testid^="flock-card-"]');
      await expect(flockCards.first()).toBeVisible();

      const navTime = Date.now() - startTime;
      navigationTimes.push(navTime);

      console.log(`Navigation ${i + 1}: ${navTime}ms`);

      // Navigate away
      await page.goto(`/coops/${testCoopId}`);
      await page.waitForLoadState('networkidle');
    }

    // Calculate average and verify consistency
    const avgTime = navigationTimes.reduce((a, b) => a + b, 0) / navigationTimes.length;
    const maxTime = Math.max(...navigationTimes);
    const minTime = Math.min(...navigationTimes);

    console.log(`Average navigation time: ${avgTime.toFixed(2)}ms`);
    console.log(`Min: ${minTime}ms, Max: ${maxTime}ms`);

    // Average should be within acceptable limits
    expect(avgTime).toBeLessThanOrEqualTo(MAX_RENDER_TIME_MS);

    // Variance should be reasonable (max shouldn't be > 2x min)
    expect(maxTime).toBeLessThanOrEqualTo(minTime * 2);
  });
});
