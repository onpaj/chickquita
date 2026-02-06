import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { coopsApi } from '../api/coopsApi';

export function useCoops() {
  return useQuery({
    queryKey: ['coops'],
    queryFn: coopsApi.getAll,
  });
}

export function useCreateCoop() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: coopsApi.create,
    onSuccess: () => {
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
