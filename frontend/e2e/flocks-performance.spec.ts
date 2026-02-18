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

  test('should render flock list efficiently with performance metrics', async ({ page }) => {
    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle');

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
    expect(performanceMetrics.firstContentfulPaint).toBeLessThanOrEqualTo(1000);
  });

  test('should measure API response time for fetching flocks', async ({ page }) => {
    let apiResponseTime = 0;

    page.on('response', async (response) => {
      if (response.url().includes('/api/coops/') && response.url().includes('/flocks')) {
        const timing = response.timing();
        apiResponseTime = timing.responseEnd - timing.requestStart;
      }
    });

    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    console.log(`API response time: ${apiResponseTime.toFixed(2)}ms`);

    expect(apiResponseTime).toBeGreaterThan(0);
    expect(apiResponseTime).toBeLessThanOrEqualTo(2000); // 2 seconds for E2E including network
  });

  test('should verify no layout shift during render', async ({ page }) => {
    await page.goto(`/coops/${testCoopId}/flocks`);
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
});
