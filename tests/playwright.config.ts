import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for TabScore2 E2E tests
 * 
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  testDir: './e2e',
  
  /* Run tests in files in parallel */
  fullyParallel: false, // Serial execution for stateful tests
  
  /* Fail the build on CI if you accidentally left test.only in the source code */
  forbidOnly: !!process.env.CI,
  
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  
  /* Opt out of parallel tests on CI */
  workers: 1, // Single worker for serial test execution
  
  /* Reporter to use */
  reporter: [
    ['html', { open: 'never' }],
    ['list']
  ],
  
  /* Shared settings for all the projects below */
  use: {
    /* Base URL for the TabScore2 web application */
    baseURL: process.env.TABSCORE2_URL || 'http://localhost:5213',

    /* Collect trace when retrying the failed test */
    trace: 'on-first-retry',
    
    /* Screenshot on failure */
    screenshot: 'only-on-failure',
    
    /* Video recording */
    video: 'retain-on-failure',
    
    /* Default timeout for actions */
    actionTimeout: 10000,
    
    /* Default navigation timeout */
    navigationTimeout: 30000,
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },

    /* Test against mobile viewports (tablet simulation) */
    {
      name: 'tablet',
      use: { 
        ...devices['iPad Pro'],
        // TabScore2 is designed for tablets
      },
    },

    /* Uncomment for additional browser testing
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },

    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    */
  ],

  /* Global timeout for each test */
  timeout: 60000,
  
  /* Expect timeout */
  expect: {
    timeout: 5000,
  },

  /* Run your local dev server before starting the tests */
  // Note: TabScore2 must be started manually as it's a Windows Forms app
  // webServer: {
  //   command: 'TabScore2.exe',
  //   url: 'http://localhost:5213',
  //   reuseExistingServer: !process.env.CI,
  // },
});
