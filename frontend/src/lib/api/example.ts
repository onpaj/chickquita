/**
 * Example API Usage
 *
 * This file demonstrates how to use the configured API client
 * with automatic JWT token injection from Clerk.
 *
 * DO NOT USE THIS FILE IN PRODUCTION - it's for reference only.
 */

import apiClient from '../apiClient';

// Example interfaces for API responses
interface User {
  id: string;
  email: string;
  tenantId: string;
}

interface Coop {
  id: string;
  name: string;
  location: string;
}

/**
 * Example: GET request
 */
export const getUser = async (): Promise<User> => {
  const response = await apiClient.get<User>('/users/me');
  return response.data;
};

/**
 * Example: GET request with parameters
 */
export const getCoops = async (limit?: number): Promise<Coop[]> => {
  const response = await apiClient.get<Coop[]>('/coops', {
    params: { limit },
  });
  return response.data;
};

/**
 * Example: POST request
 */
export const createCoop = async (data: Omit<Coop, 'id'>): Promise<Coop> => {
  const response = await apiClient.post<Coop>('/coops', data);
  return response.data;
};

/**
 * Example: PUT request
 */
export const updateCoop = async (id: string, data: Partial<Coop>): Promise<Coop> => {
  const response = await apiClient.put<Coop>(`/coops/${id}`, data);
  return response.data;
};

/**
 * Example: DELETE request
 */
export const deleteCoop = async (id: string): Promise<void> => {
  await apiClient.delete(`/coops/${id}`);
};

/**
 * Example: Error handling with try-catch
 */
export const getCoopWithErrorHandling = async (id: string): Promise<Coop | null> => {
  try {
    const response = await apiClient.get<Coop>(`/coops/${id}`);
    return response.data;
  } catch (error) {
    // Error is already logged by the response interceptor
    // Here you can add additional error handling, e.g., showing a toast
    console.error('Failed to fetch coop:', error);
    return null;
  }
};

/**
 * Example: Using with React Query
 *
 * @example
 * ```tsx
 * import { useQuery } from '@tanstack/react-query';
 * import { getCoops } from './lib/api/example';
 *
 * function CoopsList() {
 *   const { data, isLoading, error } = useQuery({
 *     queryKey: ['coops'],
 *     queryFn: () => getCoops(),
 *   });
 *
 *   if (isLoading) return <div>Loading...</div>;
 *   if (error) return <div>Error loading coops</div>;
 *
 *   return (
 *     <ul>
 *       {data?.map(coop => (
 *         <li key={coop.id}>{coop.name}</li>
 *       ))}
 *     </ul>
 *   );
 * }
 * ```
 */

/**
 * Example: Using with mutations
 *
 * @example
 * ```tsx
 * import { useMutation, useQueryClient } from '@tanstack/react-query';
 * import { createCoop } from './lib/api/example';
 *
 * function CreateCoopForm() {
 *   const queryClient = useQueryClient();
 *
 *   const mutation = useMutation({
 *     mutationFn: createCoop,
 *     onSuccess: () => {
 *       // Invalidate and refetch coops query
 *       queryClient.invalidateQueries({ queryKey: ['coops'] });
 *     },
 *   });
 *
 *   const handleSubmit = (formData) => {
 *     mutation.mutate(formData);
 *   };
 *
 *   return <form onSubmit={handleSubmit}>...</form>;
 * }
 * ```
 */
