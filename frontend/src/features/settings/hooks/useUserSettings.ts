import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '@clerk/clerk-react';
import { settingsApi } from '../api/settingsApi';
import type { TenantSettings } from '../api/settingsApi';

export const SETTINGS_QUERY_KEY = ['settings'];

/**
 * Hook for fetching tenant settings.
 * Only runs when the user is authenticated.
 *
 * @returns Query result with settings data, loading state, and error
 */
export function useUserSettings() {
  const { isSignedIn } = useAuth();

  return useQuery({
    queryKey: SETTINGS_QUERY_KEY,
    queryFn: settingsApi.getSettings,
    enabled: !!isSignedIn,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Hook for updating tenant settings.
 * Invalidates settings query on success.
 *
 * @returns Mutation result with updateSettings function and isPending state
 */
export function useUpdateUserSettings() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: (settings: TenantSettings) => settingsApi.updateSettings(settings),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: SETTINGS_QUERY_KEY });
    },
  });

  return {
    updateSettings: mutation.mutate,
    isPending: mutation.isPending,
  };
}
