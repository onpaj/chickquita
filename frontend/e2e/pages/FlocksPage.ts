import { Page, Locator } from '@playwright/test';

export class FlocksPage {
  readonly page: Page;
  readonly pageTitle: Locator;
  readonly addFlockButton: Locator;
  readonly emptyStateMessage: Locator;
  readonly emptyStateAddButton: Locator;
  readonly flockCardsList: Locator;
  readonly activeFilterButton: Locator;
  readonly allFilterButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.pageTitle = page.getByRole('heading', { name: /hejna|flocks/i, level: 1 });
    this.addFlockButton = page.getByRole('button', { name: /přidat hejno|add flock/i }).first();
    this.emptyStateMessage = page.getByText(/zatím tu nejsou žádná hejna|no flocks yet/i);
    this.emptyStateAddButton = page.getByRole('button', { name: /přidat hejno|add flock/i }).first();
    this.flockCardsList = page.locator('[data-testid="flock-card"]');
    this.activeFilterButton = page.getByRole('button', { name: /^aktivní$|^active$/i });
    this.allFilterButton = page.getByRole('button', { name: /^vše$|^all$/i });
  }

  /**
   * Navigate to the flocks page for a specific coop
   */
  async goto(coopId: string) {
    await this.page.goto(`/coops/${coopId}/flocks`);
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Open the create flock modal
   */
  async openCreateFlockModal() {
    await this.addFlockButton.click();
    await this.page.waitForTimeout(500); // Wait for modal animation
  }

  /**
   * Get a flock card by its identifier
   */
  getFlockCard(identifier: string) {
    return this.page
      .locator('[data-testid="flock-card"]')
      .filter({ hasText: identifier });
  }

  /**
   * Get count of visible flock cards
   */
  async getFlockCount(): Promise<number> {
    return await this.flockCardsList.count();
  }

  /**
   * Open the action menu for a specific flock
   */
  async openFlockMenu(identifier: string) {
    const flockCard = this.getFlockCard(identifier);
    const menuButton = flockCard.getByRole('button', { name: /more|více/i });
    await menuButton.click();
    await this.page.waitForTimeout(300); // Wait for menu animation
  }

  /**
   * Click the edit menu item for a flock
   */
  async clickEditFlock(identifier: string) {
    await this.openFlockMenu(identifier);
    const editMenuItem = this.page.getByRole('menuitem', { name: /upravit|edit/i });
    await editMenuItem.click();
    await this.page.waitForTimeout(500); // Wait for modal animation
  }

  /**
   * Click the archive menu item for a flock
   */
  async clickArchiveFlock(identifier: string) {
    await this.openFlockMenu(identifier);
    const archiveMenuItem = this.page.getByRole('menuitem', { name: /archivovat|archive/i });
    await archiveMenuItem.click();
    await this.page.waitForTimeout(500); // Wait for dialog animation
  }

  /**
   * Click the view history menu item for a flock
   */
  async clickViewHistory(identifier: string) {
    await this.openFlockMenu(identifier);
    const historyMenuItem = this.page.getByRole('menuitem', { name: /zobrazit historii|view history/i });
    await historyMenuItem.click();
  }

  /**
   * Check if empty state is visible
   */
  async isEmptyStateVisible(): Promise<boolean> {
    return await this.emptyStateMessage.isVisible();
  }

  /**
   * Check if a flock is visible on the page
   */
  async isFlockVisible(identifier: string): Promise<boolean> {
    const flockCard = this.getFlockCard(identifier);
    return await flockCard.isVisible();
  }

  /**
   * Filter flocks by active status
   */
  async filterActive() {
    await this.activeFilterButton.click();
    await this.page.waitForTimeout(500); // Wait for filtering
  }

  /**
   * Filter to show all flocks
   */
  async filterAll() {
    await this.allFilterButton.click();
    await this.page.waitForTimeout(500); // Wait for filtering
  }

  /**
   * Get the status chip text for a specific flock
   */
  async getFlockStatus(identifier: string): Promise<string> {
    const flockCard = this.getFlockCard(identifier);
    const statusChip = flockCard.locator('.MuiChip-label');
    return await statusChip.textContent() || '';
  }

  /**
   * Get the composition counts for a specific flock
   */
  async getFlockComposition(identifier: string): Promise<{
    hens: number;
    roosters: number;
    chicks: number;
    total: number;
  }> {
    const flockCard = this.getFlockCard(identifier);

    // Extract numbers from the card text
    const cardText = await flockCard.textContent() || '';

    // Use regex to extract numbers
    const hensMatch = cardText.match(/Slepice:|Hens:.*?(\d+)/);
    const roostersMatch = cardText.match(/Kohouti:|Roosters:.*?(\d+)/);
    const chicksMatch = cardText.match(/Kuřata:|Chicks:.*?(\d+)/);
    const totalMatch = cardText.match(/Celkem:|Total:.*?(\d+)/);

    return {
      hens: hensMatch ? parseInt(hensMatch[1]) : 0,
      roosters: roostersMatch ? parseInt(roostersMatch[1]) : 0,
      chicks: chicksMatch ? parseInt(chicksMatch[1]) : 0,
      total: totalMatch ? parseInt(totalMatch[1]) : 0,
    };
  }

  /**
   * Wait for flock cards to load
   */
  async waitForFlocksToLoad() {
    // Wait for either empty state or flock cards to appear
    await this.page.waitForFunction(
      () => {
        const emptyState = document.querySelector('[role="region"]');
        const flockCards = document.querySelectorAll('[data-testid="flock-card"]');
        return (emptyState && emptyState.textContent?.includes('Zatím tu nejsou žádná hejna')) ||
               (emptyState && emptyState.textContent?.includes('No flocks yet')) ||
               flockCards.length > 0;
      },
      { timeout: 10000 }
    );
  }
}
