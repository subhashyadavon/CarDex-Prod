/**
 * Trade Service
 * Handles trade operations: listing, creating, accepting, and canceling trades
 */

import apiClient from '../api/apiClient';
import { API_CONFIG } from '../config/api.config';
import { OpenTrade, CompletedTrade } from '../types/types';

export interface CreateTradeRequest {
  offeringUserId: string;
  receivingUserId: string;
  offeredCardIds: string[];
  requestedCardIds: string[];
}

export const tradeService = {
  /**
   * Get all open trades
   */
  getOpenTrades: async (): Promise<OpenTrade[]> => {
    const response = await apiClient.get<OpenTrade[]>(
      API_CONFIG.ENDPOINTS.TRADES.GET_ALL
    );
    return response.data;
  },

  /**
   * Get trade by ID
   */
  getTradeById: async (tradeId: string): Promise<OpenTrade> => {
    const response = await apiClient.get<OpenTrade>(
      API_CONFIG.ENDPOINTS.TRADES.GET_BY_ID(tradeId)
    );
    return response.data;
  },

  /**
   * Get all trades for a specific user
   */
  getUserTrades: async (userId: string): Promise<OpenTrade[]> => {
    const response = await apiClient.get<OpenTrade[]>(
      API_CONFIG.ENDPOINTS.TRADES.GET_USER_TRADES(userId)
    );
    return response.data;
  },

  /**
   * Create a new trade
   */
  createTrade: async (tradeData: CreateTradeRequest): Promise<OpenTrade> => {
    const response = await apiClient.post<OpenTrade>(
      API_CONFIG.ENDPOINTS.TRADES.CREATE,
      tradeData
    );
    return response.data;
  },

  /**
   * Accept a trade
   */
  acceptTrade: async (tradeId: string): Promise<CompletedTrade> => {
    const response = await apiClient.post<CompletedTrade>(
      API_CONFIG.ENDPOINTS.TRADES.ACCEPT(tradeId)
    );
    return response.data;
  },

  /**
   * Cancel a trade
   */
  cancelTrade: async (tradeId: string): Promise<void> => {
    await apiClient.delete(API_CONFIG.ENDPOINTS.TRADES.CANCEL(tradeId));
  },
};
