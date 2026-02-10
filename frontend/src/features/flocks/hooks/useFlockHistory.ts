import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { flocksApi, type FlockHistory } from '../api/flocksApi';

/**
 * React Query hook to fetch flock history timeline.
 * Automatically refetches when flockId changes.
 *
 * @param flockId - The ID of the flock to get history for
 * @returns Query result with FlockHistory[] data
 */
export function useFlockHistory(flockId: string) {
  return useQuery({
    queryKey: ['flocks', flockId, 'history'],
    queryFn: () => flocksApi.getHistory(flockId),
    enabled: !!flockId,
  });
}

/**
 * React Query mutation hook to update notes on a flock history entry.
 * Optimistically updates the cache and invalidates queries on success.
 *
 * @returns Mutation object with mutate/mutateAsync functions
 */
export function useUpdateFlockHistoryNotes() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ historyId, notes }: { historyId: string; notes: string | null }) =>
      flocksApi.updateHistoryNotes(historyId, notes),

    onMutate: async ({ historyId, notes }) => {
      // Cancel any outgoing refetches to prevent optimistic update from being overwritten
      await queryClient.cancelQueries({ queryKey: ['flocks'] });

      // Snapshot the previous value
      const previousHistory = queryClient.getQueriesData({ queryKey: ['flocks'] });

      // Optimistically update the cache
      queryClient.setQueriesData<FlockHistory[]>(
        { queryKey: ['flocks'], predicate: (query) => query.queryKey.includes('history') },
        (old) => {
          if (!old) return old;
          return old.map((entry) =>
            entry.id === historyId
              ? { ...entry, notes: notes ?? undefined, updatedAt: new Date().toISOString() }
              : entry
          );
        }
      );

      return { previousHistory };
    },

    onError: (_err, _variables, context) => {
      // Roll back on error
      if (context?.previousHistory) {
        context.previousHistory.forEach(([queryKey, data]) => {
          queryClient.setQueryData(queryKey, data);
        });
      }
    },

    onSettled: () => {
      // Refetch to ensure cache is in sync with server
      queryClient.invalidateQueries({
        queryKey: ['flocks'],
        predicate: (query) => query.queryKey.includes('history')
      });
    },
  });
}
