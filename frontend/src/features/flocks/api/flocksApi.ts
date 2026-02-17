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
  initialHens: number;
  initialRoosters: number;
  initialChicks: number;
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
   * Retrieves all flocks for a specific coop.
   * Backend endpoint: GET /api/coops/{coopId}/flocks?includeInactive={bool}
   */
  getAll: async (coopId: string, includeInactive: boolean = false): Promise<Flock[]> => {
    const response = await apiClient.get<Flock[]>(
      `/coops/${coopId}/flocks?includeInactive=${includeInactive}`
    );
    return response.data;
  },

  /**
   * Retrieves a specific flock by ID.
   * Backend endpoint: GET /api/flocks/{flockId}
   * Includes full composition history.
   */
  getById: async (_coopId: string, flockId: string): Promise<Flock> => {
    const response = await apiClient.get<Flock>(`/flocks/${flockId}`);
    return response.data;
  },

  /**
   * Creates a new flock with initial composition.
   * Automatically creates the first history entry.
   * Note: Backend endpoint not yet implemented
   */
  create: async (data: CreateFlockRequest): Promise<Flock> => {
    const response = await apiClient.post<Flock>(`/coops/${data.coopId}/flocks`, data);
    return response.data;
  },

  /**
   * Updates an existing flock's basic information.
   * Does not affect composition - use composition change endpoints for that.
   * Note: Backend endpoint not yet implemented
   */
  update: async (coopId: string, data: UpdateFlockRequest): Promise<Flock> => {
    const response = await apiClient.put<Flock>(`/coops/${coopId}/flocks/${data.id}`, data);
    return response.data;
  },

  /**
   * Archives a flock (sets isActive to false).
   * Note: Backend endpoint not yet implemented
   */
  archive: async (coopId: string, flockId: string): Promise<boolean> => {
    const response = await apiClient.patch<boolean>(`/coops/${coopId}/flocks/${flockId}/archive`);
    return response.data;
  },

  /**
   * Retrieves the full history timeline for a flock.
   * Backend endpoint: GET /api/flocks/{flockId}/history
   * Returns all FlockHistory entries sorted by changeDate DESC.
   */
  getHistory: async (flockId: string): Promise<FlockHistory[]> => {
    const response = await apiClient.get<FlockHistory[]>(`/flocks/${flockId}/history`);
    return response.data;
  },

  /**
   * Updates the notes field on a flock history entry.
   * Backend endpoint: PATCH /api/flock-history/{historyId}/notes
   * This is the only mutable field on history records.
   */
  updateHistoryNotes: async (historyId: string, notes: string | null): Promise<FlockHistory> => {
    const response = await apiClient.patch<FlockHistory>(`/flock-history/${historyId}/notes`, {
      notes,
    });
    return response.data;
  },
};
