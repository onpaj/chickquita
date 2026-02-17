/**
 * Clerk Authentication Setup for E2E Tests
 *
 * Uses @clerk/testing programmatic sign-in to bypass Clerk bot protection and MFA.
 * Requires:
 *   - E2E_CLERK_USER_USERNAME in frontend/.env.test
 *   - E2E_CLERK_USER_PASSWORD in frontend/.env.test (used only for UI fallback)
 *   - CLERK_SECRET_KEY in frontend/.env.test.local
 *   - CLERK_PUBLISHABLE_KEY in frontend/.env.test
 */

import { test as setup } from '@playwright/test';
import { clerkSetup, clerk } from '@clerk/testing/playwright';
import * as fs from 'fs';
import * as path from 'path';
import { fileURLToPath } from 'url';

// ES module equivalent of __dirname
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// Load environment variables from .env.test and .env.test.local
function loadEnvFile(filePath: string): void {
  if (!fs.existsSync(filePath)) return;
  const content = fs.readFileSync(filePath, 'utf-8');
  content.split('\n').forEach(line => {
    const trimmed = line.trim();
    if (trimmed && !trimmed.startsWith('#')) {
      const eqIndex = trimmed.indexOf('=');
      if (eqIndex > 0) {
        const key = trimmed.slice(0, eqIndex).trim();
        const value = trimmed.slice(eqIndex + 1).trim();
        if (!process.env[key]) {
          process.env[key] = value;
        }
      }
    }
  });
}

const frontendDir = path.join(__dirname, '..');
loadEnvFile(path.join(frontendDir, '.env.test'));
loadEnvFile(path.join(frontendDir, '.env.test.local'));

const clerkFile = '.clerk/user.json';

setup('authenticate with Clerk', async ({ page }) => {
  // Validate required environment variables
  const emailAddress = process.env.E2E_CLERK_USER_USERNAME;
  const secretKey = process.env.CLERK_SECRET_KEY;
  const publishableKey = process.env.CLERK_PUBLISHABLE_KEY;

  if (!emailAddress) {
    throw new Error(
      'E2E_CLERK_USER_USERNAME is not set. Add it to frontend/.env.test',
    );
  }
  if (!secretKey) {
    throw new Error(
      'CLERK_SECRET_KEY is not set. Add it to frontend/.env.test.local',
    );
  }

  console.log('🔐 Setting up Clerk testing token...');

  // Initialize Clerk testing token (fetches from Clerk Backend API)
  await clerkSetup({ publishableKey });

  console.log('🔐 Signing in via Clerk programmatic API...');

  // Navigate to a non-protected page that loads Clerk (required before clerk.signIn)
  await page.goto('/');
  await page.waitForLoadState('networkidle');

  // Sign in programmatically using email (creates sign-in token via backend API, bypasses MFA)
  await clerk.signIn({ page, emailAddress });

  // Navigate to dashboard to confirm authentication
  await page.goto('/dashboard');
  await page.waitForLoadState('networkidle', { timeout: 15000 });

  // Verify we're on the dashboard (not redirected back to sign-in)
  const currentURL = page.url();
  if (currentURL.includes('/sign-in') || currentURL.includes('/sign-up')) {
    throw new Error('Authentication failed - still on authentication page after sign-in attempt');
  }

  console.log('✅ Authentication successful');

  // Create .clerk directory if it doesn't exist
  const clerkDir = path.dirname(clerkFile);
  if (!fs.existsSync(clerkDir)) {
    fs.mkdirSync(clerkDir, { recursive: true });
  }

  // Save authenticated browser state (real session cookies) for reuse in all test projects
  await page.context().storageState({ path: clerkFile });

  console.log('💾 Auth state saved to .clerk/user.json');
});
