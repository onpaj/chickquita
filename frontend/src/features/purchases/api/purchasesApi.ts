import { apiClient } from '../../../lib/apiClient';
import type {
  PurchaseDto,
  CreatePurchaseDto,
  UpdatePurchaseDto,
  PurchaseFilterParams,
} from '../types/purchase.types';

/**
 * API client for purchase-related operations.
 * All endpoints automatically include authentication via apiClient.
 */
export const purchasesApi = {
  /**
   * Retrieves all purchases with optional filtering.
   * Backend endpoint: GET /api/v1/purchases?fromDate={date}&toDate={date}&type={number}&flockId={guid}
   *
   * @param params - Optional filter parameters (date range, type, flockId)
   * @returns Promise resolving to an array of PurchaseDto
   * @throws Error if the API request fails
   */
  getPurchases: async (params?: PurchaseFilterParams): Promise<PurchaseDto[]> => {
    const queryParams = new URLSearchParams();

    if (params?.fromDate) {
      queryParams.append('fromDate', params.fromDate);
    }
    if (params?.toDate) {
      queryParams.append('toDate', params.toDate);
    }
    if (params?.type !== undefined && params?.type !== null) {
      queryParams.append('type', params.type.toString());
    }
    if (params?.flockId) {
      queryParams.append('flockId', params.flockId);
    }

    const queryString = queryParams.toString();
    const url = `/purchases${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.get<PurchaseDto[]>(url);
    return response.data;
  },

  /**
   * Retrieves a specific purchase by ID.
   * Backend endpoint: GET /api/v1/purchases/{id}
   *
   * @param id - The unique identifier of the purchase (UUID)
   * @returns Promise resolving to the PurchaseDto
   * @throws Error if the purchase is not found or request fails
   */
  getPurchaseById: async (id: string): Promise<PurchaseDto> => {
    const response = await apiClient.get<PurchaseDto>(`/purchases/${id}`);
    return response.data;
  },

  /**
   * Retrieves a list of distinct purchase names for autocomplete functionality.
   * Backend endpoint: GET /api/v1/purchases/names?query={string}&limit={number}
   *
   * @param query - Optional search query to filter names (default: undefined)
   * @param limit - Maximum number of results to return (default: 20)
   * @returns Promise resolving to an array of purchase name strings
   * @throws Error if the API request fails
   */
  getPurchaseNames: async (query?: string, limit: number = 20): Promise<string[]> => {
    const queryParams = new URLSearchParams();

    if (query) {
      queryParams.append('query', query);
    }
    queryParams.append('limit', limit.toString());

    const queryString = queryParams.toString();
    const url = `/purchases/names${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.get<string[]>(url);
    return response.data;
  },

  /**
   * Creates a new purchase record.
   * Backend endpoint: POST /api/v1/purchases
   *
   * Date serialization: Dates should be provided as ISO 8601 strings (YYYY-MM-DD or full ISO datetime).
   * The backend will parse and validate the dates.
   *
   * @param data - The purchase data (CreatePurchaseDto)
   * @returns Promise resolving to the created PurchaseDto with generated ID and timestamps
   * @throws Error if validation fails or request fails
   */
  createPurchase: async (data: CreatePurchaseDto): Promise<PurchaseDto> => {
    const response = await apiClient.post<PurchaseDto>('/purchases', data);
    return response.data;
  },

  /**
   * Updates an existing purchase record.
   * Backend endpoint: PUT /api/v1/purchases/{id}
   *
   * Date serialization: Dates should be provided as ISO 8601 strings (YYYY-MM-DD or full ISO datetime).
   * The backend will parse and validate the dates.
   *
   * @param data - The updated purchase data (UpdatePurchaseDto with id)
   * @returns Promise resolving to the updated PurchaseDto
   * @throws Error if the purchase is not found, validation fails, or request fails
   */
  updatePurchase: async (data: UpdatePurchaseDto): Promise<PurchaseDto> => {
    const response = await apiClient.put<PurchaseDto>(`/purchases/${data.id}`, data);
    return response.data;
  },

  /**
   * Deletes a purchase record permanently.
   * Backend endpoint: DELETE /api/v1/purchases/{id}
   *
   * @param id - The unique identifier of the purchase to delete (UUID)
   * @returns Promise resolving to true if deletion was successful
   * @throws Error if the purchase is not found or request fails
   */
  deletePurchase: async (id: string): Promise<boolean> => {
    await apiClient.delete(`/purchases/${id}`);
    return true;
  },
};
