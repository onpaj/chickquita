import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { flocksApi, type Flock } from '../api/flocksApi';
import { useToast } from '../../../hooks/useToast';

/**
 * Hook for fetching flocks for a specific coop.
 * Supports filtering by active/inactive status.
 *
 * @param coopId - The ID of the coop to fetch flocks for
 * @param includeInactive - Whether to include inactive flocks (default: false)
 */
export function useFlocks(coopId: string, includeInactive: boolean = false) {
  return useQuery({
    queryKey: ['flocks', coopId, includeInactive],
    queryFn: () => flocksApi.getAll(coopId, includeInactive),
    enabled: !!coopId,
  });
}

/**
 * Hook for fetching a single flock by ID.
 *
 * @param coopId - The ID of the coop the flock belongs to
 * @param flockId - The ID of the flock to fetch
 */
export function useFlockDetail(coopId: string, flockId: string) {
  return useQuery({
    queryKey: ['flocks', coopId, flockId],
    queryFn: () => flocksApi.getById(coopId, flockId),
    enabled: !!coopId && !!flockId,
  });
}

/**
 * Hook for creating a new flock.
 * Includes optimistic updates and toast notifications.
 */
export function useCreateFlock() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  return useMutation({
    mutationFn: flocksApi.create,
    onMutate: async (newFlock) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({
        queryKey: ['flocks', newFlock.coopId]
      });

      // Snapshot the previous value
      const previousFlocks = queryClient.getQueryData<Flock[]>([
        'flocks',
        newFlock.coopId,
        false,
      ]);

      // Optimistically update to the new value
      queryClient.setQueryData<Flock[]>(
        ['flocks', newFlock.coopId, false],
        (old = []) => {
          const optimisticFlock: Flock = {
            id: `temp-${Date.now()}`,
            tenantId: 'pending',
            coopId: newFlock.coopId,
            identifier: newFlock.identifier,
            hatchDate: newFlock.hatchDate,
            currentHens: newFlock.initialHens,
            currentRoosters: newFlock.initialRoosters,
            currentChicks: newFlock.initialChicks,
            isActive: true,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
            history: [],
          };
          return [optimisticFlock, ...old];
        }
      );

      return { previousFlocks, coopId: newFlock.coopId };
    },
    onError: (_err, _newFlock, context) => {
      // Roll back on error
      if (context?.previousFlocks) {
        queryClient.setQueryData(
          ['flocks', context.coopId, false],
          context.previousFlocks
        );
      }
      showError(
        t('flocks.create.error'),
        'flocks.create.error'
      );
    },
    onSuccess: () => {
      showSuccess(
        t('flocks.create.success'),
        'flocks.create.success'
      );
    },
    onSettled: (_data, _error, variables) => {
      // Refetch to ensure consistency
      queryClient.invalidateQueries({
        queryKey: ['flocks', variables.coopId]
      });
    },
  });
}

/**
 * Hook for updating an existing flock.
 * Invalidates queries and shows toast notifications on success/error.
 */
export function useUpdateFlock() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  return useMutation({
    mutationFn: ({
      coopId,
      data,
    }: {
      coopId: string;
      data: Parameters<typeof flocksApi.update>[1];
    }) => flocksApi.update(coopId, data),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['flocks', variables.coopId]
      });
      queryClient.invalidateQueries({
        queryKey: ['flocks', variables.coopId, variables.data.id]
      });
      showSuccess(
        t('flocks.update.success'),
        'flocks.update.success'
      );
    },
    onError: () => {
      showError(
        t('flocks.update.error'),
        'flocks.update.error'
      );
    },
  });
}

/**
 * Hook for archiving a flock (sets isActive to false).
 * Invalidates queries and shows toast notifications on success/error.
 */
export function useArchiveFlock() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  return useMutation({
    mutationFn: ({ coopId, flockId }: { coopId: string; flockId: string }) =>
      flocksApi.archive(coopId, flockId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['flocks', variables.coopId]
      });
      queryClient.invalidateQueries({
        queryKey: ['flocks', variables.coopId, variables.flockId]
      });
      showSuccess(
        t('flocks.archive.success'),
        'flocks.archive.success'
      );
    },
    onError: () => {
      showError(
        t('flocks.archive.error'),
        'flocks.archive.error'
      );
    },
  });
}
