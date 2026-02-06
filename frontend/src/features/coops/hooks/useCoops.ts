import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coopsApi, type Coop } from '../api/coopsApi';

export function useCoops() {
  return useQuery({
    queryKey: ['coops'],
    queryFn: coopsApi.getAll,
  });
}

export function useCoopDetail(id: string) {
  return useQuery({
    queryKey: ['coops', id],
    queryFn: () => coopsApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateCoop() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: coopsApi.create,
    onMutate: async (newCoop) => {
      // Cancel any outgoing refetches to prevent them from overwriting our optimistic update
      await queryClient.cancelQueries({ queryKey: ['coops'] });

      // Snapshot the previous value
      const previousCoops = queryClient.getQueryData<Coop[]>(['coops']);

      // Optimistically update to the new value
      queryClient.setQueryData<Coop[]>(['coops'], (old = []) => {
        // Create an optimistic coop object with a temporary ID
        const optimisticCoop: Coop = {
          id: `temp-${Date.now()}`,
          tenantId: 'pending',
          name: newCoop.name,
          location: newCoop.location,
          isActive: true,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          flocksCount: 0,
        };
        return [optimisticCoop, ...old];
      });

      // Return a context object with the snapshotted value
      return { previousCoops };
    },
    onError: (_err, _newCoop, context) => {
      // If the mutation fails, use the context returned from onMutate to roll back
      if (context?.previousCoops) {
        queryClient.setQueryData(['coops'], context.previousCoops);
      }
    },
    onSettled: () => {
      // Always refetch after error or success to ensure consistency
      queryClient.invalidateQueries({ queryKey: ['coops'] });
    },
  });
}

export function useUpdateCoop() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: coopsApi.update,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['coops'] });
      queryClient.invalidateQueries({ queryKey: ['coops', data.id] });
    },
  });
}

export function useArchiveCoop() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: coopsApi.archive,
    onSuccess: (_data, coopId) => {
      queryClient.invalidateQueries({ queryKey: ['coops'] });
      queryClient.invalidateQueries({ queryKey: ['coops', coopId] });
    },
  });
}

export function useDeleteCoop() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: coopsApi.delete,
    onSuccess: (_data, coopId) => {
      queryClient.invalidateQueries({ queryKey: ['coops'] });
      queryClient.invalidateQueries({ queryKey: ['coops', coopId] });
    },
  });
}
