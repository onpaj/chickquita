import { test, expect } from '@playwright/test';
import { CoopsPage } from './pages/CoopsPage';
import { FlocksPage } from './pages/FlocksPage';
import { CreateFlockModal } from './pages/CreateFlockModal';
import { EditFlockModal } from './pages/EditFlockModal';
import { ArchiveFlockDialog } from './pages/ArchiveFlockDialog';
import { CreateCoopModal } from './pages/CreateCoopModal';
import { generateFlockIdentifier, getDaysAgoDate } from './fixtures/flock.fixture';
import { generateCoopName } from './fixtures/coop.fixture';

/**
 * E2E Tests for Flock Internationalization (i18n)
 *
 * US-026 - Validate Internationalization for Flocks
 *
 * These tests verify that:
 * - All flock UI text uses translation keys (no hardcoded text)
 * - Czech translations exist for all flock text
 * - English translations exist for all flock text
 * - Form validation errors are localized
 * - Date formatting respects locale
 * - Number formatting respects locale
 */

// Run i18n tests only on chromium to avoid test duplication and speed up execution
test.describe('Flock Internationalization (i18n)', () => {
  test.describe.configure({ mode: 'serial' });
  // Only run on chromium for i18n validation
  test.use({ storageState: '.clerk/user.json' });
  let coopsPage: CoopsPage;
  let flocksPage: FlocksPage;
  let createFlockModal: CreateFlockModal;
  let editFlockModal: EditFlockModal;
  let archiveFlockDialog: ArchiveFlockDialog;
  let createCoopModal: CreateCoopModal;
  let testCoopId: string;
  let testCoopName: string;

  // Increase timeout for beforeEach setup
  test.beforeEach(async ({ page, context }) => {
    test.setTimeout(120000); // 2 minutes per test

    // Initialize page objects
    coopsPage = new CoopsPage(page);
    flocksPage = new FlocksPage(page);
    createFlockModal = new CreateFlockModal(page);
    editFlockModal = new EditFlockModal(page);
    archiveFlockDialog = new ArchiveFlockDialog(page);
    createCoopModal = new CreateCoopModal(page);

    // Create a test coop for flock tests
    await coopsPage.goto();
    await page.waitForLoadState('networkidle', { timeout: 30000 });

    testCoopName = generateCoopName('i18n Test Coop');

    // Intercept POST /api/coops to capture the coop ID
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
    await createCoopModal.createCoop(testCoopName, 'Test Location for i18n');
    await createCoopModal.waitForClose();

    // Wait for the coop ID to be captured from the API response
    await page.waitForTimeout(1000);
    testCoopId = createdCoopId || '';

    if (!testCoopId) {
      throw new Error(`Could not capture coop ID from API response`);
    }

    // Navigate directly to the coop's flocks page using the captured ID
    await page.goto(`/coops/${testCoopId}/flocks`);
    await page.waitForLoadState('networkidle', { timeout: 30000 });
  });

  /**
   * Helper function to change language via localStorage and reload
   */
  async function changeLanguage(page: any, language: 'cs' | 'en') {
    await page.evaluate((lang: string) => {
      localStorage.setItem('i18nextLng', lang);
    }, language);
    await page.reload();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000); // Give time for i18n to initialize
  }

  test.describe('Czech Language (cs)', () => {
    test('should display all flock UI elements in Czech', async ({ page }) => {
      // Set Czech language
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);

      // Verify page title
      await expect(flocksPage.pageTitle).toHaveText('Hejna');

      // Verify empty state in Czech
      await expect(flocksPage.emptyStateMessage).toContainText('Zatím tu nejsou žádná hejna');

      // Verify filter buttons
      const activeButton = page.getByRole('button', { name: 'Aktivní' });
      const allButton = page.getByRole('button', { name: 'Vše' });
      await expect(activeButton).toBeVisible();
      await expect(allButton).toBeVisible();

      // Verify FAB button has Czech aria-label
      await expect(flocksPage.addFlockButton).toHaveAttribute('aria-label', 'Přidat hejno');
    });

    test('should display create modal in Czech', async ({ page }) => {
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Verify modal title
      await expect(createFlockModal.modalTitle).toHaveText('Přidat hejno');

      // Verify form labels
      const dialog = page.getByRole('dialog');
      await expect(dialog.getByText('Identifikátor hejna').first()).toBeVisible();
      await expect(dialog.getByText('Datum líhnutí').first()).toBeVisible();
      await expect(dialog.getByText('Složení hejna').first()).toBeVisible();
      await expect(dialog.getByText('Slepice').first()).toBeVisible();
      await expect(dialog.getByText('Kohouti').first()).toBeVisible();
      await expect(dialog.getByText('Kuřata').first()).toBeVisible();

      // Verify buttons
      await expect(createFlockModal.submitButton).toHaveText('Uložit');
      await expect(createFlockModal.cancelButton).toHaveText('Zrušit');
    });

    test('should show validation errors in Czech', async ({ page }) => {
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Trigger empty identifier validation
      await createFlockModal.identifierInput.fill('');
      await createFlockModal.identifierInput.blur();
      await page.waitForTimeout(500);

      // Check for Czech validation error
      const errorText = await page.textContent('body');
      expect(errorText).toContain('Toto pole je povinné');

      // Test future date validation
      await createFlockModal.identifierInput.fill(generateFlockIdentifier());
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      const futureDate = tomorrow.toISOString().split('T')[0];

      await createFlockModal.hatchDateInput.fill(futureDate);
      await createFlockModal.hatchDateInput.blur();
      await page.waitForTimeout(500);

      // Check for Czech future date error
      const bodyText = await page.textContent('body');
      expect(bodyText).toContain('Datum nemůže být v budoucnosti');
    });

    test('should display flock card in Czech', async ({ page }) => {
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);

      // Create a flock
      const flockData = {
        identifier: generateFlockIdentifier(),
        hatchDate: getDaysAgoDate(30),
        hens: 10,
        roosters: 2,
        chicks: 5,
      };

      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);
      await flocksPage.waitForFlocksToLoad();

      // Verify flock card labels in Czech
      const flockCard = page.getByTestId('flock-card').first();
      await expect(flockCard.getByText('Slepice')).toBeVisible();
      await expect(flockCard.getByText('Kohouti')).toBeVisible();
      await expect(flockCard.getByText('Kuřata')).toBeVisible();
      await expect(flockCard.getByText('Celkem')).toBeVisible();
      await expect(flockCard.getByText('Aktivní')).toBeVisible();
    });

    test('should display archive dialog in Czech', async ({ page }) => {
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);

      // Create a flock
      const flockData = {
        identifier: generateFlockIdentifier(),
        hatchDate: getDaysAgoDate(30),
        hens: 10,
        roosters: 2,
        chicks: 5,
      };

      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);
      await flocksPage.waitForFlocksToLoad();

      // Open archive dialog
      await flocksPage.clickArchiveFlock(flockData.identifier);

      // Verify dialog text in Czech
      await expect(archiveFlockDialog.dialogTitle).toHaveText('Archivovat hejno?');
      await expect(page.getByText(/archivováno a odstraněno z vašeho aktivního seznamu/)).toBeVisible();
    });
  });

  test.describe('English Language (en)', () => {
    test('should display all flock UI elements in English', async ({ page }) => {
      // Set English language
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);

      // Verify page title
      await expect(flocksPage.pageTitle).toHaveText('Flocks');

      // Verify empty state in English
      await expect(flocksPage.emptyStateMessage).toContainText('No flocks yet');

      // Verify filter buttons
      const activeButton = page.getByRole('button', { name: 'Active' });
      const allButton = page.getByRole('button', { name: 'All' });
      await expect(activeButton).toBeVisible();
      await expect(allButton).toBeVisible();

      // Verify FAB button has English aria-label
      await expect(flocksPage.addFlockButton).toHaveAttribute('aria-label', 'Add Flock');
    });

    test('should display create modal in English', async ({ page }) => {
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Verify modal title
      await expect(createFlockModal.modalTitle).toHaveText('Add Flock');

      // Verify form labels
      const dialog = page.getByRole('dialog');
      await expect(dialog.getByText('Flock Identifier').first()).toBeVisible();
      await expect(dialog.getByText('Hatch Date').first()).toBeVisible();
      await expect(dialog.getByText('Flock Composition').first()).toBeVisible();
      await expect(dialog.getByText('Hens').first()).toBeVisible();
      await expect(dialog.getByText('Roosters').first()).toBeVisible();
      await expect(dialog.getByText('Chicks').first()).toBeVisible();

      // Verify buttons
      await expect(createFlockModal.submitButton).toHaveText('Save');
      await expect(createFlockModal.cancelButton).toHaveText('Cancel');
    });

    test('should show validation errors in English', async ({ page }) => {
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Trigger empty identifier validation
      await createFlockModal.identifierInput.fill('');
      await createFlockModal.identifierInput.blur();
      await page.waitForTimeout(500);

      // Check for English validation error
      const errorText = await page.textContent('body');
      expect(errorText).toContain('This field is required');

      // Test future date validation
      await createFlockModal.identifierInput.fill(generateFlockIdentifier());
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      const futureDate = tomorrow.toISOString().split('T')[0];

      await createFlockModal.hatchDateInput.fill(futureDate);
      await createFlockModal.hatchDateInput.blur();
      await page.waitForTimeout(500);

      // Check for English future date error
      const bodyText = await page.textContent('body');
      expect(bodyText).toContain('Date cannot be in the future');
    });

    test('should display flock card in English', async ({ page }) => {
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);

      // Create a flock
      const flockData = {
        identifier: generateFlockIdentifier(),
        hatchDate: getDaysAgoDate(30),
        hens: 10,
        roosters: 2,
        chicks: 5,
      };

      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);
      await flocksPage.waitForFlocksToLoad();

      // Verify flock card labels in English
      const flockCard = page.getByTestId('flock-card').first();
      await expect(flockCard.getByText('Hens')).toBeVisible();
      await expect(flockCard.getByText('Roosters')).toBeVisible();
      await expect(flockCard.getByText('Chicks')).toBeVisible();
      await expect(flockCard.getByText('Total')).toBeVisible();
      await expect(flockCard.getByText('Active')).toBeVisible();
    });

    test('should display archive dialog in English', async ({ page }) => {
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);

      // Create a flock
      const flockData = {
        identifier: generateFlockIdentifier(),
        hatchDate: getDaysAgoDate(30),
        hens: 10,
        roosters: 2,
        chicks: 5,
      };

      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);
      await flocksPage.waitForFlocksToLoad();

      // Open archive dialog
      await flocksPage.clickArchiveFlock(flockData.identifier);

      // Verify dialog text in English
      await expect(archiveFlockDialog.dialogTitle).toHaveText('Archive Flock?');
      await expect(page.getByText(/archived and removed from your active list/)).toBeVisible();
    });
  });

  test.describe('Language Switching', () => {
    test('should switch from Czech to English dynamically', async ({ page }) => {
      // Start with Czech
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);
      await expect(flocksPage.pageTitle).toHaveText('Hejna');

      // Switch to English
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);
      await expect(flocksPage.pageTitle).toHaveText('Flocks');

      // Switch back to Czech
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);
      await expect(flocksPage.pageTitle).toHaveText('Hejna');
    });

    test('should persist language preference across page reloads', async ({ page }) => {
      // Set English language
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);
      await expect(flocksPage.pageTitle).toHaveText('Flocks');

      // Reload page
      await page.reload();
      await page.waitForLoadState('networkidle');

      // Language should still be English
      await expect(flocksPage.pageTitle).toHaveText('Flocks');
    });
  });

  test.describe('Number Formatting', () => {
    test('should display animal counts as integers (no decimals)', async ({ page }) => {
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);

      // Create a flock with specific counts
      const flockData = {
        identifier: generateFlockIdentifier(),
        hatchDate: getDaysAgoDate(30),
        hens: 123,
        roosters: 45,
        chicks: 67,
      };

      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);
      await flocksPage.waitForFlocksToLoad();

      // Verify numbers are displayed without decimals
      const composition = await flocksPage.getFlockComposition(flockData.identifier);
      expect(composition.hens).toBe(123);
      expect(composition.roosters).toBe(45);
      expect(composition.chicks).toBe(67);
      expect(composition.total).toBe(235);

      // Verify numbers are displayed as plain integers (no thousand separators for small numbers)
      const flockCard = await flocksPage.getFlockCard(flockData.identifier);
      const cardText = await flockCard.textContent();
      expect(cardText).toContain('123');
      expect(cardText).toContain('45');
      expect(cardText).toContain('67');
      expect(cardText).toContain('235');
    });

    test('should accept only non-negative integers in form inputs', async ({ page }) => {
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Fill identifier and date
      await createFlockModal.identifierInput.fill(generateFlockIdentifier());
      await createFlockModal.hatchDateInput.fill(getDaysAgoDate(30));

      // Try to enter negative numbers - the input should prevent it or show error
      await createFlockModal.hensInput.fill('-5');
      await createFlockModal.hensInput.blur();

      // The form should either prevent negative input or disable submit
      const submitButton = createFlockModal.submitButton;
      const isDisabled = await submitButton.isDisabled();

      // If not disabled, the input should have been corrected to 0 or positive
      if (!isDisabled) {
        const hensValue = await createFlockModal.hensInput.inputValue();
        expect(parseInt(hensValue, 10)).toBeGreaterThanOrEqual(0);
      }
    });
  });

  test.describe('Date Formatting', () => {
    test('should use HTML date input (browser locale)', async ({ page }) => {
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Verify hatch date input is of type date
      await expect(createFlockModal.hatchDateInput).toHaveAttribute('type', 'date');

      // HTML date inputs use browser's locale automatically
      // The format will be handled by the browser (dd.mm.yyyy for Czech, mm/dd/yyyy for English)
    });

    test('should not accept future dates', async ({ page }) => {
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);
      await flocksPage.openCreateFlockModal();

      // Fill identifier
      await createFlockModal.identifierInput.fill(generateFlockIdentifier());

      // Try to enter a future date
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      const futureDate = tomorrow.toISOString().split('T')[0];

      await createFlockModal.hatchDateInput.fill(futureDate);
      await createFlockModal.hatchDateInput.blur();
      await page.waitForTimeout(500);

      // Verify Czech error message appears
      const errorText = await page.textContent('body');
      expect(errorText).toContain('Datum nemůže být v budoucnosti');
    });
  });

  test.describe('No Hardcoded Text', () => {
    test('should not contain hardcoded English text in Czech mode', async ({ page }) => {
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);

      // Get all visible text on the page
      const pageText = await page.textContent('body');

      // These English words should NOT appear when in Czech mode
      const forbiddenEnglishWords = [
        'Add Flock',
        'Edit Flock',
        'Archive Flock',
        'Flock Identifier',
        'Hatch Date',
        'Flock Composition',
        'This field is required',
        'Date cannot be in the future',
        'Active',
        'Archived',
      ];

      for (const word of forbiddenEnglishWords) {
        expect(pageText).not.toContain(word);
      }
    });

    test('should not contain hardcoded Czech text in English mode', async ({ page }) => {
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);

      // Get all visible text on the page
      const pageText = await page.textContent('body');

      // These Czech words should NOT appear when in English mode
      const forbiddenCzechWords = [
        'Přidat hejno',
        'Upravit hejno',
        'Archivovat hejno',
        'Identifikátor hejna',
        'Datum líhnutí',
        'Složení hejna',
        'Toto pole je povinné',
        'Datum nemůže být v budoucnosti',
        'Aktivní',
        'Archivováno',
      ];

      for (const word of forbiddenCzechWords) {
        expect(pageText).not.toContain(word);
      }
    });
  });

  test.describe('Accessibility Labels (ARIA)', () => {
    test('should have localized ARIA labels in Czech', async ({ page }) => {
      await changeLanguage(page, 'cs');
      await flocksPage.goto(testCoopId);

      // Create a flock to test card aria-label
      const flockData = {
        identifier: generateFlockIdentifier(),
        hatchDate: getDaysAgoDate(30),
        hens: 10,
        roosters: 2,
        chicks: 5,
      };

      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);
      await flocksPage.waitForFlocksToLoad();

      // Check FAB button aria-label
      await expect(flocksPage.addFlockButton).toHaveAttribute('aria-label', 'Přidat hejno');

      // Check filter toggle aria-label
      const filterToggle = page.locator('[aria-label="Filtrovat podle stavu"]');
      await expect(filterToggle).toBeVisible();

      // Open create modal and check increment/decrement button aria-labels
      await flocksPage.openCreateFlockModal();
      const increaseButtons = page.locator('[aria-label="Zvýšit"]');
      const decreaseButtons = page.locator('[aria-label="Snížit"]');

      // Should have 3 pairs of buttons (hens, roosters, chicks)
      await expect(increaseButtons).toHaveCount(3);
      await expect(decreaseButtons).toHaveCount(3);
    });

    test('should have localized ARIA labels in English', async ({ page }) => {
      await changeLanguage(page, 'en');
      await flocksPage.goto(testCoopId);

      // Create a flock to test card aria-label
      const flockData = {
        identifier: generateFlockIdentifier(),
        hatchDate: getDaysAgoDate(30),
        hens: 10,
        roosters: 2,
        chicks: 5,
      };

      await flocksPage.openCreateFlockModal();
      await createFlockModal.createFlock(flockData);
      await flocksPage.waitForFlocksToLoad();

      // Check FAB button aria-label
      await expect(flocksPage.addFlockButton).toHaveAttribute('aria-label', 'Add Flock');

      // Check filter toggle aria-label
      const filterToggle = page.locator('[aria-label="Filter by status"]');
      await expect(filterToggle).toBeVisible();

      // Open create modal and check increment/decrement button aria-labels
      await flocksPage.openCreateFlockModal();
      const increaseButtons = page.locator('[aria-label="Increase"]');
      const decreaseButtons = page.locator('[aria-label="Decrease"]');

      // Should have 3 pairs of buttons (hens, roosters, chicks)
      await expect(increaseButtons).toHaveCount(3);
      await expect(decreaseButtons).toHaveCount(3);
    });
  });
});
