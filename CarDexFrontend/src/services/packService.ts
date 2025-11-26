/**
 * Pack Service
 * Handles pack-related operations: fetching packs and opening packs
 */

import apiClient from '../api/apiClient';
import { API_CONFIG } from '../config/api.config';
import { Pack, Card } from '../types/types';

export interface OpenPackResponse {
  cards: Card[];
  pack: Pack;
}

export const packService = {
  /**
   * Get all available packs
   */
  getPacks: async (): Promise<Pack[]> => {
    const response = await apiClient.get<Pack[]>(
      API_CONFIG.ENDPOINTS.PACKS.GET_ALL
    );
    return response.data;
  },

  /**
   * Get pack by ID
   */
  getPackById: async (packId: string): Promise<Pack> => {
    const response = await apiClient.get<Pack>(
      API_CONFIG.ENDPOINTS.PACKS.GET_BY_ID(packId)
    );
    return response.data;
  },

  /**
   * Open a pack and receive cards
   */
  openPack: async (packId: string): Promise<OpenPackResponse> => {
    const response = await apiClient.post<OpenPackResponse>(
      API_CONFIG.ENDPOINTS.PACKS.OPEN_PACK(packId)
    );
    return response.data;
  },
};
