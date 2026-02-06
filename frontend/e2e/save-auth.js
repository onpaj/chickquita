#!/usr/bin/env node
/**
 * Helper script to save authentication state for E2E tests
 *
 * Usage:
 * 1. Make sure the dev server is running (npm run dev)
 * 2. Run: node e2e/save-auth.js
 * 3. A browser will open - log in to the application
 * 4. After login, close the browser
 * 5. Auth state will be saved to .auth/user.json
 */

const { chromium } = require('@playwright/test');
const path = require('path');
const fs = require('fs');

(async () => {
  const authFile = path.join(__dirname, '..', '.auth', 'user.json');
  const baseURL = process.env.VITE_APP_URL || 'http://localhost:3100';

  console.log('🚀 Starting browser for manual login...');
  console.log(`📍 Opening: ${baseURL}`);
  console.log('');
  console.log('Instructions:');
  console.log('1. Log in to the application');
  console.log('2. Wait until you see the dashboard');
  console.log('3. Close the browser window');
  console.log('4. Auth state will be saved automatically');
  console.log('');

  const browser = await chromium.launch({
    headless: false,
    slowMo: 1000,
  });

  const context = await browser.newContext({
    baseURL,
  });

  const page = await context.newPage();

  // Navigate to the app
  await page.goto('/');

  // Wait for user to log in and navigate
  console.log('⏳ Waiting for you to log in...');

  // Wait for the page to be closed by the user
  await page.waitForEvent('close', { timeout: 0 }).catch(() => {});

  // Check if we're logged in by looking at the storage state
  const storageState = await context.storageState();

  if (storageState.cookies.length > 0 || storageState.origins.length > 0) {
    // Create .auth directory if it doesn't exist
    const authDir = path.dirname(authFile);
    if (!fs.existsSync(authDir)) {
      fs.mkdirSync(authDir, { recursive: true });
    }

    // Save storage state
    await context.storageState({ path: authFile });
    console.log('✅ Authentication state saved successfully!');
    console.log(`📁 Saved to: ${authFile}`);
    console.log('');
    console.log('You can now run E2E tests: npm run test:e2e');
  } else {
    console.log('⚠️  Warning: No authentication state detected.');
    console.log('Make sure you logged in before closing the browser.');
  }

  await browser.close();
})();
