/**
 * Game Context - Game Inventory and Pack Management
 * 
 * This context manages all game-related state:
 * - Available cards (catalog)
 * - User's owned cards (inventory)
 * - Available packs
 * - User's collection progress
 * 
 * DIFFERENCE FROM AUTHCONTEXT:
 * - AuthContext: Global, single user session
 * - GameContext: Dynamic, loads different data based on current user
 * 
 * WHY SEPARATE FROM AUTH?
 * - Separation of concerns: Auth = who you are, Game = what you have
 * - Different loading patterns: Auth loads once, Game reloads often
 * - Easier to test and reason about
 * 
 * USAGE PATTERN:
 * 1. User logs in (AuthContext has user.id)
 * 2. Garage page calls loadUserCards(user.id)
 * 3. Pack page calls loadPacks()
 * 4. User opens pack → calls openPack() → calls refreshInventory()
 * 5. State updates, all components using useGame() re-render
 */

import React, { createContext, useState, ReactNode } from 'react';
import { Card, Pack, Collection } from '../types/types';
import { cardService } from '../services/cardService';
import { packService, OpenPackResponse } from '../services/packService';
import { collectionService } from '../services/collectionService';

/**
 * GameContextType: Shape of data this context provides
 * 
 * STATE (what data we store):
 * - cards: All cards available in the game (catalog)
 * - userCards: Cards owned by current user
 * - packs: Available packs to purchase/open
 * - collection: User's collection completion status
 * - isLoading: True while any API call is in progress
 * 
 * FUNCTIONS (how to update the data):
 * - loadCards: Fetch all available cards
 * - loadUserCards: Fetch cards owned by specific user
 * - loadPacks: Fetch available packs
 * - loadCollection: Fetch user's collection
 * - openPack: Open a pack and receive cards
 * - refreshInventory: Reload all user data (cards + packs + collection)
 */
interface GameContextType {
  cards: Card[];                                      // All cards in game
  userCards: Card[];                                  // User's owned cards
  packs: Pack[];                                      // Available packs
  collection: Collection | null;                      // User's collection status
  isLoading: boolean;                                 // Loading state
  loadCards: () => Promise<void>;                     // Load card catalog
  loadUserCards: (userId: number) => Promise<void>;  // Load user's cards
  loadPacks: () => Promise<void>;                     // Load available packs
  loadCollection: (userId: number) => Promise<void>; // Load user's collection
  openPack: (packId: number) => Promise<OpenPackResponse>; // Open pack
  refreshInventory: (userId: number) => Promise<void>; // Reload all user data
}

export const GameContext = createContext<GameContextType | undefined>(undefined);

interface GameProviderProps {
  children: ReactNode;
}

export const GameProvider: React.FC<GameProviderProps> = ({ children }) => {
  const [cards, setCards] = useState<Card[]>([]);
  const [userCards, setUserCards] = useState<Card[]>([]);
  const [packs, setPacks] = useState<Pack[]>([]);
  const [collection, setCollection] = useState<Collection | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const loadCards = async () => {
    setIsLoading(true);
    try {
      const data = await cardService.getCards();
      setCards(data);
    } catch (error) {
      console.error('Failed to load cards:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const loadUserCards = async (userId: number) => {
    setIsLoading(true);
    try {
      const data = await cardService.getUserCards(userId);
      setUserCards(data);
    } catch (error) {
      console.error('Failed to load user cards:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const loadPacks = async () => {
    setIsLoading(true);
    try {
      const data = await packService.getPacks();
      setPacks(data);
    } catch (error) {
      console.error('Failed to load packs:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const loadCollection = async (userId: number) => {
    setIsLoading(true);
    try {
      const data = await collectionService.getCollection(userId);
      setCollection(data);
    } catch (error) {
      console.error('Failed to load collection:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const openPack = async (packId: number): Promise<OpenPackResponse> => {
    setIsLoading(true);
    try {
      const result = await packService.openPack(packId);
      return result;
    } catch (error) {
      console.error('Failed to open pack:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const refreshInventory = async (userId: number) => {
    await Promise.all([
      loadUserCards(userId),
      loadPacks(),
      loadCollection(userId),
    ]);
  };

  const value: GameContextType = {
    cards,
    userCards,
    packs,
    collection,
    isLoading,
    loadCards,
    loadUserCards,
    loadPacks,
    loadCollection,
    openPack,
    refreshInventory,
  };

  return <GameContext.Provider value={value}>{children}</GameContext.Provider>;
};
