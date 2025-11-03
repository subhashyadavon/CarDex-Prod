/**
 * useTrade Hook
 * Custom hook to access TradeContext
 * Provides trade state and trade management functions
 */

import { useContext } from 'react';
import { TradeContext } from '../context/TradeContext';

export const useTrade = () => {
  const context = useContext(TradeContext);
  
  if (context === undefined) {
    throw new Error('useTrade must be used within a TradeProvider');
  }
  
  return context;
};
