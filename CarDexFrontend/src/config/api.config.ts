/**
 * API Configuration
 * 
 * This file is the single source of truth for all API endpoint URLs.
 * 
 * WHY WE DO THIS:
 * - If an endpoint URL changes, you only update it here, not in 50 different files
 * - Easy to switch between dev/staging/prod environments by changing BASE_URL
 * - Type-safe: Functions like GET_BY_ID(5) prevent typos like "/api/cards/undefined"
 * - Clear documentation: All available endpoints visible in one place
 * 
 * HOW TO USE:
 * - Import this in service files: import { API_CONFIG } from '../config/api.config'
 * - Use like: apiClient.get(API_CONFIG.ENDPOINTS.CARDS.GET_ALL)
 * - For dynamic URLs: API_CONFIG.ENDPOINTS.CARDS.GET_BY_ID(cardId)
 */

export const API_CONFIG = {
  /**
   * BASE_URL: The root URL of the backend API
   * - Loaded from .env file (REACT_APP_API_URL)
   * - Falls back to localhost:5083 if not set
   * - Change .env to switch between dev/prod without changing code
   */
  BASE_URL: process.env.REACT_APP_API_URL || "http://localhost/",

  /**
   * ENDPOINTS: All API endpoint paths organized by resource
   *
   * PATTERN:
   * - Static endpoints: Simple strings like '/api/cards'
   * - Dynamic endpoints: Functions that take IDs like (id) => `/api/cards/${id}`
   *
   * WHY FUNCTIONS?
   * - Ensures IDs are always included (won't forget to add them)
   * - TypeScript checks the parameter type (prevents passing strings as numbers)
   * - Clear and self-documenting: GET_BY_ID(5) is clearer than '/api/cards/' + id
   */
  ENDPOINTS: {
    // Authentication endpoints
    AUTH: {
      LOGIN: "auth/login", // POST: Log in with email/password
      REGISTER: "auth/register", // POST: Create new account
      LOGOUT: "auth/logout", // POST: End session
    },

    // Card management endpoints
    CARDS: {
      GET_ALL: "/cards", // GET: Fetch all cards
      GET_BY_ID: (id: string) => `/cards/${id}`, // GET: Fetch specific card
      GET_USER_CARDS: (userId: string) => `/cards/user/${userId}`, // GET: User's cards
    },

    // Pack management endpoints
    PACKS: {
      GET_ALL: "/packs", // GET: Fetch all packs
      GET_BY_ID: (id: string) => `/packs/${id}`, // GET: Fetch specific pack
      OPEN_PACK: (packId: string) => `/packs/${packId}/open`, // POST: Open pack, get cards
      PURCHASE: "/packs/purchase",
    },

    // Trade management endpoints
    TRADES: {
      GET_ALL: "/trades", // GET: All open trades
      GET_BY_ID: (id: string) => `/trades/${id}`, // GET: Specific trade
      GET_USER_TRADES: (userId: string) => `/trades/user/${userId}`, // GET: User's trades
      CREATE: "/trades", // POST: Create new trade
      ACCEPT: (tradeId: string) => `/trades/${tradeId}/accept`, // POST: Accept trade
      CANCEL: (tradeId: string) => `/trades/${tradeId}/cancel`, // DELETE: Cancel trade
    },

    // User profile endpoints
    USERS: {
      GET_PROFILE: (userId: string) => `/users/${userId}`, // GET: User profile
      UPDATE_PROFILE: (userId: string) => `/users/${userId}`, // PUT: Update profile
      GET_REWARDS: (userId: string) => `/users/${userId}/rewards`, // GET: User rewards
      GET_COLLECTION_PROGRESS: (userId: string) =>
        `/users/${userId}/collection-progress`, // GET: Collection progress
      GET_CARDS_WITH_VEHICLES: (userId: string) =>
        `/users/${userId}/cards/with-vehicles`, // GET: Cards with vehicle details
    },

    // Collection management endpoints
    COLLECTIONS: {
      GET_ALL: () => `/collections`,
      GET_COLLECTION: (userId: string) => `/collections/${userId}`, // GET: User collection
      UPDATE_COLLECTION: (userId: string) => `/collections/${userId}`, // PUT: Update collection
      GET_COLLECTION_BY_ID: (collectionId: string) =>
        `/collections/${collectionId}`,
    },
  },

  /**
   * TIMEOUT: Maximum time to wait for API response (milliseconds)
   * - Prevents requests from hanging forever if backend is slow/down
   * - After 10 seconds, axios will cancel the request and throw an error
   * - Your code can then show "Request timed out" to the user
   */
  TIMEOUT: 10000, // 10 seconds
};
