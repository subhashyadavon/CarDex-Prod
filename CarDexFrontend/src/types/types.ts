
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
  id: string;
  username: string;
  password: string;
  currency: number;
  // Note: Backend uses relational DB - cards/packs are fetched via separate endpoints
  // These arrays are kept for backward compatibility but may be deprecated
  owned_cards: string[];
  owned_packs: string[];
  open_trades: string[];
  trade_history: string[];
}

export interface Vehicle {
  id: string;
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
  id: string;
  user_id: string;
  vehicle_id: string;
  collection_id: string;
  grade: GradeEnum;
  value: number;
}

export interface Pack {
  id: string;
  user_id: string;
  collection_id: string;
  value: number;
}

export interface Collection {
  id: string;
  vehicles: string[];
  name: string;
  image: string;
  pack_price: number;
}

export interface OpenTrade {
  id: string;
  type: TradeEnum;
  user_id: string;
  card_id: string;
  price: number;
  want_card_id: string | null;
}

export interface CompletedTrade {
  id: string;
  type: TradeEnum;
  seller_user_id: string;
  seller_card_id: string;
  buyer_user_id: string;
  buyer_card_id: string | null;
  executed_date: Date;
  price: number;
}

export interface Reward {
  id: string;
  user_id: string;
  type: RewardEnum;
  item_id: string | null;
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
  collectionId: string;
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

/**
 * Detailed vehicle information
 * Used when fetching cards with full vehicle data
 */
export interface VehicleDetails {
  id: string;
  year: string;
  make: string;
  model: string;
  stat1: number;
  stat2: number;
  stat3: number;
  value: number;
  image: string;
}

/**
 * Card with embedded vehicle details
 * Returned by GET /users/{userId}/cards/with-vehicles
 * Combines card metadata (grade, value) with full vehicle info
 */
export interface CardWithVehicle {
  id: string;
  vehicleId: string;
  collectionId: string;
  grade: GradeEnum;
  value: number;
  // Embedded vehicle details
  year: string;
  make: string;
  model: string;
  stat1: number;
  stat2: number;
  stat3: number;
  vehicleImage: string;
}

/**
 * Response from GET /users/{userId}/cards/with-vehicles
 * Returns cards with full vehicle details for display
 */
export interface CardWithVehicleListResponse {
  cards: CardWithVehicle[];
  total: number;
  limit: number;
  offset: number;
}
