/**
 * Pack Service
 * Handles pack-related operations: fetching packs and opening packs
 */

import apiClient from "../api/apiClient";
import { API_CONFIG } from "../config/api.config";
// CHANGED: use DTO types that match backend
import type { Pack, CardDetailed, PackResponse } from "../types/types";

// CHANGED: Response when purchasing a pack now matches backend PackPurchaseResponse
export interface PurchasePackResponse {
  pack: PackResponse; // backend PackResponse
  userCurrency: number; // remaining user currency
}

// CHANGED: Response shape for opening a pack matches backend PackOpenResponse
export interface OpenPackResponse {
  cards: CardDetailed[]; // backend CardDetailedResponse[]
  pack: PackResponse; // backend PackResponse
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

  /**
   * Purchase a pack for a given collection
   */
  // CHANGED: now expects backend PackPurchaseResponse shape
  purchasePack: async (collectionId: string): Promise<PurchasePackResponse> => {
    const response = await apiClient.post<PurchasePackResponse>(
      API_CONFIG.ENDPOINTS.PACKS.PURCHASE, // e.g. "/pack/purchase"
      { collectionId }
    );
    return response.data;
  },
};
