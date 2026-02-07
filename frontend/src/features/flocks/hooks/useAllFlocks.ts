import { useQuery } from '@tanstack/react-query';
import { coopsApi } from '@/features/coops/api/coopsApi';
import { flocksApi } from '../api/flocksApi';

/**
 * Simplified flock data for QuickAddModal dropdown.
 * Combines flock identifier with coop name for clear context.
 */
export interface FlockForQuickAdd {
  id: string;
  identifier: string;
  coopName: string;
}

/**
 * Hook for fetching all active flocks across all coops.
 * Used by QuickAddModal for the flock selection dropdown.
 *
 * This hook:
 * 1. Fetches all active coops
 * 2. Fetches active flocks for each coop
 * 3. Transforms data into a flat list with coop context
 *
 * @returns Query result with flattened list of flocks with coop names
 */
export function useAllFlocks() {
  return useQuery({
    queryKey: ['flocks', 'all'],
    queryFn: async (): Promise<FlockForQuickAdd[]> => {
      // Fetch all coops
      const coops = await coopsApi.getAll();
      const activeCoops = coops.filter((coop) => coop.isActive);

      // If no coops, return empty array
      if (activeCoops.length === 0) {
        return [];
      }

      // Fetch flocks for all coops in parallel
      const flocksPromises = activeCoops.map((coop) =>
        flocksApi
          .getAll(coop.id, false) // Only active flocks
          .then((flocks) => ({ coop, flocks }))
      );

      const coopsWithFlocks = await Promise.all(flocksPromises);

      // Flatten and transform to QuickAdd format
      const allFlocks: FlockForQuickAdd[] = [];
      coopsWithFlocks.forEach(({ coop, flocks }) => {
        flocks.forEach((flock) => {
          allFlocks.push({
            id: flock.id,
            identifier: flock.identifier,
            coopName: coop.name,
          });
        });
      });

      return allFlocks;
    },
    staleTime: 1000 * 60, // 1 minute
    gcTime: 1000 * 60 * 5, // 5 minutes
  });
}
