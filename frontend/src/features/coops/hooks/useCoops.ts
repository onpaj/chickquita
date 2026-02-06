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
