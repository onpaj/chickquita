/**
 * Authentication setup for E2E tests
 *
 * This setup runs before all tests to create an authenticated session.
 *
 * Option 1 (Automated - Recommended for CI/CD):
 * Set environment variables in .env.test file:
 *   TEST_USER_EMAIL=your-test-user@example.com
 *   TEST_USER_PASSWORD=your-password
 *
 * Option 2 (Manual - Recommended for local development):
 * Run: npm run test:e2e:save-auth
 * Then log in manually when the browser opens.
 */

import { test as setup, expect } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';
import { fileURLToPath } from 'url';

// ES module equivalent of __dirname
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Load environment variables from .env.test if it exists
const envTestPath = path.join(__dirname, '..', '.env.test');
if (fs.existsSync(envTestPath)) {
  const envContent = fs.readFileSync(envTestPath, 'utf-8');
  envContent.split('\n').forEach(line => {
    const trimmed = line.trim();
    if (trimmed && !trimmed.startsWith('#')) {
      const [key, ...valueParts] = trimmed.split('=');
      if (key && valueParts.length > 0) {
        const value = valueParts.join('=').trim();
        if (!process.env[key]) {
          process.env[key] = value;
        }
      }
    }
  });
}

const authFile = '.auth/user.json';

setup('authenticate', async ({ page }) => {
  // Check if auth file already exists
  if (fs.existsSync(authFile)) {
    console.log('✅ Using existing auth state from .auth/user.json');
    return;
  }

  // Try automated authentication if credentials are provided
  const email = process.env.TEST_USER_EMAIL;
  const password = process.env.TEST_USER_PASSWORD;

  if (!email || !password) {
    console.log('⚠️  No auth state found and TEST_USER_EMAIL/TEST_USER_PASSWORD not set');
    console.log('📝 To fix this, run: npm run test:e2e:save-auth');
    console.log('   Or set TEST_USER_EMAIL and TEST_USER_PASSWORD environment variables');
    throw new Error('Authentication required. Please run "npm run test:e2e:save-auth" or set TEST_USER_EMAIL and TEST_USER_PASSWORD environment variables.');
  }

  console.log('🔐 Setting up authentication...');

  await page.goto('/sign-in');

  // Wait for Clerk sign-in form to load
  await page.waitForLoadState('networkidle');

  // Wait for the email input to be visible
  const emailInput = page.locator('input[name="identifier"]');
  await emailInput.waitFor({ state: 'visible' });

  // Fill in email
  await emailInput.fill(email);

  // Fill in password (Clerk shows both fields together)
  const passwordInput = page.locator('input[name="password"]').filter({ hasText: '' });
  await passwordInput.waitFor({ state: 'visible', timeout: 5000 });
  await passwordInput.fill(password);

  // Click the submit button (use text matcher for Czech "Pokračovat" or English "Continue")
  const submitButton = page.getByRole('button', { name: /pokračovat|continue/i });
  await submitButton.waitFor({ state: 'visible' });
  await submitButton.click();

  // Wait for redirect to dashboard (Clerk redirects to /dashboard after login)
  await page.waitForURL(/\/(dashboard)?$/, { timeout: 15000 });

  // Verify we're logged in by checking we're not on sign-in page
  const currentURL = page.url();
  if (currentURL.includes('/sign-in') || currentURL.includes('/sign-up')) {
    throw new Error('Login failed - still on authentication page');
  }

  // Create .auth directory if it doesn't exist
  const authDir = path.dirname(authFile);
  if (!fs.existsSync(authDir)) {
    fs.mkdirSync(authDir, { recursive: true });
  }

  // Save authentication state
  await page.context().storageState({ path: authFile });

  console.log('✅ Authentication successful - state saved to .auth/user.json');
});
