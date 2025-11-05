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
  BASE_URL: process.env.REACT_APP_API_URL || 'http://localhost:5083',
  
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
      LOGIN: '/api/auth/login',           // POST: Log in with email/password
      REGISTER: '/api/auth/register',     // POST: Create new account
      LOGOUT: '/api/auth/logout',         // POST: End session
    },
    
    // Card management endpoints
    CARDS: {
      GET_ALL: '/api/cards',                                    // GET: Fetch all cards
      GET_BY_ID: (id: number) => `/api/cards/${id}`,           // GET: Fetch specific card
      GET_USER_CARDS: (userId: number) => `/api/cards/user/${userId}`, // GET: User's cards
    },
    
    // Pack management endpoints
    PACKS: {
      GET_ALL: '/api/packs',                                    // GET: Fetch all packs
      GET_BY_ID: (id: number) => `/api/packs/${id}`,           // GET: Fetch specific pack
      OPEN_PACK: (packId: number) => `/api/packs/${packId}/open`, // POST: Open pack, get cards
    },
    
    // Trade management endpoints
    TRADES: {
      GET_ALL: '/api/trades',                                   // GET: All open trades
      GET_BY_ID: (id: number) => `/api/trades/${id}`,          // GET: Specific trade
      GET_USER_TRADES: (userId: number) => `/api/trades/user/${userId}`, // GET: User's trades
      CREATE: '/api/trades',                                    // POST: Create new trade
      ACCEPT: (tradeId: number) => `/api/trades/${tradeId}/accept`,  // POST: Accept trade
      CANCEL: (tradeId: number) => `/api/trades/${tradeId}/cancel`,  // DELETE: Cancel trade
    },
    
    // User profile endpoints
    USERS: {
      GET_PROFILE: (userId: number) => `/api/users/${userId}`,         // GET: User profile
      UPDATE_PROFILE: (userId: number) => `/api/users/${userId}`,      // PUT: Update profile
      GET_REWARDS: (userId: number) => `/api/users/${userId}/rewards`, // GET: User rewards
    },
    
    // Collection management endpoints
    COLLECTIONS: {
      GET_COLLECTION: (userId: number) => `/api/collections/${userId}`,    // GET: User collection
      UPDATE_COLLECTION: (userId: number) => `/api/collections/${userId}`, // PUT: Update collection
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
