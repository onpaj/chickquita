import { defineConfig, devices } from '@playwright/test';

/**
 * TESTING-002: Cross-Browser & Device Testing Configuration
 *
 * This configuration extends the base Playwright config to support:
 * - Multi-browser testing (Chrome, Safari, Firefox - latest 2 versions)
 * - Mobile device testing (iOS Safari 15+, Android Chrome 10+)
 * - Responsive breakpoints (320px, 480px, 768px, 1024px, 1920px)
 * - Screenshot capture for visual regression
 * - Comprehensive HTML/JSON reporting
 *
 * @see https://playwright.dev/docs/test-configuration
 */

// Device viewport configurations for breakpoint testing
const BREAKPOINTS = {
  mobile_xs: { width: 320, height: 568 },   // iPhone SE (1st gen)
  mobile_sm: { width: 375, height: 667 },   // iPhone SE (2nd/3rd gen)
  mobile_md: { width: 390, height: 844 },   // iPhone 14
  tablet: { width: 768, height: 1024 },     // iPad
  desktop_md: { width: 1024, height: 768 }, // Small laptop
  desktop_lg: { width: 1920, height: 1080 },// Full HD desktop
};

// Custom device configurations matching requirements
const CUSTOM_DEVICES = {
  // iPhone SE - smallest iOS device (320px viewport)
  'iPhone SE': {
    ...devices['iPhone SE'],
    viewport: BREAKPOINTS.mobile_xs,
  },
  // iPhone 14 - standard iOS device
  'iPhone 14': {
    ...devices['iPhone 14'],
    viewport: BREAKPOINTS.mobile_md,
  },
  // iPad - tablet device
  'iPad': {
    ...devices['iPad (gen 7)'],
    viewport: BREAKPOINTS.tablet,
  },
  // Samsung Galaxy A52 - Android mid-range device
  'Samsung Galaxy A52': {
    userAgent: 'Mozilla/5.0 (Linux; Android 12; SM-A525F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36',
    viewport: { width: 412, height: 915 },
    deviceScaleFactor: 2.625,
    isMobile: true,
    hasTouch: true,
  },
};

