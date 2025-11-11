
// ENUMS

// > Card grade rarity, from common -> uncommon -> rare
export enum GradeEnum {
  FACTORY = "FACTORY",
  LIMITED_RUN = "LIMITED_RUN",
  NISMO = "NISMO"
}

// > Trade type, signifying what the User wants in return
export enum TradeEnum {
  FOR_CARD = "FOR_CARD",
  FOR_PRICE = "FOR_PRICE"
}

// > Reward type. Note that all completed trades give a 'reward'
//   to be claimed, instead of giving it to the user directly
export enum RewardEnum {
  PACK = "PACK",
  CURRENCY = "CURRENCY",
  CARD_FROM_TRADE = "CARD_FROM_TRADE",
  CURRENCY_FROM_TRADE = "CURRENCY_FROM_TRADE"
}


export interface User {
  id: number;
  username: string;
  password: string;
  currency: number;
  owned_cards: number[];
  owned_packs: number[];
  open_trades: number[];
  trade_history: string[];
}

export interface Vehicle {
  id: number;
  year: string;
  make: string;
  model: string;
  stat1: number;
  stat2: number;
  statN: number;
  value: number;
  image: string;
}

export interface Card {
  id: number;
  user_id: number;
  vehicle_id: number;
  collection_id: number;
  grade: GradeEnum;
  value: number;
}

export interface Pack {
  id: number;
  user_id: number;
  collection_id: number;
  value: number;
}

export interface Collection {
  id: number;
  vehicles: number[];
  name: string;
  image: string;
  pack_price: number;
}

export interface OpenTrade {
  id: number;
  type: TradeEnum;
  user_id: number;
  card_id: number;
  price: number;
  want_card_id: number | null;
}

export interface CompletedTrade {
  id: number;
  type: TradeEnum;
  seller_user_id: number;
  seller_card_id: number;
  buyer_user_id: number;
  buyer_card_id: number | null;
  executed_date: Date;
  price: number;
}

export interface Reward {
  id: number;
  user_id: number;
  type: RewardEnum;
  item_id: number | null;
  amount: number | null;
  created_at: Date;
  claimed_at: Date | null;
}

// Helper types for validation and responses
export interface ValidationResult {
  valid: boolean;
  error?: string;
}

export interface TradeValidation extends ValidationResult {
  trade?: CompletedTrade;
}

/**
 * Represents a user's progress in a specific collection
 */
export interface CollectionProgress {
  collectionId: number;
  collectionName: string;
  collectionImage: string;
  ownedVehicles: number;   
  totalVehicles: number;     // Total unique vehicles in collection
  percentage: number;        
}

/**
 * Response from GET /users/{userId}/collection-progress
 * Contains progress data for all collections where user owns at least 1 card
 */
export interface CollectionProgressResponse {
  collections: CollectionProgress[];
  totalCollections: number;  // How many collections user has cards from
}
