import { apiClient } from '../../../lib/apiClient';

export interface TenantSettings {
  singleCoopMode: boolean;
  currency: string;
}

export const settingsApi = {
  /**
   * Retrieves tenant settings.
   * Backend endpoint: GET /api/settings
   *
   * @returns Promise resolving to TenantSettings
   * @throws Error if the API request fails
   */
  getSettings: async (): Promise<TenantSettings> => {
    const response = await apiClient.get<TenantSettings>('/settings');
    return response.data;
  },

  /**
   * Updates tenant settings.
   * Backend endpoint: PUT /api/settings
   *
   * @param settings - The settings to update
   * @returns Promise resolving to void (204 No Content)
   * @throws Error if the API request fails
   */
  updateSettings: async (settings: TenantSettings): Promise<void> => {
    await apiClient.put('/settings', settings);
  },
};
