import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { eggSalesApi } from '../api/eggSalesApi';
import { useToast } from '../../../hooks/useToast';
import type {
  EggSaleDto,
  CreateEggSaleDto,
  UpdateEggSaleDto,
  EggSaleFilterParams,
} from '../types/eggSale.types';

/**
 * Hook for fetching egg sales with optional date range filtering.
 *
 * @param filters - Optional filter parameters (date range)
 * @returns Query result with eggSales, loading state, error, and refetch function
 */
export function useEggSales(filters?: EggSaleFilterParams) {
  const query = useQuery({
    queryKey: ['eggSales', filters],
    queryFn: () => eggSalesApi.getEggSales(filters),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  return {
    eggSales: Array.isArray(query.data) ? query.data : [],
    isLoading: query.isLoading,
    error: query.error,
    refetch: query.refetch,
  };
}

/**
 * Hook for fetching a single egg sale by ID.
 *
 * @param id - The ID of the egg sale to fetch
 * @returns Query result with egg sale details
 */
export function useEggSaleDetail(id: string) {
  return useQuery({
    queryKey: ['eggSales', id],
    queryFn: () => eggSalesApi.getEggSaleById(id),
    enabled: !!id,
  });
}

/**
 * Hook for creating a new egg sale.
 * Includes optimistic updates and toast notifications.
 *
 * @returns Mutation result with createEggSale function and isCreating state
 */
export function useCreateEggSale() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  const mutation = useMutation({
    mutationFn: eggSalesApi.createEggSale,
    onMutate: async (newEggSale: CreateEggSaleDto) => {
      await queryClient.cancelQueries({ queryKey: ['eggSales'] });

      const previousEggSales = queryClient.getQueryData<EggSaleDto[]>([
        'eggSales',
        undefined,
      ]);

      queryClient.setQueryData<EggSaleDto[]>(
        ['eggSales', undefined],
        (old = []) => {
          const optimisticEggSale: EggSaleDto = {
            id: `temp-${Date.now()}`,
            tenantId: 'pending',
            date: newEggSale.date,
            quantity: newEggSale.quantity,
            pricePerUnit: newEggSale.pricePerUnit,
            buyerName: newEggSale.buyerName || null,
            notes: newEggSale.notes || null,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
          };
          return [optimisticEggSale, ...old];
        }
      );

      return { previousEggSales };
    },
    onError: (_err, _newEggSale, context) => {
      if (context?.previousEggSales !== undefined) {
        queryClient.setQueryData(['eggSales', undefined], context.previousEggSales);
      }
      showError(t('eggSales.create.error'), 'eggSales.create.error');
    },
    onSuccess: () => {
      showSuccess(t('eggSales.create.success'), 'eggSales.create.success');
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['eggSales'] });
    },
  });

  return {
    createEggSale: mutation.mutate,
    isCreating: mutation.isPending,
  };
}

/**
 * Hook for updating an existing egg sale.
 * Invalidates queries and shows toast notifications on success/error.
 *
 * @returns Mutation result with updateEggSale function and isUpdating state
 */
export function useUpdateEggSale() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  const mutation = useMutation({
    mutationFn: eggSalesApi.updateEggSale,
    onMutate: async (updatedEggSale: UpdateEggSaleDto) => {
      await queryClient.cancelQueries({ queryKey: ['eggSales'] });

      const previousEggSales = queryClient.getQueryData<EggSaleDto[]>([
        'eggSales',
        undefined,
      ]);

      queryClient.setQueryData<EggSaleDto[]>(
        ['eggSales', undefined],
        (old = []) =>
          old.map((sale) =>
            sale.id === updatedEggSale.id
              ? { ...sale, ...updatedEggSale, updatedAt: new Date().toISOString() }
              : sale
          )
      );

      return { previousEggSales };
    },
    onError: (_err, _updatedEggSale, context) => {
      if (context?.previousEggSales !== undefined) {
        queryClient.setQueryData(['eggSales', undefined], context.previousEggSales);
      }
      showError(t('eggSales.update.error'), 'eggSales.update.error');
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ['eggSales'] });
      queryClient.invalidateQueries({ queryKey: ['eggSales', variables.id] });
      showSuccess(t('eggSales.update.success'), 'eggSales.update.success');
    },
  });

  return {
    updateEggSale: mutation.mutate,
    isUpdating: mutation.isPending,
  };
}

/**
 * Hook for deleting an egg sale.
 * Optimistically removes the record and shows toast notifications.
 *
 * @returns Mutation result with deleteEggSale function and isDeleting state
 */
export function useDeleteEggSale() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  const mutation = useMutation({
    mutationFn: eggSalesApi.deleteEggSale,
    onMutate: async (id: string) => {
      await queryClient.cancelQueries({ queryKey: ['eggSales'] });

      const previousEggSales = queryClient.getQueryData<EggSaleDto[]>([
        'eggSales',
        undefined,
      ]);

      queryClient.setQueryData<EggSaleDto[]>(
        ['eggSales', undefined],
        (old = []) => old.filter((sale) => sale.id !== id)
      );

      return { previousEggSales };
    },
    onError: (_err, _id, context) => {
      if (context?.previousEggSales !== undefined) {
        queryClient.setQueryData(['eggSales', undefined], context.previousEggSales);
      }
      showError(t('eggSales.delete.error'), 'eggSales.delete.error');
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['eggSales'] });
      showSuccess(t('eggSales.delete.success'), 'eggSales.delete.success');
    },
  });

  return {
    deleteEggSale: mutation.mutate,
    isDeleting: mutation.isPending,
  };
}

/**
 * Hook that returns the price per unit from the most recent egg sale.
 * Used to pre-fill the price field in the "quick add sale" form.
 *
 * Returns undefined while loading or if no sales exist yet.
 */
export function useLastUsedEggPrice(): number | undefined {
  const { eggSales, isLoading } = useEggSales();

  if (isLoading || eggSales.length === 0) {
    return undefined;
  }

  // Sales are returned newest-first from the API; take the first one
  const sorted = [...eggSales].sort(
    (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()
  );

  return sorted[0].pricePerUnit;
}
