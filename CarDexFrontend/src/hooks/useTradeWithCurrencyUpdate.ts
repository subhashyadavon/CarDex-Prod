/**
 * useTradeWithCurrencyUpdate Hook
 * 
 * Custom hook that wraps trade operations with automatic currency updates.
 * When a user accepts a trade, this hook fetches the updated user profile
 * to sync the frontend display with backend state.
 * 
 * USAGE:
 * const { acceptTradeAndUpdate } = useTradeWithCurrencyUpdate();
 * await acceptTradeAndUpdate(tradeId);
 */

import { useAuth } from './useAuth';
import { useTrade } from './useTrade';
import { userService } from '../services/userService';

export const useTradeWithCurrencyUpdate = () => {
  const { user, updateUserCurrency } = useAuth();
  const { acceptTrade } = useTrade();

  const acceptTradeAndUpdate = async (tradeId: string) => {
    try {
      // 1. Accept the trade via TradeContext
      const completedTrade = await acceptTrade(tradeId);
      console.log('[useTradeWithCurrencyUpdate] Trade accepted:', completedTrade);

      // 2. Fetch updated user profile to get new currency balance
      if (user?.id) {
        const updatedUser = await userService.getProfile(user.id);
        console.log('[useTradeWithCurrencyUpdate] Updated user profile:', updatedUser);
        
        // 3. Update currency in auth context
        updateUserCurrency(updatedUser.currency);
        console.log('[useTradeWithCurrencyUpdate] Updated currency to:', updatedUser.currency);
      }

      return completedTrade;
    } catch (error) {
      console.error('[useTradeWithCurrencyUpdate] Failed to accept trade:', error);
      throw error;
    }
  };

  return {
    acceptTradeAndUpdate,
  };
};
