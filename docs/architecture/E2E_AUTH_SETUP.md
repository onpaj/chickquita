# E2E Authentication Setup Guide

> **✅ Updated:** 2026-02-17 — Migrated to `@clerk/testing` (automated auth, no manual browser login)
> - Auth package: `@clerk/testing` ✓
> - Auth setup file: `e2e/clerk.setup.ts` ✓
> - Storage state path: `.clerk/user.json` configured in `playwright.config.ts` ✓
> - All test projects use `storageState: '.clerk/user.json'` with setup dependency ✓

The E2E tests use **`@clerk/testing`** for automated authentication. No manual browser interaction is required — authentication happens programmatically using Clerk's Testing Token mechanism.

## How It Works

1. `clerkSetup()` fetches a Testing Token from Clerk Backend API using `CLERK_SECRET_KEY`
2. `clerk.signIn()` signs in programmatically using the testing token (bypasses bot protection)
3. The authenticated browser state is saved to `.clerk/user.json`
4. All test projects reuse this state, so authentication runs only once per test run

## Prerequisites

- `@clerk/testing` package (installed as devDependency)
- A Clerk **development** instance (secret keys from production instances will fail)
- A test user account in your Clerk dev instance

## Local Development Setup

### Step 1: Create `.env.test.local`

Create `frontend/.env.test.local` (already gitignored via `*.local` pattern):

```env
CLERK_SECRET_KEY=sk_test_your_key_here
```

Get the `CLERK_SECRET_KEY` from your Clerk dashboard → API Keys → Secret keys.

> **⚠️ NEVER commit `CLERK_SECRET_KEY`** — keep it only in `.env.test.local`

### Step 2: Verify `.env.test`

`frontend/.env.test` should contain (already configured):

```env
CLERK_PUBLISHABLE_KEY=pk_test_...
E2E_CLERK_USER_USERNAME=your-test-user@example.com
E2E_CLERK_USER_PASSWORD=your-test-password
```

### Step 3: Run E2E tests

```bash
cd frontend

# Make sure dev server is running (in a separate terminal)
npm run dev

# In another terminal, run E2E tests
npm run test:e2e
```

The `clerk.setup.ts` project runs first, authenticates automatically, and saves state to `.clerk/user.json`. All other browser projects then reuse this state.

## CI/CD Setup (GitHub Actions)

Add these secrets to your GitHub repository (Settings → Secrets and variables → Actions):

| Secret | Description |
|--------|-------------|
| `CLERK_PUBLISHABLE_KEY` | Publishable key from Clerk dashboard |
| `CLERK_SECRET_KEY` | Secret key from Clerk dashboard (dev instance only!) |
| `E2E_CLERK_USER_USERNAME` | Test user email address |
| `E2E_CLERK_USER_PASSWORD` | Test user password |

These secrets are automatically passed to E2E test steps in both `ci-cd.yml` and `crossbrowser-tests.yml`.

## Troubleshooting

### `CLERK_SECRET_KEY is not set`

Create `frontend/.env.test.local` with your secret key:
```env
CLERK_SECRET_KEY=sk_test_...
```

### `E2E_CLERK_USER_USERNAME is not set`

Check `frontend/.env.test` has the test user credentials.

### `Authentication failed - still on authentication page`

- Verify the test user exists in your Clerk dev instance
- Confirm the email/password in `.env.test` are correct
- Make sure the test user's email is verified in Clerk

### `Secret key must be from development instance`

`@clerk/testing` only works with development Clerk instances. Do not use production secret keys.

### `Network issues / Testing token fetch failed`

- Check internet connectivity
- Verify `CLERK_PUBLISHABLE_KEY` matches your Clerk dev instance
- Check Clerk status at https://status.clerk.com

### `.clerk/user.json` not created

- Ensure the `setup` project runs first (check `dependencies: ['setup']` in configs)
- Look for errors in the setup project output during `npm run test:e2e`

## Security Notes

- `.clerk/user.json` contains authentication tokens — it's gitignored (`/.clerk/`)
- `.env.test` contains test credentials — it's gitignored
- `.env.test.local` contains `CLERK_SECRET_KEY` — gitignored via `*.local` pattern
- Use dedicated test accounts, not production accounts
- Only use development Clerk instances for testing
- Rotate test credentials periodically