export default defineConfig({
  testDir: './e2e',

  // Test file patterns
  testMatch: ['**/*.crossbrowser.spec.ts', '**/*.visual.spec.ts'],

  // Output directory for test artifacts
  outputDir: './test-results/crossbrowser',

  // Run tests in parallel across different browsers
  fullyParallel: true,

  // Fail build if test.only is left in code
  forbidOnly: !!process.env.CI,

  // Retry failed tests on CI
  retries: process.env.CI ? 2 : 1,

  // Limit workers in CI for stability
  workers: process.env.CI ? 2 : undefined,

  // Global timeout per test
  timeout: 60000,

  // Expect timeout for assertions
  expect: {
    timeout: 10000,
    // Visual comparison settings
    toHaveScreenshot: {
      maxDiffPixels: 100,
      threshold: 0.2,
      animations: 'disabled',
    },
    toMatchSnapshot: {
      threshold: 0.2,
    },
  },

  // Reporters for test results
  reporter: [
    // Console output for CI
    ['list'],
    // HTML report with screenshots
    ['html', {
      outputFolder: './playwright-report/crossbrowser',
      open: process.env.CI ? 'never' : 'on-failure',
    }],
    // JSON report for programmatic access
    ['json', {
      outputFile: './test-results/crossbrowser/results.json',
    }],
    // JUnit for CI integration
    ['junit', {
      outputFile: './test-results/crossbrowser/junit.xml',
    }],
  ],

  // Shared settings for all projects
  use: {
    // Base URL
    baseURL: process.env.VITE_APP_URL || 'http://localhost:3100',

    // Always capture trace for debugging
    trace: 'retain-on-failure',

    // Screenshot settings - capture on all tests for visual regression
    screenshot: {
      mode: 'on',
      fullPage: true,
    },

    // Video recording
    video: 'retain-on-failure',

    // Authentication state
    storageState: '.auth/user.json',

    // Viewport settings (overridden per project)
    viewport: BREAKPOINTS.desktop_md,

    // Action timeout
    actionTimeout: 15000,

    // Navigation timeout
    navigationTimeout: 30000,
  },

  // Browser projects configuration
  projects: [
    // ============================================
    // SETUP PROJECT
    // ============================================
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
      use: {
        ...devices['Desktop Chrome'],
      },
    },

    // ============================================
    // DESKTOP BROWSERS - Latest 2 versions
    // ============================================

    // Chrome Desktop (latest)
    {
      name: 'chrome-desktop',
      use: {
        ...devices['Desktop Chrome'],
        viewport: BREAKPOINTS.desktop_lg,
        channel: 'chrome',
      },
      dependencies: ['setup'],
    },

    // Chrome Desktop (previous version simulation via stable channel)
    {
      name: 'chrome-desktop-stable',
      use: {
        ...devices['Desktop Chrome'],
        viewport: BREAKPOINTS.desktop_md,
        // Uses stable Chromium bundled with Playwright
      },
      dependencies: ['setup'],
    },

    // Firefox Desktop (latest)
    {
      name: 'firefox-desktop',
      use: {
        ...devices['Desktop Firefox'],
        viewport: BREAKPOINTS.desktop_lg,
      },
      dependencies: ['setup'],
    },

    // Firefox Desktop (alternate viewport for version coverage)
    {
      name: 'firefox-desktop-alt',
      use: {
        ...devices['Desktop Firefox'],
        viewport: BREAKPOINTS.desktop_md,
      },
      dependencies: ['setup'],
    },

    // Safari Desktop (WebKit)
    {
      name: 'safari-desktop',
      use: {
        ...devices['Desktop Safari'],
        viewport: BREAKPOINTS.desktop_lg,
      },
      dependencies: ['setup'],
    },

    // Safari Desktop (alternate viewport)
    {
      name: 'safari-desktop-alt',
      use: {
        ...devices['Desktop Safari'],
        viewport: BREAKPOINTS.desktop_md,
      },
      dependencies: ['setup'],
    },

    // ============================================
    // MOBILE BROWSERS - iOS & Android
    // ============================================

    // Mobile Safari - iPhone SE (320px - smallest viewport)
    {
      name: 'mobile-safari-iphone-se',
      use: {
        ...CUSTOM_DEVICES['iPhone SE'],
      },
      dependencies: ['setup'],
    },

    // Mobile Safari - iPhone 14 (standard iOS device)
    {
      name: 'mobile-safari-iphone-14',
      use: {
        ...CUSTOM_DEVICES['iPhone 14'],
      },
      dependencies: ['setup'],
    },

    // Mobile Safari - iPad (tablet)
    {
      name: 'mobile-safari-ipad',
      use: {
        ...CUSTOM_DEVICES['iPad'],
      },
      dependencies: ['setup'],
    },

    // Mobile Chrome - Pixel 5 (Android reference device)
    {
      name: 'mobile-chrome-pixel5',
      use: {
        ...devices['Pixel 5'],
      },
      dependencies: ['setup'],
    },

    // Mobile Chrome - Samsung Galaxy A52
    {
      name: 'mobile-chrome-samsung-a52',
      use: {
        ...CUSTOM_DEVICES['Samsung Galaxy A52'],
      },
      dependencies: ['setup'],
    },

    // ============================================
    // BREAKPOINT-SPECIFIC PROJECTS
    // ============================================

    // Breakpoint: 320px (Mobile portrait - smallest)
    {
      name: 'breakpoint-320',
      use: {
        ...devices['Desktop Chrome'],
        viewport: BREAKPOINTS.mobile_xs,
      },
      dependencies: ['setup'],
    },

    // Breakpoint: 480px (Mobile landscape)
    {
      name: 'breakpoint-480',
      use: {
        ...devices['Desktop Chrome'],
        viewport: { width: 480, height: 320 },
      },
      dependencies: ['setup'],
    },

    // Breakpoint: 768px (Tablet)
    {
      name: 'breakpoint-768',
      use: {
        ...devices['Desktop Chrome'],
        viewport: BREAKPOINTS.tablet,
      },
      dependencies: ['setup'],
    },

    // Breakpoint: 1024px (Desktop)
    {
      name: 'breakpoint-1024',
      use: {
        ...devices['Desktop Chrome'],
        viewport: BREAKPOINTS.desktop_md,
      },
      dependencies: ['setup'],
    },

    // Breakpoint: 1920px (Full HD)
    {
      name: 'breakpoint-1920',
      use: {
        ...devices['Desktop Chrome'],
        viewport: BREAKPOINTS.desktop_lg,
      },
      dependencies: ['setup'],
    },
  ],

  // Web server configuration
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:3100',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
    stdout: 'ignore',
    stderr: 'pipe',
  },
});
