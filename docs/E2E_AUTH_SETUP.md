# E2E Authentication Setup Guide

> **✅ Configuration Verified:** 2026-02-09
> - npm scripts: `test:e2e:save-auth`, `test:e2e` exist ✓
> - Auth setup file: `e2e/auth.setup.ts` exists ✓
> - Manual auth script: `e2e/save-auth.js` exists ✓
> - Storage state path: `.auth/user.json` configured in playwright.config.ts ✓
> - All test projects use `storageState: '.auth/user.json'` with setup dependency ✓

The e2e tests require authentication. There are two ways to set this up:

## Option 1: Manual Authentication (Recommended for Local Development)

This is the quickest way to get started:

1. **Make sure the dev server is running:**
   ```bash
   npm run dev
   ```

2. **In a separate terminal, run the auth save script:**
   ```bash
   npm run test:e2e:save-auth
   ```

3. **A browser window will open automatically:**
   - Log in with your test account credentials
   - Wait until you see the dashboard (make sure you're fully logged in)
   - Close the browser window

4. **Authentication state is now saved** to `.auth/user.json`

5. **Run the tests:**
   ```bash
   npm run test:e2e
   ```

**Note:** You only need to do this once. The auth state will be reused for all test runs until it expires.

## Option 2: Automated Authentication (Recommended for CI/CD)

For automated testing without manual intervention:

1. **Create a test user account** in your Clerk dashboard (if you haven't already)

2. **Set environment variables:**

   **On macOS/Linux:**
   ```bash
   export TEST_USER_EMAIL="your-test-user@example.com"
   export TEST_USER_PASSWORD="your-test-password"
   ```

   **On Windows (PowerShell):**
   ```powershell
   $env:TEST_USER_EMAIL="your-test-user@example.com"
   $env:TEST_USER_PASSWORD="your-test-password"
   ```

   **Or add to `.env.test` file:**
   ```env
   TEST_USER_EMAIL=your-test-user@example.com
   TEST_USER_PASSWORD=your-test-password
   ```

3. **Run the tests:**
   ```bash
   npm run test:e2e
   ```

The auth setup will run automatically before tests and save the state to `.auth/user.json`.

## Troubleshooting

### "Authentication required" error
- **Solution:** Run `npm run test:e2e:save-auth` to create the auth state file manually

### Auth state expired
- **Solution:** Delete `.auth/user.json` and run the save-auth script again
  ```bash
  rm .auth/user.json
  npm run test:e2e:save-auth
  ```

### Tests fail with Clerk errors
- **Solution:** Make sure:
  - Backend is running on `http://localhost:5100`
  - Frontend is running on `http://localhost:3100`
  - Your test account has verified email
  - Clerk publishable key in `.env.development` is correct

## For CI/CD (GitHub Actions)

Add these secrets to your GitHub repository:
- `TEST_USER_EMAIL`
- `TEST_USER_PASSWORD`

The automated auth setup will handle authentication during CI test runs.

## Security Notes

- `.auth/user.json` contains authentication tokens - it's gitignored
- `.env.test` contains test credentials - it's gitignored
- Never commit these files to version control
- Use dedicated test accounts, not production accounts
- Rotate test credentials periodically
