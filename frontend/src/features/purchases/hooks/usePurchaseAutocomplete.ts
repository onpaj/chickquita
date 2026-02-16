import { useQuery } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import { purchasesApi } from '../api/purchasesApi';

/**
 * Hook for purchase name autocomplete with debouncing.
 * Fetches purchase name suggestions based on user input.
 *
 * @param query - Search query string (minimum 2 characters required)
 * @returns Object containing suggestions array and loading state
 */
export function usePurchaseAutocomplete(query: string) {
  const [debouncedQuery, setDebouncedQuery] = useState(query);

  // Debounce the query with 300ms delay
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedQuery(query);
    }, 300);

    return () => {
      clearTimeout(timer);
    };
  }, [query]);

  const { data, isLoading } = useQuery({
    queryKey: ['purchase-names', debouncedQuery],
    queryFn: () => purchasesApi.getPurchaseNames(debouncedQuery),
    enabled: debouncedQuery.length >= 2,
    staleTime: 10 * 60 * 1000, // 10 minutes
  });

  return {
    suggestions: debouncedQuery.length >= 2 && Array.isArray(data) ? data : [],
    isLoading,
  };
}
