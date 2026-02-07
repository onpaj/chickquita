import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { purchasesApi } from '../api/purchasesApi';
import { useToast } from '../../../hooks/useToast';
import type {
  PurchaseDto,
  CreatePurchaseDto,
  UpdatePurchaseDto,
  PurchaseFilterParams,
} from '../types/purchase.types';

/**
 * Hook for fetching purchases with optional filtering.
 * Supports filtering by date range, type, and flockId.
 *
 * @param filters - Optional filter parameters (date range, type, flockId)
 * @returns Query result with purchases, loading state, error, and refetch function
 */
export function usePurchases(filters?: PurchaseFilterParams) {
  const query = useQuery({
    queryKey: ['purchases', filters],
    queryFn: () => purchasesApi.getPurchases(filters),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  return {
    purchases: query.data,
    isLoading: query.isLoading,
    error: query.error,
    refetch: query.refetch,
  };
}

/**
 * Hook for fetching a single purchase by ID.
 *
 * @param id - The ID of the purchase to fetch
 * @returns Query result with purchase details
 */
export function usePurchaseDetail(id: string) {
  return useQuery({
    queryKey: ['purchases', id],
    queryFn: () => purchasesApi.getPurchaseById(id),
    enabled: !!id,
  });
}

/**
 * Hook for creating a new purchase.
 * Includes optimistic updates and toast notifications.
 *
 * @returns Mutation result with createPurchase function and isCreating state
 */
export function useCreatePurchase() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  const mutation = useMutation({
    mutationFn: purchasesApi.createPurchase,
    onMutate: async (newPurchase: CreatePurchaseDto) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({
        queryKey: ['purchases'],
      });

      // Snapshot the previous value
      const previousPurchases = queryClient.getQueryData<PurchaseDto[]>([
        'purchases',
        undefined,
      ]);

      // Optimistically update to the new value
      queryClient.setQueryData<PurchaseDto[]>(
        ['purchases', undefined],
        (old = []) => {
          const optimisticPurchase: PurchaseDto = {
            id: `temp-${Date.now()}`,
            tenantId: 'pending',
            coopId: newPurchase.coopId || null,
            name: newPurchase.name,
            type: newPurchase.type,
            amount: newPurchase.amount,
            quantity: newPurchase.quantity,
            unit: newPurchase.unit,
            purchaseDate: newPurchase.purchaseDate,
            consumedDate: newPurchase.consumedDate || null,
            notes: newPurchase.notes || null,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
          };
          return [optimisticPurchase, ...old];
        }
      );

      return { previousPurchases };
    },
    onError: (_err, _newPurchase, context) => {
      // Roll back on error
      if (context?.previousPurchases !== undefined) {
        queryClient.setQueryData(
          ['purchases', undefined],
          context.previousPurchases
        );
      }
      showError(
        t('purchases.create.error'),
        'purchases.create.error'
      );
    },
    onSuccess: () => {
      showSuccess(
        t('purchases.create.success'),
        'purchases.create.success'
      );
    },
    onSettled: () => {
      // Refetch to ensure consistency
      queryClient.invalidateQueries({
        queryKey: ['purchases'],
      });
    },
  });

  return {
    createPurchase: mutation.mutate,
    isCreating: mutation.isPending,
  };
}

/**
 * Hook for updating an existing purchase.
 * Invalidates queries and shows toast notifications on success/error.
 *
 * @returns Mutation result with updatePurchase function and isUpdating state
 */
export function useUpdatePurchase() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  const mutation = useMutation({
    mutationFn: purchasesApi.updatePurchase,
    onMutate: async (updatedPurchase: UpdatePurchaseDto) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({
        queryKey: ['purchases'],
      });

      // Snapshot the previous value
      const previousPurchases = queryClient.getQueryData<PurchaseDto[]>([
        'purchases',
        undefined,
      ]);

      // Optimistically update the purchase
      queryClient.setQueryData<PurchaseDto[]>(
        ['purchases', undefined],
        (old = []) => {
          return old.map((purchase) =>
            purchase.id === updatedPurchase.id
              ? {
                  ...purchase,
                  ...updatedPurchase,
                  updatedAt: new Date().toISOString(),
                }
              : purchase
          );
        }
      );

      return { previousPurchases };
    },
    onError: (_err, _updatedPurchase, context) => {
      // Roll back on error
      if (context?.previousPurchases !== undefined) {
        queryClient.setQueryData(
          ['purchases', undefined],
          context.previousPurchases
        );
      }
      showError(
        t('purchases.update.error'),
        'purchases.update.error'
      );
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['purchases'],
      });
      queryClient.invalidateQueries({
        queryKey: ['purchases', variables.id],
      });
      showSuccess(
        t('purchases.update.success'),
        'purchases.update.success'
      );
    },
  });

  return {
    updatePurchase: mutation.mutate,
    isUpdating: mutation.isPending,
  };
}

/**
 * Hook for deleting a purchase.
 * Invalidates queries and shows toast notifications on success/error.
 *
 * @returns Mutation result with deletePurchase function and isDeleting state
 */
export function useDeletePurchase() {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
  const { showSuccess, showError } = useToast();

  const mutation = useMutation({
    mutationFn: purchasesApi.deletePurchase,
    onMutate: async (id: string) => {
      // Cancel any outgoing refetches
      await queryClient.cancelQueries({
        queryKey: ['purchases'],
      });

      // Snapshot the previous value
      const previousPurchases = queryClient.getQueryData<PurchaseDto[]>([
        'purchases',
        undefined,
      ]);

      // Optimistically remove the purchase
      queryClient.setQueryData<PurchaseDto[]>(
        ['purchases', undefined],
        (old = []) => old.filter((purchase) => purchase.id !== id)
      );

      return { previousPurchases };
    },
    onError: (_err, _id, context) => {
      // Roll back on error
      if (context?.previousPurchases !== undefined) {
        queryClient.setQueryData(
          ['purchases', undefined],
          context.previousPurchases
        );
      }
      showError(
        t('purchases.delete.error'),
        'purchases.delete.error'
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['purchases'],
      });
      showSuccess(
        t('purchases.delete.success'),
        'purchases.delete.success'
      );
    },
  });

  return {
    deletePurchase: mutation.mutate,
    isDeleting: mutation.isPending,
  };
}
