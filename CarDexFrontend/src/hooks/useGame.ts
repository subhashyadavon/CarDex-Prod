/**
 * useGame Hook
 * Custom hook to access GameContext
 * Provides game state and inventory management functions
 */

import { useContext } from 'react';
import { GameContext } from '../context/GameContext';

export const useGame = () => {
  const context = useContext(GameContext);
  
  if (context === undefined) {
    throw new Error('useGame must be used within a GameProvider');
  }
  
  return context;
};
