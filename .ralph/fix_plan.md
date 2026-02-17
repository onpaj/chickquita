# Ralph Fix Plan - Migrate E2E Auth to @clerk/testing

**PRD:** `tasks/prd-e2e-clerk-testing.md`
**Goal:** Replace fragile Clerk UI-based E2E authentication with `@clerk/testing` package. Eliminate manual login, remove legacy auth scripts, update all configs and workflows. After migration, `npm run test:e2e` must authenticate automatically without any manual browser interaction.
**Browser:** `http://localhost:3100` (must be running before starting)

## Constraints

- **Never commit `CLERK_SECRET_KEY`** — it must only exist in `.env.test.local` (gitignored) or CI secrets
- **Do not fix failing E2E tests** — only migrate auth infrastructure; the 195 failing tests are tracked in `fix_plan-e2etests.md`
- **Verify auth works after each step** — run a quick smoke test to confirm authentication still functions
- **Never use `git push` without explicit user instruction**
- **Do not change test logic in spec files** — only update auth-related paths (`.auth/` → `.clerk/`)
- **Preserve all existing Playwright config structure** — only change auth-related settings
- **Check @clerk/testing docs** for the correct API before writing code — the package API may differ from assumptions

---

## Priority Order

P0 is the core migration (install + new setup + config). P1 updates all consumers. P2 is cleanup and CI. P3 is docs.

---

## P0 — Core Migration (New Auth Infrastructure)

### STEP-001: Install @clerk/testing package

**Source:** PRD US-001
**Files:** `frontend/package.json`

- [ ] Run `cd frontend && npm install @clerk/testing --save-dev`
- [ ] Verify `@clerk/testing` appears in `devDependencies` in `package.json`
- [ ] Verify no version conflicts with `@clerk/clerk-react@^5.60.0`
- [ ] Run `npx tsc --noEmit` — confirm zero TypeScript errors

---

### STEP-002: Configure environment variables

**Source:** PRD US-006
**Files:** `frontend/.env.test`

- [ ] Read current `frontend/.env.test` to understand existing content
- [ ] Update `frontend/.env.test` with new env var names:
  - `CLERK_PUBLISHABLE_KEY` (same value as `VITE_CLERK_PUBLISHABLE_KEY`)
  - `E2E_CLERK_USER_USERNAME` (test user email)
  - `E2E_CLERK_USER_PASSWORD` (test user password)
- [ ] Remove old env var names: `TEST_USER_EMAIL`, `TEST_USER_PASSWORD`
- [ ] Verify `*.local` pattern in `frontend/.gitignore` covers `.env.test.local`
- [ ] **DO NOT** put `CLERK_SECRET_KEY` in `.env.test` — only in `.env.test.local`
- [ ] Verify `.env.test.local` exists locally with `CLERK_SECRET_KEY=sk_test_...` (ask user if missing)

---

### STEP-003: Create new clerk.setup.ts

**Source:** PRD US-002
**Files:** `frontend/e2e/clerk.setup.ts` (new file)

- [ ] Check `@clerk/testing` docs: verify correct import paths for `clerkSetup` and `clerk` from `@clerk/testing/playwright`
- [ ] Create `frontend/e2e/clerk.setup.ts` with:
  - Load `.env.test` and `.env.test.local` env vars
  - Call `clerkSetup()` to obtain Testing Token
  - Call `clerk.signIn()` with `strategy: 'password'`, using `E2E_CLERK_USER_USERNAME` and `E2E_CLERK_USER_PASSWORD`
  - Navigate to app and wait for authenticated redirect
  - Save browser state to `.clerk/user.json` via `page.context().storageState()`
  - Create `.clerk/` directory if it doesn't exist
  - Throw clear error if required env vars are missing
- [ ] Run `npx tsc --noEmit` — confirm no TypeScript errors in the new file

---

### STEP-004: Update main playwright.config.ts

**Source:** PRD US-003
**Files:** `frontend/playwright.config.ts`

