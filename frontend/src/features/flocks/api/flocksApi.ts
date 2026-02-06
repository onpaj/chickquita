import { apiClient } from '../../../lib/apiClient';

/**
 * Data Transfer Object for FlockHistory entity.
 * Represents a point-in-time snapshot of flock composition.
 */
export interface FlockHistory {
  id: string;
  tenantId: string;
  flockId: string;
  changeDate: string;
  hens: number;
  roosters: number;
  chicks: number;
  notes?: string;
  reason: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Data Transfer Object for Flock entity.
 * Represents a group of chickens with their current composition and history.
 */
export interface Flock {
  id: string;
  tenantId: string;
  coopId: string;
  identifier: string;
  hatchDate: string;
  currentHens: number;
  currentRoosters: number;
  currentChicks: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  history: FlockHistory[];
}

/**
 * Request payload for creating a new flock.
 */
export interface CreateFlockRequest {
  coopId: string;
  identifier: string;
  hatchDate: string;
  currentHens: number;
  currentRoosters: number;
  currentChicks: number;
}

/**
 * Request payload for updating an existing flock.
 */
export interface UpdateFlockRequest {
  id: string;
  identifier: string;
  hatchDate: string;
  currentHens: number;
  currentRoosters: number;
  currentChicks: number;
}

/**
 * API client for flock-related operations.
 * All endpoints automatically include authentication via apiClient.
 */
export const flocksApi = {
  /**
   * Retrieves all flocks for the authenticated user's tenant.
   * Optionally filters by coop if coopId is provided.
   */
  getAll: async (coopId?: string): Promise<Flock[]> => {
    const url = coopId ? `/flocks?coopId=${coopId}` : '/flocks';
    const response = await apiClient.get<Flock[]>(url);
    return response.data;
  },

  /**
   * Retrieves a specific flock by ID.
   * Includes full composition history.
   */
  getById: async (id: string): Promise<Flock> => {
    const response = await apiClient.get<Flock>(`/flocks/${id}`);
    return response.data;
  },

  /**
   * Creates a new flock with initial composition.
   * Automatically creates the first history entry.
   */
  create: async (data: CreateFlockRequest): Promise<Flock> => {
    const response = await apiClient.post<Flock>('/flocks', data);
    return response.data;
  },

  /**
   * Updates an existing flock's basic information.
   * Does not affect composition - use composition change endpoints for that.
   */
  update: async (data: UpdateFlockRequest): Promise<Flock> => {
    const response = await apiClient.put<Flock>(`/flocks/${data.id}`, data);
    return response.data;
  },

  /**
   * Deletes a flock permanently.
   * This operation cannot be undone.
   */
  delete: async (id: string): Promise<boolean> => {
    const response = await apiClient.delete<boolean>(`/flocks/${id}`);
    return response.data;
  },
};
