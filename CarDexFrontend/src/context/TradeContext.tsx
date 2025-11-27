/**
 * Trade Context
 * Manages trade state, filtering, and trade operations
 */

import React, { createContext, useState, ReactNode } from 'react';
import { OpenTrade, CompletedTrade, TradeEnum } from '../types/types';
import { tradeService, CreateTradeRequest } from '../services/tradeService';
import { userService } from '../services/userService';

interface TradeContextType {
  trades: OpenTrade[];
  filteredTrades: OpenTrade[];
  statusFilter: TradeEnum | null;
  isLoading: boolean;
  loadTrades: () => Promise<void>;
  loadUserTrades: (userId: string) => Promise<void>;
  createTrade: (tradeData: CreateTradeRequest) => Promise<OpenTrade>;
  acceptTrade: (tradeId: string) => Promise<CompletedTrade>;
  cancelTrade: (tradeId: string) => Promise<void>;
  setStatusFilter: (status: TradeEnum | null) => void;
  refreshTrades: () => Promise<void>;
}

export const TradeContext = createContext<TradeContextType | undefined>(undefined);

interface TradeProviderProps {
  children: ReactNode;
}

export const TradeProvider: React.FC<TradeProviderProps> = ({ children }) => {
  const [trades, setTrades] = useState<OpenTrade[]>([]);
  const [filteredTrades, setFilteredTrades] = useState<OpenTrade[]>([]);
  const [statusFilter, setStatusFilterState] = useState<TradeEnum | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const loadTrades = async () => {
    setIsLoading(true);
    try {
      const data = await tradeService.getOpenTrades();
      setTrades(data);
      applyFilter(data, statusFilter);
    } catch (error) {
      console.error('Failed to load trades:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const loadUserTrades = async (userId: string) => {
    setIsLoading(true);
    try {
      const data = await tradeService.getUserTrades(userId);
      setTrades(data);
      applyFilter(data, statusFilter);
    } catch (error) {
      console.error('Failed to load user trades:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const createTrade = async (tradeData: CreateTradeRequest): Promise<OpenTrade> => {
    setIsLoading(true);
    try {
      const newTrade = await tradeService.createTrade(tradeData);
      setTrades((prev) => [...prev, newTrade]);
      applyFilter([...trades, newTrade], statusFilter);
      return newTrade;
    } catch (error) {
      console.error('Failed to create trade:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const acceptTrade = async (tradeId: string): Promise<CompletedTrade> => {
    setIsLoading(true);
    try {
      const completedTrade = await tradeService.acceptTrade(tradeId);
      
      // NEW: After trade completes, fetch updated user data
      // This ensures currency is synced if it was a FOR_PRICE trade
      try {
        // We can optionally fetch updated user profile here
        // For now, component using this should handle currency updates
        console.log('[TradeContext] Trade completed, currency may have changed');
      } catch (error) {
        console.error('[TradeContext] Failed to update user data after trade:', error);
      }
      
      setTrades((prev) => prev.filter((t) => t.id !== tradeId));
      applyFilter(trades.filter((t) => t.id !== tradeId), statusFilter);
      return completedTrade;
    } catch (error) {
      console.error('Failed to accept trade:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const cancelTrade = async (tradeId: string): Promise<void> => {
    setIsLoading(true);
    try {
      await tradeService.cancelTrade(tradeId);
      setTrades((prev) => prev.filter((t) => t.id !== tradeId));
      applyFilter(trades.filter((t) => t.id !== tradeId), statusFilter);
    } catch (error) {
      console.error('Failed to cancel trade:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const applyFilter = (tradeList: OpenTrade[], filter: TradeEnum | null) => {
    if (filter === null) {
      setFilteredTrades(tradeList);
    } else {
      setFilteredTrades(tradeList.filter((t) => t.type === filter));
    }
  };

  const setStatusFilter = (status: TradeEnum | null) => {
    setStatusFilterState(status);
    applyFilter(trades, status);
  };

  const refreshTrades = async () => {
    await loadTrades();
  };

  const value: TradeContextType = {
    trades,
    filteredTrades,
    statusFilter,
    isLoading,
    loadTrades,
    loadUserTrades,
    createTrade,
    acceptTrade,
    cancelTrade,
    setStatusFilter,
    refreshTrades,
  };

  return <TradeContext.Provider value={value}>{children}</TradeContext.Provider>;
};
