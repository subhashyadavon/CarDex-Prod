/**
 * Card Service
 * Handles card-related operations: fetching cards, user cards, card details
 */

import apiClient from '../api/apiClient';
import { API_CONFIG } from '../config/api.config';
import { Card } from '../types/types';

export const cardService = {
  /**
   * Get all available cards
   */
  getCards: async (): Promise<Card[]> => {
    const response = await apiClient.get<Card[]>(
      API_CONFIG.ENDPOINTS.CARDS.GET_ALL
    );
    return response.data;
  },

  /**
   * Get card by ID
   */
  getCardById: async (cardId: number): Promise<Card> => {
    const response = await apiClient.get<Card>(
      API_CONFIG.ENDPOINTS.CARDS.GET_BY_ID(cardId)
    );
    return response.data;
  },

  /**
   * Get all cards owned by a specific user
   */
  getUserCards: async (userId: number): Promise<Card[]> => {
    const response = await apiClient.get<Card[]>(
      API_CONFIG.ENDPOINTS.CARDS.GET_USER_CARDS(userId)
    );
    return response.data;
  },
};
