import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for the Login page (Clerk sign-in)
 */
export class LoginPage {
  readonly page: Page;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly submitButton: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    // Clerk uses specific identifiers for their hosted UI
    this.emailInput = page.locator('input[name="identifier"]').or(page.getByLabel(/email|e-mail/i).first());
    this.passwordInput = page.locator('input[name="password"][type="password"]');
    this.submitButton = page.locator('button[type="submit"]').first();
    this.errorMessage = page.locator('[role="alert"]').or(page.locator('.cl-formFieldErrorText'));
  }

  async goto() {
    await this.page.goto('/sign-in');
  }

  async login(email: string, password: string) {
    // Fill email
    await this.emailInput.fill(email);

    // Fill password
    await this.passwordInput.fill(password);

    // Submit the form - find the visible submit button
    const submitBtn = this.page.getByRole('button', { name: /continue|pokračovat|sign in|přihlásit/i }).filter({ hasText: /continue|pokračovat|sign in|přihlásit/i });
    await submitBtn.click();
  }

  async expectLoginSuccess() {
    // Wait for successful navigation (Clerk redirects after login)
    await this.page.waitForURL('/', { timeout: 10000 });
  }

  async hasError(): Promise<boolean> {
    return await this.errorMessage.isVisible().catch(() => false);
  }

  async getErrorMessage(): Promise<string> {
    return await this.errorMessage.textContent() || '';
  }
}
