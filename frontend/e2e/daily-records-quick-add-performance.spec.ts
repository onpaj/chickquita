import { test, expect } from '@playwright/test';
import { CoopsPage } from './pages/CoopsPage';
import { CreateCoopModal } from './pages/CreateCoopModal';
import { CoopDetailPage } from './pages/CoopDetailPage';
import { CreateFlockModal } from './pages/CreateFlockModal';
import { generateCoopName } from './fixtures/coop.fixture';
import { generateFlockIdentifier, getDaysAgoDate } from './fixtures/flock.fixture';

/**
 * E2E Performance tests for Daily Records Quick Add Modal
 * US-023: E2E - Quick Add Modal Performance Test
 *
 * Tests the Quick Add workflow performance on throttled network (Fast 3G):
 * - Full workflow < 30 seconds on Fast 3G
 * - Test passes consistently (3/3 runs)
 * - Performance metrics logged
 *
 * Fast 3G Simulation:
 * - Download throughput: 1.6 Mbps (200 KB/s)
 * - Upload throughput: 750 Kbps (93.75 KB/s)
 * - Latency: 562.5 ms RTT
 */
test.describe('Daily Records - Quick Add Modal Performance (Fast 3G)', () => {
  // Increase timeout for setup and tests due to network throttling
  test.setTimeout(120000); // 2 minutes
  let coopsPage: CoopsPage;
  let createCoopModal: CreateCoopModal;
  let coopDetailPage: CoopDetailPage;
  let createFlockModal: CreateFlockModal;
  let testCoopId: string;
  let testFlockId: string;
  let testCoopName: string;
  let testFlockIdentifier: string;

  /**
   * Before each test, create a fresh test environment:
   * 1. Create a new coop
   * 2. Create a new flock in that coop
   * This ensures test isolation
   * NOTE: Setup is done WITHOUT throttling to keep setup time reasonable
   */
  test.beforeEach(async ({ page }) => {
    // Initialize page objects
    coopsPage = new CoopsPage(page);
    createCoopModal = new CreateCoopModal(page);
    coopDetailPage = new CoopDetailPage(page);
    createFlockModal = new CreateFlockModal(page);

    // Generate unique test data
    testCoopName = generateCoopName();
    testFlockIdentifier = generateFlockIdentifier();

    // Navigate to coops page
    await coopsPage.goto();
    await page.waitForLoadState('networkidle');

    // Create test coop - set up response listener
    let createdCoopId: string | null = null;
    page.on('response', async (response) => {
      if (
        response.url().includes('/api/coops') &&
        response.request().method() === 'POST' &&
        response.status() === 201
      ) {
        try {
          const data = await response.json();
          if (data && data.id) {
            createdCoopId = data.id;
          }
        } catch {
          // Ignore JSON parsing errors
        }
      }
    });

    await coopsPage.openCreateCoopModal();
    await page.waitForTimeout(500);
    await createCoopModal.fillForm(testCoopName, 'E2E Performance Test Coop');
    await createCoopModal.submit();

    // Wait for modal to close and UI to update
    await createCoopModal.waitForClose();
    await page.waitForTimeout(3000); // Extra time for response to be captured
    await page.waitForLoadState('networkidle');

    // Verify coop ID was captured
    if (!createdCoopId) {
      throw new Error(`Could not capture coop ID from API response for coop: ${testCoopName}`);
    }

    testCoopId = createdCoopId;

    // Navigate to flocks page
    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle');

    // Create test flock - set up response listener
    let createdFlockId: string | null = null;
    page.on('response', async (response) => {
      if (
        response.url().includes('/api/flocks') &&
        response.request().method() === 'POST' &&
        response.status() === 201
      ) {
        try {
          const data = await response.json();
          if (data && data.id) {
            createdFlockId = data.id;
            testFlockId = data.id;
          }
        } catch {
          // Ignore JSON parsing errors
        }
      }
    });

    await coopDetailPage.clickAddFlockButton();
    await createFlockModal.fillForm(
      testFlockIdentifier,
      getDaysAgoDate(30),
      5, // hens
      1, // roosters
      0  // chicks
    );
    await createFlockModal.submit();

    // Wait for modal to close and UI to update
    await createFlockModal.waitForClose();
    await page.waitForTimeout(2000); // Extra time for response to be captured
    await page.waitForLoadState('networkidle');

    // Verify flock ID was captured
    if (!createdFlockId) {
      throw new Error(`Could not capture flock ID from API response for flock: ${testFlockIdentifier}`);
    }

    console.log('✓ Test setup completed');
  });

  /**
   * Helper function to emulate Fast 3G network conditions
   * Fast 3G specs from Chrome DevTools Network Throttling:
   * - Download: 1.6 Mbps (200 KB/s)
   * - Upload: 750 Kbps (93.75 KB/s)
   * - Latency: 562.5 ms RTT
   */
  async function emulateFast3G(page: any) {
    const client = await page.context().newCDPSession(page);
    await client.send('Network.emulateNetworkConditions', {
      offline: false,
      downloadThroughput: (1.6 * 1024 * 1024) / 8, // 1.6 Mbps in bytes/sec
      uploadThroughput: (750 * 1024) / 8, // 750 Kbps in bytes/sec
      latency: 562.5, // 562.5 ms
    });
  }

  /**
   * Test 1: Quick Add workflow on Fast 3G - Run 1
   * Verifies < 30 second target for quick add workflow
   */
  test('should complete Quick Add workflow in < 30 seconds on Fast 3G (Run 1/3)', async ({
    page,
  }) => {
    // Enable Fast 3G network throttling
    await emulateFast3G(page);

    const workflowStartTime = Date.now();
    console.log('Starting Quick Add workflow with Fast 3G throttling (Run 1/3)...');

    // Navigate to dashboard
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Open Quick Add modal
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await expect(quickAddButton).toBeVisible();
    await quickAddButton.click();

    // Verify modal is open
    await expect(page.getByText('Rychlý záznam vajec')).toBeVisible();

    // Fill in the form
    const eggCount = 8;
    const notes = 'E2E performance test - Run 1';

    // Set egg count using stepper
    const incrementButton = page.getByLabel('egg count increase');
    for (let i = 0; i < eggCount; i++) {
      await incrementButton.click();
    }

    // Verify egg count
    const eggCountInput = page.locator('input[type="number"][aria-label="egg count"]');
    await expect(eggCountInput).toHaveValue(eggCount.toString());

    // Add notes
    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill(notes);

    // Wait for POST response
    const createResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );

    // Submit form
    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    // Wait for response
    const createResponse = await createResponsePromise;
    const createdRecord = await createResponse.json();
    expect(createdRecord.id).toBeTruthy();

    // Modal should close
    await expect(page.getByText('Rychlý záznam vajec')).not.toBeVisible();

    // Success toast should appear
    await expect(page.getByText(/úspěšně vytvořen/i)).toBeVisible();

    // Calculate workflow time
    const workflowTime = (Date.now() - workflowStartTime) / 1000;
    console.log(`✓ Quick Add workflow completed in ${workflowTime.toFixed(2)} seconds (Run 1/3)`);

    // Performance metrics
    console.log(`Performance Metrics (Run 1/3):`);
    console.log(`  - Network: Fast 3G (1.6 Mbps down, 750 Kbps up, 562.5ms latency)`);
    console.log(`  - Total workflow time: ${workflowTime.toFixed(2)}s`);
    console.log(`  - Target: < 30 seconds`);
    console.log(`  - Status: ${workflowTime < 30 ? 'PASS ✓' : 'FAIL ✗'}`);

    // Performance requirement: < 30 seconds
    expect(workflowTime).toBeLessThan(30);
  });

  /**
   * Test 2: Quick Add workflow on Fast 3G - Run 2
   * Verifies consistent performance across multiple runs
   */
  test('should complete Quick Add workflow in < 30 seconds on Fast 3G (Run 2/3)', async ({
    page,
  }) => {
    // Enable Fast 3G network throttling
    await emulateFast3G(page);

    const workflowStartTime = Date.now();
    console.log('Starting Quick Add workflow with Fast 3G throttling (Run 2/3)...');

    // Navigate to dashboard
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Open Quick Add modal
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await expect(quickAddButton).toBeVisible();
    await quickAddButton.click();

    // Verify modal is open
    await expect(page.getByText('Rychlý záznam vajec')).toBeVisible();

    // Fill in the form
    const eggCount = 12;
    const notes = 'E2E performance test - Run 2';

    // Set egg count using stepper
    const incrementButton = page.getByLabel('egg count increase');
    for (let i = 0; i < eggCount; i++) {
      await incrementButton.click();
    }

    // Verify egg count
    const eggCountInput = page.locator('input[type="number"][aria-label="egg count"]');
    await expect(eggCountInput).toHaveValue(eggCount.toString());

    // Add notes
    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill(notes);

    // Wait for POST response
    const createResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );

    // Submit form
    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    // Wait for response
    const createResponse = await createResponsePromise;
    const createdRecord = await createResponse.json();
    expect(createdRecord.id).toBeTruthy();

    // Modal should close
    await expect(page.getByText('Rychlý záznam vajec')).not.toBeVisible();

    // Success toast should appear
    await expect(page.getByText(/úspěšně vytvořen/i)).toBeVisible();

    // Calculate workflow time
    const workflowTime = (Date.now() - workflowStartTime) / 1000;
    console.log(`✓ Quick Add workflow completed in ${workflowTime.toFixed(2)} seconds (Run 2/3)`);

    // Performance metrics
    console.log(`Performance Metrics (Run 2/3):`);
    console.log(`  - Network: Fast 3G (1.6 Mbps down, 750 Kbps up, 562.5ms latency)`);
    console.log(`  - Total workflow time: ${workflowTime.toFixed(2)}s`);
    console.log(`  - Target: < 30 seconds`);
    console.log(`  - Status: ${workflowTime < 30 ? 'PASS ✓' : 'FAIL ✗'}`);

    // Performance requirement: < 30 seconds
    expect(workflowTime).toBeLessThan(30);
  });

  /**
   * Test 3: Quick Add workflow on Fast 3G - Run 3
   * Verifies consistent performance across multiple runs
   */
  test('should complete Quick Add workflow in < 30 seconds on Fast 3G (Run 3/3)', async ({
    page,
  }) => {
    // Enable Fast 3G network throttling
    await emulateFast3G(page);

    const workflowStartTime = Date.now();
    console.log('Starting Quick Add workflow with Fast 3G throttling (Run 3/3)...');

    // Navigate to dashboard
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Open Quick Add modal
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await expect(quickAddButton).toBeVisible();
    await quickAddButton.click();

    // Verify modal is open
    await expect(page.getByText('Rychlý záznam vajec')).toBeVisible();

    // Fill in the form
    const eggCount = 15;
    const notes = 'E2E performance test - Run 3';

    // Set egg count using stepper
    const incrementButton = page.getByLabel('egg count increase');
    for (let i = 0; i < eggCount; i++) {
      await incrementButton.click();
    }

    // Verify egg count
    const eggCountInput = page.locator('input[type="number"][aria-label="egg count"]');
    await expect(eggCountInput).toHaveValue(eggCount.toString());

    // Add notes
    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill(notes);

    // Wait for POST response
    const createResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );

    // Submit form
    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    // Wait for response
    const createResponse = await createResponsePromise;
    const createdRecord = await createResponse.json();
    expect(createdRecord.id).toBeTruthy();

    // Modal should close
    await expect(page.getByText('Rychlý záznam vajec')).not.toBeVisible();

    // Success toast should appear
    await expect(page.getByText(/úspěšně vytvořen/i)).toBeVisible();

    // Calculate workflow time
    const workflowTime = (Date.now() - workflowStartTime) / 1000;
    console.log(`✓ Quick Add workflow completed in ${workflowTime.toFixed(2)} seconds (Run 3/3)`);

    // Performance metrics
    console.log(`Performance Metrics (Run 3/3):`);
    console.log(`  - Network: Fast 3G (1.6 Mbps down, 750 Kbps up, 562.5ms latency)`);
    console.log(`  - Total workflow time: ${workflowTime.toFixed(2)}s`);
    console.log(`  - Target: < 30 seconds`);
    console.log(`  - Status: ${workflowTime < 30 ? 'PASS ✓' : 'FAIL ✗'}`);

    // Performance requirement: < 30 seconds
    expect(workflowTime).toBeLessThan(30);
  });

  /**
   * Test 4: Quick Add workflow on Fast 3G with mobile viewport
   * Verifies performance on mobile device with throttled network
   */
  test('should complete Quick Add workflow in < 30 seconds on Fast 3G mobile viewport', async ({
    page,
  }) => {
    // Set mobile viewport (iPhone SE)
    await page.setViewportSize({ width: 375, height: 667 });

    // Enable Fast 3G network throttling
    await emulateFast3G(page);

    const workflowStartTime = Date.now();
    console.log('Starting Quick Add workflow with Fast 3G throttling on mobile viewport...');

    // Navigate to dashboard
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Open Quick Add modal
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await expect(quickAddButton).toBeVisible();
    await quickAddButton.click();

    // Verify modal is open
    await expect(page.getByText('Rychlý záznam vajec')).toBeVisible();

    // Fill in the form
    const eggCount = 10;
    const notes = 'E2E performance test - Mobile Fast 3G';

    // Set egg count using stepper
    const incrementButton = page.getByLabel('egg count increase');
    for (let i = 0; i < eggCount; i++) {
      await incrementButton.click();
    }

    // Verify egg count
    const eggCountInput = page.locator('input[type="number"][aria-label="egg count"]');
    await expect(eggCountInput).toHaveValue(eggCount.toString());

    // Add notes
    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill(notes);

    // Wait for POST response
    const createResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );

    // Submit form
    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();

    // Wait for response
    const createResponse = await createResponsePromise;
    const createdRecord = await createResponse.json();
    expect(createdRecord.id).toBeTruthy();

    // Modal should close
    await expect(page.getByText('Rychlý záznam vajec')).not.toBeVisible();

    // Success toast should appear
    await expect(page.getByText(/úspěšně vytvořen/i)).toBeVisible();

    // Calculate workflow time
    const workflowTime = (Date.now() - workflowStartTime) / 1000;
    console.log(
      `✓ Quick Add workflow completed in ${workflowTime.toFixed(2)} seconds on mobile viewport`
    );

    // Performance metrics
    console.log(`Performance Metrics (Mobile + Fast 3G):`);
    console.log(`  - Viewport: 375x667 (iPhone SE)`);
    console.log(`  - Network: Fast 3G (1.6 Mbps down, 750 Kbps up, 562.5ms latency)`);
    console.log(`  - Total workflow time: ${workflowTime.toFixed(2)}s`);
    console.log(`  - Target: < 30 seconds`);
    console.log(`  - Status: ${workflowTime < 30 ? 'PASS ✓' : 'FAIL ✗'}`);

    // Performance requirement: < 30 seconds
    expect(workflowTime).toBeLessThan(30);
  });

  /**
   * Test 5: Detailed performance breakdown
   * Measures individual steps to identify bottlenecks
   */
  test('should log detailed performance metrics for each workflow step', async ({
    page,
  }) => {
    // Enable Fast 3G network throttling
    await emulateFast3G(page);

    const metrics = {
      totalStart: Date.now(),
      navigationStart: 0,
      navigationEnd: 0,
      modalOpenStart: 0,
      modalOpenEnd: 0,
      formFillStart: 0,
      formFillEnd: 0,
      submitStart: 0,
      submitEnd: 0,
    };

    console.log('Starting detailed performance measurement with Fast 3G throttling...');

    // Step 1: Navigate to dashboard
    metrics.navigationStart = Date.now();
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    metrics.navigationEnd = Date.now();
    const navigationTime = (metrics.navigationEnd - metrics.navigationStart) / 1000;
    console.log(`  Step 1: Navigation to dashboard - ${navigationTime.toFixed(2)}s`);

    // Step 2: Open Quick Add modal
    metrics.modalOpenStart = Date.now();
    const quickAddButton = page.locator('[aria-label*="Přidat denní záznam"]').or(
      page.getByRole('button', { name: /zaznamenat vajíčka/i })
    );
    await expect(quickAddButton).toBeVisible();
    await quickAddButton.click();
    await expect(page.getByText('Rychlý záznam vajec')).toBeVisible();
    metrics.modalOpenEnd = Date.now();
    const modalOpenTime = (metrics.modalOpenEnd - metrics.modalOpenStart) / 1000;
    console.log(`  Step 2: Open Quick Add modal - ${modalOpenTime.toFixed(2)}s`);

    // Step 3: Fill in the form
    metrics.formFillStart = Date.now();
    const eggCount = 7;
    const incrementButton = page.getByLabel('egg count increase');
    for (let i = 0; i < eggCount; i++) {
      await incrementButton.click();
    }
    const eggCountInput = page.locator('input[type="number"][aria-label="egg count"]');
    await expect(eggCountInput).toHaveValue(eggCount.toString());
    const notesInput = page.getByLabel(/poznámky/i);
    await notesInput.fill('Performance breakdown test');
    metrics.formFillEnd = Date.now();
    const formFillTime = (metrics.formFillEnd - metrics.formFillStart) / 1000;
    console.log(`  Step 3: Fill form - ${formFillTime.toFixed(2)}s`);

    // Step 4: Submit and wait for response
    metrics.submitStart = Date.now();
    const createResponsePromise = page.waitForResponse(
      (response) =>
        response.url().includes('/api/flocks') &&
        response.url().includes('/daily-records') &&
        response.request().method() === 'POST' &&
        response.status() === 201
    );
    const saveButton = page.getByRole('button', { name: /uložit/i });
    await saveButton.click();
    const createResponse = await createResponsePromise;
    await expect(page.getByText('Rychlý záznam vajec')).not.toBeVisible();
    await expect(page.getByText(/úspěšně vytvořen/i)).toBeVisible();
    metrics.submitEnd = Date.now();
    const submitTime = (metrics.submitEnd - metrics.submitStart) / 1000;
    console.log(`  Step 4: Submit and confirm - ${submitTime.toFixed(2)}s`);

    // Total time
    const totalTime = (metrics.submitEnd - metrics.totalStart) / 1000;

    // Log complete breakdown
    console.log(`\nPerformance Breakdown Summary:`);
    console.log(`  - Network: Fast 3G (1.6 Mbps down, 750 Kbps up, 562.5ms latency)`);
    console.log(`  - Navigation: ${navigationTime.toFixed(2)}s (${((navigationTime / totalTime) * 100).toFixed(1)}%)`);
    console.log(`  - Modal Open: ${modalOpenTime.toFixed(2)}s (${((modalOpenTime / totalTime) * 100).toFixed(1)}%)`);
    console.log(`  - Form Fill: ${formFillTime.toFixed(2)}s (${((formFillTime / totalTime) * 100).toFixed(1)}%)`);
    console.log(`  - Submit & Confirm: ${submitTime.toFixed(2)}s (${((submitTime / totalTime) * 100).toFixed(1)}%)`);
    console.log(`  - TOTAL: ${totalTime.toFixed(2)}s`);
    console.log(`  - Target: < 30 seconds`);
    console.log(`  - Status: ${totalTime < 30 ? 'PASS ✓' : 'FAIL ✗'}`);

    // Verify API response
    const createdRecord = await createResponse.json();
    expect(createdRecord.id).toBeTruthy();

    // Performance requirement: < 30 seconds
    expect(totalTime).toBeLessThan(30);
  });
});
