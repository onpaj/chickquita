import { useQuery } from '@tanstack/react-query';
import { coopsApi } from '../api/coopsApi';

export function useCoopDetail(id: string) {
  return useQuery({
    queryKey: ['coops', id],
    queryFn: () => coopsApi.getById(id),
    enabled: !!id,
  });
}