- [ ] Change setup project `testMatch` from `/.*\.setup\.ts/` to target `clerk.setup.ts` specifically
- [ ] Change all `storageState: '.auth/user.json'` to `storageState: '.clerk/user.json'` (5 occurrences: chromium, firefox, webkit, Mobile Chrome, Mobile Safari)
- [ ] Verify all browser projects still depend on the setup project
- [ ] Run `npx playwright test --project=setup --project=chromium --grep "dashboard"` (or similar) — verify new auth setup runs and at least one authenticated page loads

---

### STEP-005: Update crossbrowser playwright.crossbrowser.config.ts

**Source:** PRD US-004
**Files:** `frontend/playwright.crossbrowser.config.ts`

- [ ] Change setup project `testMatch` from `/.*\.setup\.ts/` to target `clerk.setup.ts`
- [ ] Change shared `use.storageState` from `.auth/user.json` to `.clerk/user.json` (line 128)
- [ ] Verify all browser/device/breakpoint projects still depend on `setup`

---

## P1 — Update All Consumers

### STEP-006: Update test files with hardcoded auth paths

**Source:** PRD US-005
**Files:** `frontend/e2e/flocks-i18n.spec.ts`

- [ ] Change `test.use({ storageState: '.auth/user.json' })` to `test.use({ storageState: '.clerk/user.json' })` in `flocks-i18n.spec.ts` (line 28)
- [ ] Search all `e2e/**/*.spec.ts` for remaining `.auth/` references — fix any found
- [ ] Search all `e2e/**/*.spec.ts` for `auth.setup` references — confirm none remain

---

### STEP-007: Update .gitignore

**Source:** PRD US-007
**Files:** `frontend/.gitignore`

- [ ] Add `/.clerk/` entry to `frontend/.gitignore`
- [ ] Verify `*.local` pattern already covers `.env.test.local` (line 4: `*.local` exists)
- [ ] Keep `/.auth/` entry for safety (old directories may still exist locally)

---

## P2 — Cleanup & CI

### STEP-008: Remove legacy auth files

**Source:** PRD US-008
**Files:** `frontend/e2e/auth.setup.ts`, `frontend/e2e/save-auth.js`, `frontend/e2e/refresh-auth.mjs`, `frontend/package.json`

- [ ] Delete `frontend/e2e/auth.setup.ts`
- [ ] Delete `frontend/e2e/save-auth.js`
- [ ] Delete `frontend/e2e/refresh-auth.mjs`
- [ ] Remove `"test:e2e:save-auth"` script from `frontend/package.json` (line 19)
- [ ] Search entire codebase for references to deleted files — fix any remaining imports or references
- [ ] Delete `frontend/.auth/` directory if it exists locally

---

### STEP-009: Update CI/CD workflow (ci-cd.yml)

**Source:** PRD US-009
**Files:** `.github/workflows/ci-cd.yml`

- [ ] Add env vars to the `e2e-tests` job's "Run E2E tests" step (around line 196):
  ```yaml
  env:
    CI: true
    CLERK_PUBLISHABLE_KEY: ${{ secrets.CLERK_PUBLISHABLE_KEY }}
    CLERK_SECRET_KEY: ${{ secrets.CLERK_SECRET_KEY }}
    E2E_CLERK_USER_USERNAME: ${{ secrets.E2E_CLERK_USER_USERNAME }}
    E2E_CLERK_USER_PASSWORD: ${{ secrets.E2E_CLERK_USER_PASSWORD }}
  ```
- [ ] Verify no other jobs are affected
- [ ] Verify the existing `continue-on-error: false` is preserved

---

### STEP-010: Update crossbrowser workflow (crossbrowser-tests.yml)

**Source:** PRD US-010
**Files:** `.github/workflows/crossbrowser-tests.yml`

- [ ] Add Clerk env vars to `test-chrome` job's test step (around line 118):
  ```yaml
  env:
    CI: true
    CLERK_PUBLISHABLE_KEY: ${{ secrets.CLERK_PUBLISHABLE_KEY }}
    CLERK_SECRET_KEY: ${{ secrets.CLERK_SECRET_KEY }}
    E2E_CLERK_USER_USERNAME: ${{ secrets.E2E_CLERK_USER_USERNAME }}
    E2E_CLERK_USER_PASSWORD: ${{ secrets.E2E_CLERK_USER_PASSWORD }}
  ```
