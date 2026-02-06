import { apiClient } from '../../../lib/apiClient';

export interface Coop {
  id: string;
  tenantId: string;
  name: string;
  location?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCoopRequest {
  name: string;
  location?: string;
}

export const coopsApi = {
  getAll: async (): Promise<Coop[]> => {
    const response = await apiClient.get<Coop[]>('/coops');
    return response.data;
  },

  getById: async (id: string): Promise<Coop> => {
    const response = await apiClient.get<Coop>(`/coops/${id}`);
    return response.data;
  },

  create: async (data: CreateCoopRequest): Promise<Coop> => {
    const response = await apiClient.post<Coop>('/coops', data);
    return response.data;
  },
};
