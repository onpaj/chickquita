/**
 * Test fixtures for flock-related E2E tests
 */

export interface FlockTestData {
  identifier: string;
  hatchDate: string;
  hens: number;
  roosters: number;
  chicks: number;
}

/**
 * Generate a unique flock identifier with timestamp
 */
export function generateFlockIdentifier(prefix: string = 'Flock'): string {
  return `${prefix} ${Date.now()}`;
}

/**
 * Create flock test data with optional overrides
 */
export function createFlockTestData(overrides?: Partial<FlockTestData>): FlockTestData {
  const today = new Date();
  const defaultHatchDate = new Date(today.setDate(today.getDate() - 30))
    .toISOString()
    .split('T')[0]; // 30 days ago

  return {
    identifier: generateFlockIdentifier(),
    hatchDate: defaultHatchDate,
    hens: 10,
    roosters: 2,
    chicks: 5,
    ...overrides,
  };
}

/**
 * Predefined test flock data for common scenarios
 */
export const testFlocks = {
  /**
   * Basic flock with mixed composition
   */
  basic: (): FlockTestData =>
    createFlockTestData({
      identifier: generateFlockIdentifier('Test Flock'),
    }),

  /**
   * Flock with only hens
   */
  hensOnly: (): FlockTestData =>
    createFlockTestData({
      identifier: generateFlockIdentifier('Hens Only'),
      hens: 20,
      roosters: 0,
      chicks: 0,
    }),

  /**
   * Flock with high numbers
   */
  large: (): FlockTestData =>
    createFlockTestData({
      identifier: generateFlockIdentifier('Large Flock'),
      hens: 100,
      roosters: 10,
      chicks: 50,
    }),

  /**
   * Recently hatched flock (7 days ago)
   */
  recentHatch: (): FlockTestData => {
    const sevenDaysAgo = new Date();
    sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
    return createFlockTestData({
      identifier: generateFlockIdentifier('Recent'),
      hatchDate: sevenDaysAgo.toISOString().split('T')[0],
      hens: 0,
      roosters: 0,
      chicks: 30,
    });
  },

  /**
   * Flock with long identifier (testing UI limits)
   */
  longIdentifier: (): FlockTestData =>
    createFlockTestData({
      identifier: `Very Long Flock Name ${Date.now()} That Approaches Limit`,
    }),

  /**
   * Flock with special characters in identifier
   */
  specialCharacters: (): FlockTestData =>
    createFlockTestData({
      identifier: `Flock-${Date.now()}_#1!`,
    }),

  /**
   * Flock with Czech language identifier
   */
  multilingual: (): FlockTestData =>
    createFlockTestData({
      identifier: `Hejno č.${Date.now()}`,
    }),
};

/**
 * Invalid flock data for validation testing
 */
export const invalidFlocks = {
  /**
   * Empty identifier
   */
  emptyIdentifier: (): Partial<FlockTestData> =>
    createFlockTestData({
      identifier: '',
    }),

  /**
   * Identifier exceeds max length (50 characters)
   */
  tooLongIdentifier: (): Partial<FlockTestData> =>
    createFlockTestData({
      identifier: 'A'.repeat(51) + Date.now(),
    }),

  /**
   * Future hatch date (invalid)
   */
  futureHatchDate: (): Partial<FlockTestData> => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return createFlockTestData({
      hatchDate: tomorrow.toISOString().split('T')[0],
    });
  },

  /**
   * Empty hatch date
   */
  emptyHatchDate: (): Partial<FlockTestData> =>
    createFlockTestData({
      hatchDate: '',
    }),

  /**
   * All counts are zero (must have at least one animal)
   */
  zeroCounts: (): Partial<FlockTestData> =>
    createFlockTestData({
      hens: 0,
      roosters: 0,
      chicks: 0,
    }),

  /**
   * Negative counts (invalid)
   */
  negativeCounts: (): Partial<FlockTestData> =>
    createFlockTestData({
      hens: -5,
      roosters: -2,
      chicks: -10,
    }),
};

/**
 * Helper to get today's date in YYYY-MM-DD format
 */
export function getTodayDate(): string {
  return new Date().toISOString().split('T')[0];
}

/**
 * Helper to get a date N days ago in YYYY-MM-DD format
 */
export function getDaysAgoDate(days: number): string {
  const date = new Date();
  date.setDate(date.getDate() - days);
  return date.toISOString().split('T')[0];
}
