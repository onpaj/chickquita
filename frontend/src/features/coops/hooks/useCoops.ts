import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coopsApi } from '../api/coopsApi';

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
    onSuccess: async () => {
      // Invalidate and immediately refetch to ensure UI updates with correct backend data
      // refetchType: 'active' ensures immediate refetch even with refetchOnMount:false config
      await queryClient.invalidateQueries({ queryKey: ['coops'], refetchType: 'active' });
    },
  });
}

export function useUpdateCoop() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: coopsApi.update,
    onSuccess: async (data) => {
      // Invalidate and immediately refetch active queries to ensure UI updates
      // refetchType: 'active' ensures immediate refetch even with refetchOnMount:false config
      await queryClient.invalidateQueries({ queryKey: ['coops'], refetchType: 'active' });
      await queryClient.invalidateQueries({ queryKey: ['coops', data.id], refetchType: 'active' });
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
