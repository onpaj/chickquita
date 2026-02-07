/**
 * Test fixtures for Coop E2E tests
 * Provides reusable test data and helper functions
 */

export interface CoopTestData {
  name: string;
  location?: string;
}

/**
 * Generate a unique coop name with timestamp and random suffix
 */
export function generateCoopName(prefix: string = 'Test Coop'): string {
  const random = Math.floor(Math.random() * 1000000);
  return `${prefix} ${Date.now()}${random}`;
}

/**
 * Create test coop data
 */
export function createCoopTestData(overrides?: Partial<CoopTestData>): CoopTestData {
  return {
    name: generateCoopName(),
    location: 'Test Location',
    ...overrides,
  };
}

/**
 * Common test coops for various scenarios
 */
export const testCoops = {
  basic: (): CoopTestData => ({
    name: generateCoopName('Basic Coop'),
  }),

  withLocation: (): CoopTestData => ({
    name: generateCoopName('Coop with Location'),
    location: 'Behind the house',
  }),

  longName: (): CoopTestData => ({
    name: `Very Long Coop Name ${Date.now()} with lots of text to test the name length limits and UI rendering`,
    location: 'Test',
  }),

  specialCharacters: (): CoopTestData => ({
    name: `Coop-${Date.now()}_#@!`,
    location: 'Location & Place / Test',
  }),

  multilingual: (): CoopTestData => ({
    name: `Kurník ${Date.now()}`,
    location: 'Za domem',
  }),
};

/**
 * Validation test data
 */
export const invalidCoops = {
  emptyName: (): CoopTestData => ({
    name: '',
    location: 'Some location',
  }),

  tooLongName: (): CoopTestData => ({
    name: 'A'.repeat(101), // Exceeds 100 character limit
    location: 'Test',
  }),

  tooLongLocation: (): CoopTestData => ({
    name: generateCoopName(),
    location: 'L'.repeat(201), // Exceeds 200 character limit
  }),
};
