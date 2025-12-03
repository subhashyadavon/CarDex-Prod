/**
 * Card Service
 * Handles card-related operations: fetching cards, user cards, card details
 */

import apiClient from "../api/apiClient";
import { API_CONFIG } from "../config/api.config";
import { Card, CardWithVehicleListResponse } from "../types/types";

/**
 * Vehicle type for /cards/vehicles response.
 * Adjust fields if your backend returns slightly different names.
 */
export type Vehicle = {
  id?: string; // sometimes vehicles may not expose id in the DTO
  year: number | string;
  make: string;
  model: string;
  stat1: number;
  stat2: number;
  stat3: number;
  value: number;
  image?: string;      // legacy / alt field
  imageUrl?: string;   // 👈 main field from your API sample
};

export const cardService = {
  /**
   * Get all available cards
   * GET /cards
   */
  getCards: async (): Promise<Card[]> => {
    const response = await apiClient.get<Card[]>(
      API_CONFIG.ENDPOINTS.CARDS.GET_ALL
    );
    return response.data;
  },

  /**
   * Get card by ID
   *
   * IMPORTANT:
   * - Only calls /cards/{cardId}
   * - Does NOT call /cards/vehicle/{vehicleId}
   *   (we rely on the global /cards/vehicles list for stats)
   */
  getCardById: async (cardId: string): Promise<any> => {
    const response = await apiClient.get<any>(
      API_CONFIG.ENDPOINTS.CARDS.GET_BY_ID(cardId)
    );
    return response.data;
  },

  /**
   * Get all cards owned by a specific user
   * GET /cards/user/{userId}
   */
  getUserCards: async (userId: string): Promise<Card[]> => {
    const response = await apiClient.get<Card[]>(
      API_CONFIG.ENDPOINTS.CARDS.GET_USER_CARDS(userId)
    );
    return response.data;
  },

  /**
   * Get user's cards with full vehicle details embedded
   * This is optimized for UI display - includes make, model, year, stats, and image
   * @param userId - User ID to fetch cards for
   * @param collectionId - Optional: Filter by collection
   * @param grade - Optional: Filter by grade (FACTORY, LIMITED_RUN, NISMO)
   * @param limit - Number of results per page (default 50)
   * @param offset - Number of results to skip (default 0)
   */
  getUserCardsWithVehicles: async (
    userId: string,
    collectionId?: string,
    grade?: string,
    limit: number = 50,
    offset: number = 0
  ): Promise<CardWithVehicleListResponse> => {
    const params = new URLSearchParams({
      limit: limit.toString(),
      offset: offset.toString(),
    });

    if (collectionId) params.append("collectionId", collectionId);
    if (grade) params.append("grade", grade);

    const response = await apiClient.get<CardWithVehicleListResponse>(
      `${API_CONFIG.ENDPOINTS.USERS.GET_CARDS_WITH_VEHICLES(
        userId
      )}?${params.toString()}`
    );
    return response.data;
  },

  /**
   * Get all vehicles (raw vehicle stats list)
   *
   * Backend may return either:
   *   Vehicle[]
   * or:
   *   { vehicles: Vehicle[] }
   *
   * This handles both shapes defensively.
   */
  getAllVehicles: async (): Promise<Vehicle[]> => {
    const response = await apiClient.get<any>(
      API_CONFIG.ENDPOINTS.CARDS.GET_ALL_VEHICLES
    );

    if (Array.isArray(response.data)) {
      return response.data as Vehicle[];
    }

    if (response.data && Array.isArray(response.data.vehicles)) {
      return response.data.vehicles as Vehicle[];
    }

    console.warn(
      "[cardService] Unexpected /cards/vehicles response shape:",
      response.data
    );
    return [];
  },

  /**
   * NOTE:
   * - There is NO getVehicleById here anymore.
   * - There is NO usage of /cards/vehicle/{id}.
   */
};
