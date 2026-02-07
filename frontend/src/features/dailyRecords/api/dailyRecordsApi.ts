import { apiClient } from '../../../lib/apiClient';

/**
 * Data Transfer Object for DailyRecord entity.
 * Represents a daily egg production record for a specific flock.
 */
export interface DailyRecordDto {
  id: string;
  tenantId: string;
  flockId: string;
  recordDate: string; // ISO 8601 date string (YYYY-MM-DD)
  eggCount: number;
  notes?: string;
  createdAt: string; // ISO 8601 datetime string
  updatedAt: string; // ISO 8601 datetime string
}

/**
 * Request payload for creating a new daily record.
 * FlockId is typically provided in the route parameter.
 */
export interface CreateDailyRecordRequest {
  flockId: string;
  recordDate: string; // ISO 8601 date string (YYYY-MM-DD)
  eggCount: number;
  notes?: string;
}

/**
 * Request payload for updating an existing daily record.
 * Only allows updates on the same day the record was created (same-day edit restriction).
 * RecordDate and FlockId cannot be changed via update.
 */
export interface UpdateDailyRecordRequest {
  id: string;
  eggCount: number;
  notes?: string;
}

/**
 * Query parameters for retrieving daily records.
 * All parameters are optional to allow flexible filtering.
 */
export interface GetDailyRecordsParams {
  flockId?: string;
  startDate?: string; // ISO 8601 date string (YYYY-MM-DD)
  endDate?: string; // ISO 8601 date string (YYYY-MM-DD)
}

/**
 * API client for daily record-related operations.
 * All endpoints automatically include authentication via apiClient.
 */
export const dailyRecordsApi = {
  /**
   * Retrieves all daily records with optional filtering.
   * Backend endpoint: GET /api/daily-records?flockId={guid}&startDate={date}&endDate={date}
   */
  getAll: async (params?: GetDailyRecordsParams): Promise<DailyRecordDto[]> => {
    const queryParams = new URLSearchParams();
    if (params?.flockId) queryParams.append('flockId', params.flockId);
    if (params?.startDate) queryParams.append('startDate', params.startDate);
    if (params?.endDate) queryParams.append('endDate', params.endDate);

    const queryString = queryParams.toString();
    const url = `/daily-records${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.get<DailyRecordDto[]>(url);
    return response.data;
  },

  /**
   * Retrieves all daily records for a specific flock with optional date range filtering.
   * Backend endpoint: GET /api/flocks/{flockId}/daily-records?startDate={date}&endDate={date}
   */
  getByFlock: async (
    flockId: string,
    params?: Omit<GetDailyRecordsParams, 'flockId'>
  ): Promise<DailyRecordDto[]> => {
    const queryParams = new URLSearchParams();
    if (params?.startDate) queryParams.append('startDate', params.startDate);
    if (params?.endDate) queryParams.append('endDate', params.endDate);

    const queryString = queryParams.toString();
    const url = `/flocks/${flockId}/daily-records${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.get<DailyRecordDto[]>(url);
    return response.data;
  },

  /**
   * Creates a new daily record for a specific flock.
   * Backend endpoint: POST /api/flocks/{flockId}/daily-records
   * Returns the created daily record with generated ID and timestamps.
   */
  create: async (flockId: string, data: Omit<CreateDailyRecordRequest, 'flockId'>): Promise<DailyRecordDto> => {
    const response = await apiClient.post<DailyRecordDto>(`/flocks/${flockId}/daily-records`, data);
    return response.data;
  },

  /**
   * Updates an existing daily record.
   * Backend endpoint: PUT /api/daily-records/{id}
   * Only allows updates on the same day the record was created (same-day edit restriction).
   * Returns the updated daily record.
   */
  update: async (data: UpdateDailyRecordRequest): Promise<DailyRecordDto> => {
    const response = await apiClient.put<DailyRecordDto>(`/daily-records/${data.id}`, data);
    return response.data;
  },

  /**
   * Deletes a daily record.
   * Backend endpoint: DELETE /api/daily-records/{id}
   * Returns true if deletion was successful.
   */
  delete: async (id: string): Promise<boolean> => {
    await apiClient.delete(`/daily-records/${id}`);
    return true;
  },
};