- [ ] Add same env vars to `test-firefox` job's test step (around line 178)
- [ ] Add same env vars to `test-safari` job's test step (around line 238)
- [ ] Add same env vars to `test-mobile` job's test step (around line 296)
- [ ] Add same env vars to `test-breakpoints` job's test step (around line 355)
- [ ] Verify all existing `CI: true` env vars are preserved

---

## P3 — Documentation

### STEP-011: Update E2E documentation

**Source:** PRD US-011
**Files:** `docs/architecture/E2E_AUTH_SETUP.md`, `frontend/e2e/README.md`, `frontend/e2e/TEST_SETUP.md`

- [ ] Rewrite `docs/architecture/E2E_AUTH_SETUP.md`:
  - Prerequisites: `@clerk/testing` package, `.env.test.local` with `CLERK_SECRET_KEY`
  - Local setup: Just create `.env.test.local` and run `npm run test:e2e`
  - CI setup: List required GitHub Secrets
  - Troubleshooting: Missing secret key, expired tokens, network issues
- [ ] Update `frontend/e2e/README.md` — replace old auth instructions with new `@clerk/testing` approach
- [ ] Update `frontend/e2e/TEST_SETUP.md` — replace `.auth/user.json` references with `.clerk/user.json`, remove save-auth instructions
- [ ] Remove references to `save-auth.js`, `refresh-auth.mjs`, and manual browser login from all docs

---

## Final Verification

After completing all steps:

- [ ] Run `npx tsc --noEmit` in `frontend/` — zero TypeScript errors
- [ ] Run `npm run lint` in `frontend/` — zero lint errors
- [ ] Run `npx playwright test --project=chromium` — auth setup completes successfully
- [ ] Verify `.clerk/user.json` is created with valid cookies and localStorage
- [ ] Verify at least one authenticated test passes (dashboard loads)
- [ ] Verify no references to old files remain: search for `auth.setup`, `save-auth`, `refresh-auth`, `.auth/user.json`
- [ ] Verify `CLERK_SECRET_KEY` does not appear in any committed file
- [ ] Commit changes with appropriate message

---

## Progress Summary

### Current Status
- **Steps**: 11
- **Completed**: 0 / 11
- **P0 Core Migration**: 0 / 5 (STEP-001 through STEP-005)
- **P1 Consumers**: 0 / 2 (STEP-006 through STEP-007)
- **P2 Cleanup & CI**: 0 / 3 (STEP-008 through STEP-010)
- **P3 Documentation**: 0 / 1 (STEP-011)

### Step → Files Mapping
| Step | Description | Files Affected |
|------|-------------|----------------|
| STEP-001 | Install @clerk/testing | `package.json` |
| STEP-002 | Configure env vars | `.env.test` |
| STEP-003 | Create clerk.setup.ts | `e2e/clerk.setup.ts` (new) |
| STEP-004 | Update main Playwright config | `playwright.config.ts` |
| STEP-005 | Update crossbrowser config | `playwright.crossbrowser.config.ts` |
| STEP-006 | Update hardcoded auth paths in tests | `e2e/flocks-i18n.spec.ts` |
| STEP-007 | Update .gitignore | `.gitignore` |
| STEP-008 | Remove legacy auth files | `e2e/auth.setup.ts`, `e2e/save-auth.js`, `e2e/refresh-auth.mjs`, `package.json` |
| STEP-009 | Update CI/CD workflow | `.github/workflows/ci-cd.yml` |
| STEP-010 | Update crossbrowser workflow | `.github/workflows/crossbrowser-tests.yml` |
| STEP-011 | Update documentation | `docs/architecture/E2E_AUTH_SETUP.md`, `e2e/README.md`, `e2e/TEST_SETUP.md` |
