// src/services/tradeService.ts
import apiClient from '../api/apiClient';
import { API_CONFIG } from '../config/api.config';
import { OpenTrade, CompletedTrade, TradeEnum } from '../types/types';

export interface CreateTradeRequest {
  userId: string;                // user creating the trade
  cardId: string;                // card they are listing
  type: TradeEnum;               // FOR_PRICE or FOR_CARD
  price?: number | null;         // only for FOR_PRICE
  wantCardId?: string | null;    // only for FOR_CARD
}

export const tradeService = {
  /**
   * Get all open trades
   */
  getOpenTrades: async (): Promise<OpenTrade[]> => {
    const response = await apiClient.get(
      API_CONFIG.ENDPOINTS.TRADES.GET_ALL
    );
    return response.data.trades;   
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
    const response = await apiClient.get(
      API_CONFIG.ENDPOINTS.TRADES.GET_USER_TRADES(userId)
    );
    return response.data.trades;
  },

  /**
   * Create a new open trade
   */
  createTrade: async (tradeData: CreateTradeRequest): Promise<OpenTrade> => {
    const response = await apiClient.post<OpenTrade>(
      API_CONFIG.ENDPOINTS.TRADES.CREATE,
      {
        // Map TS fields to backend contract; ASP.NET model binding is usually
        // case-insensitive so this is safe:
        type: tradeData.type,
        userId: tradeData.userId,
        cardId: tradeData.cardId,
        price: tradeData.price ?? null,
        wantCardId: tradeData.wantCardId ?? null,
      }
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
