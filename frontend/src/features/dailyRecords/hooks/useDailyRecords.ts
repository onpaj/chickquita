import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import {
  dailyRecordsApi,
  type DailyRecordDto,
  type GetDailyRecordsParams,
  type UpdateDailyRecordRequest,
} from '../api/dailyRecordsApi';
import { useToast } from '../../../hooks/useToast';

/**
 * Hook for fetching daily records with optional filtering.
 * Caches results for 5 minutes.
 *
 * @param params - Optional filtering parameters (flockId, startDate, endDate)
 */
export function useDailyRecords(params?: GetDailyRecordsParams) {
  return useQuery({
    queryKey: ['dailyRecords', params],
    queryFn: () => dailyRecordsApi.getAll(params),
    staleTime: 5 * 60 * 1000, // 5 minutes cache
  });
}

/**
 * Hook for fetching daily records for a specific flock.
 * Caches results for 5 minutes.
 *
 * @param flockId - The ID of the flock to fetch records for
 * @param params - Optional date range filtering (startDate, endDate)
 */
export function useDailyRecordsByFlock(
  flockId: string,
  params?: Omit<GetDailyRecordsParams, 'flockId'>
) {
  return useQuery({
    queryKey: ['dailyRecords', 'flock', flockId, params],
    queryFn: () => dailyRecordsApi.getByFlock(flockId, params),
    enabled: !!flockId,
    staleTime: 5 * 60 * 1000, // 5 minutes cache
  });
}

/**
 * Hook for creating a new daily record.
 * Includes optimistic updates and toast notifications.
 */
export function useCreateDailyRecord() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  return useMutation({
    mutationFn: ({
      flockId,
      data,
    }: {
      flockId: string;
      data: { recordDate: string; eggCount: number; notes?: string };
    }) => dailyRecordsApi.create(flockId, data),
    onMutate: async ({ flockId, data }) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({
        queryKey: ['dailyRecords'],
      });

      // Snapshot the previous values
      const previousDailyRecords = queryClient.getQueryData<DailyRecordDto[]>([
        'dailyRecords',
        'flock',
        flockId,
      ]);

      // Optimistically update to the new value
      queryClient.setQueryData<DailyRecordDto[]>(
        ['dailyRecords', 'flock', flockId],
        (old = []) => {
          const optimisticRecord: DailyRecordDto = {
            id: `temp-${Date.now()}`,
            tenantId: 'pending',
            flockId,
            flockName: '',
            flockCoopName: '',
            recordDate: data.recordDate,
            eggCount: data.eggCount,
            notes: data.notes,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
          };
          return [optimisticRecord, ...old];
        }
      );

      return { previousDailyRecords, flockId };
    },
    onError: (_err, _variables, context) => {
      // Roll back on error
      if (context?.previousDailyRecords) {
        queryClient.setQueryData(
          ['dailyRecords', 'flock', context.flockId],
          context.previousDailyRecords
        );
      }
      showError(
        t('dailyRecords.create.error'),
        'dailyRecords.create.error'
      );
    },
    onSuccess: () => {
      showSuccess(
        t('dailyRecords.create.success'),
        'dailyRecords.create.success'
      );
    },
    onSettled: (_data, _error, variables) => {
      // Refetch to ensure consistency
      queryClient.invalidateQueries({
        queryKey: ['dailyRecords'],
      });
      queryClient.invalidateQueries({
        queryKey: ['dailyRecords', 'flock', variables.flockId],
      });
    },
  });
}

/**
 * Hook for updating an existing daily record.
 * Only allows updates on the same day the record was created.
 * Invalidates queries and shows toast notifications on success/error.
 */
export function useUpdateDailyRecord() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  return useMutation({
    mutationFn: (data: UpdateDailyRecordRequest) => dailyRecordsApi.update(data),
    onSuccess: (updatedRecord) => {
      queryClient.invalidateQueries({
        queryKey: ['dailyRecords'],
      });
      queryClient.invalidateQueries({
        queryKey: ['dailyRecords', 'flock', updatedRecord.flockId],
      });
      showSuccess(
        t('dailyRecords.update.success'),
        'dailyRecords.update.success'
      );
    },
    onError: () => {
      showError(
        t('dailyRecords.update.error'),
        'dailyRecords.update.error'
      );
    },
  });
}

/**
 * Hook for deleting a daily record.
 * Invalidates queries and shows toast notifications on success/error.
 */
export function useDeleteDailyRecord() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  return useMutation({
    mutationFn: ({ id }: { id: string; flockId: string }) =>
      dailyRecordsApi.delete(id),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['dailyRecords'],
      });
      queryClient.invalidateQueries({
        queryKey: ['dailyRecords', 'flock', variables.flockId],
      });
      showSuccess(
        t('dailyRecords.delete.success'),
        'dailyRecords.delete.success'
      );
    },
    onError: () => {
      showError(
        t('dailyRecords.delete.error'),
        'dailyRecords.delete.error'
      );
    },
  });
}
