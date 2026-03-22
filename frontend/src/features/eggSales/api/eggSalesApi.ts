import { apiClient } from '../../../lib/apiClient';
import type {
  EggSaleDto,
  CreateEggSaleDto,
  UpdateEggSaleDto,
  EggSaleFilterParams,
} from '../types/eggSale.types';

/**
 * API client for egg sale-related operations.
 * All endpoints automatically include authentication via apiClient.
 */
export const eggSalesApi = {
  /**
   * Retrieves all egg sales with optional date range filtering.
   * Backend endpoint: GET /api/egg-sales?fromDate={date}&toDate={date}
   *
   * @param params - Optional filter parameters (date range)
   * @returns Promise resolving to an array of EggSaleDto
   * @throws Error if the API request fails
   */
  getEggSales: async (params?: EggSaleFilterParams): Promise<EggSaleDto[]> => {
    const queryParams = new URLSearchParams();

    if (params?.fromDate) {
      queryParams.append('fromDate', params.fromDate);
    }
    if (params?.toDate) {
      queryParams.append('toDate', params.toDate);
    }

    const queryString = queryParams.toString();
    const url = `/egg-sales${queryString ? `?${queryString}` : ''}`;

    const response = await apiClient.get<EggSaleDto[]>(url);
    return response.data;
  },

  /**
   * Retrieves a specific egg sale by ID.
   * Backend endpoint: GET /api/egg-sales/{id}
   *
   * @param id - The unique identifier of the egg sale (UUID)
   * @returns Promise resolving to the EggSaleDto
   * @throws Error if the egg sale is not found or request fails
   */
  getEggSaleById: async (id: string): Promise<EggSaleDto> => {
    const response = await apiClient.get<EggSaleDto>(`/egg-sales/${id}`);
    return response.data;
  },

  /**
   * Creates a new egg sale record.
   * Backend endpoint: POST /api/egg-sales
   *
   * @param data - The egg sale data (CreateEggSaleDto)
   * @returns Promise resolving to the created EggSaleDto with generated ID and timestamps
   * @throws Error if validation fails or request fails
   */
  createEggSale: async (data: CreateEggSaleDto): Promise<EggSaleDto> => {
    const response = await apiClient.post<EggSaleDto>('/egg-sales', data);
    return response.data;
  },

  /**
   * Updates an existing egg sale record.
   * Backend endpoint: PUT /api/egg-sales/{id}
   *
   * @param data - The updated egg sale data (UpdateEggSaleDto with id)
   * @returns Promise resolving to the updated EggSaleDto
   * @throws Error if the egg sale is not found, validation fails, or request fails
   */
  updateEggSale: async (data: UpdateEggSaleDto): Promise<EggSaleDto> => {
    const response = await apiClient.put<EggSaleDto>(`/egg-sales/${data.id}`, data);
    return response.data;
  },

  /**
   * Deletes an egg sale record permanently.
   * Backend endpoint: DELETE /api/egg-sales/{id}
   *
   * @param id - The unique identifier of the egg sale to delete (UUID)
   * @returns Promise resolving to true if deletion was successful
   * @throws Error if the egg sale is not found or request fails
   */
  deleteEggSale: async (id: string): Promise<boolean> => {
    await apiClient.delete(`/egg-sales/${id}`);
    return true;
  },
};
