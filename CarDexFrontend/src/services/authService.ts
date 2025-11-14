/**
 * Auth Service - Authentication API Calls
 * 
 * This service handles all HTTP requests related to authentication.
 * 
 * WHAT IS A SERVICE?
 * A service is a collection of functions that talk to the backend API.
 * Each function represents one API endpoint.
 * 
 * WHY SEPARATE SERVICES FROM CONTEXT?
 * - Separation of Concerns: Services handle HTTP, Context handles state
 * - Reusability: Services can be used outside of Context if needed
 * - Testability: Easy to mock services for testing
 * - Clarity: Each file has one clear job
 * 
 * PATTERN FOR ALL SERVICES:
 * 1. Import apiClient (handles auth headers, errors, etc.)
 * 2. Import API_CONFIG (endpoint URLs)
 * 3. Define TypeScript interfaces for requests/responses
 * 4. Export service object with async functions
 * 5. Each function: call API, return data
 * 
 * HOW TO USE:
 * - Usually called from Context (e.g., AuthContext calls authService.login)
 * - Can also be called directly in components if needed
 * - Always use try-catch when calling (async functions can throw errors)
 */

import apiClient from '../api/apiClient';
import { API_CONFIG } from '../config/api.config';
import { User } from '../types/types';

/**
 * LoginRequest: Data needed to log in
 * 
 * WHY WE DEFINE THIS?
 * - TypeScript will check that you include both email AND password
 * - Auto-complete in VS Code will suggest these fields
 * - Backend expects exactly these fields
 */
export interface LoginRequest {
  username: string;
  password: string;
}

/**
 * RegisterRequest: Data needed to create account
 * 
 * Similar to LoginRequest, but includes username
 */
export interface RegisterRequest {
  username: string;
  password: string;
}

/**
 * AuthResponse: What the backend returns after login/register
 * 
 * Backend sends back:
 * - user: Full user object with all their data
 * - accessToken: JWT token for authenticating future requests
 * - tokenType: Type of token (usually "Bearer")
 * - expiresIn: Token expiration time in seconds
 */
export interface AuthResponse {
  user: User;
  accessToken: string;
  tokenType: string;
  expiresIn: number;
}

/**
 * authService: Collection of authentication API functions
 * 
 * We export this as an object so you can call:
 *   authService.login({ email: '...', password: '...' })
 * 
 * All functions are async because they make network requests
 */
export const authService = {
  /**
   * login: Authenticate user with email and password
   * 
   * FLOW:
   * 1. Receives credentials from caller (email, password)
   * 2. Makes POST request to backend /api/auth/login
   * 3. Backend validates credentials against database
   * 4. If valid, backend returns user object + JWT token
   * 5. If invalid, backend returns 401 error (caught by apiClient interceptor)
   * 
   * WHAT HAPPENS AFTER:
   * - Caller (usually AuthContext) saves user and token to state
   * - Token is added to localStorage for session persistence
   * - Future API requests automatically include this token (via apiClient)
   * 
   * ERROR HANDLING:
   * - Network errors: Thrown by axios, caught by caller
   * - Wrong credentials: Backend returns 401, thrown as error
   * - Caller should wrap in try-catch and show error message to user
   * 
   * @param credentials - Object with email and password
   * @returns Promise<AuthResponse> - User object and JWT token
   * @throws Error if login fails (wrong credentials, network error, etc.)
   */
  login: async (credentials: LoginRequest): Promise<AuthResponse> => {
    // apiClient.post<AuthResponse> means:
    // - Make a POST request
    // - Expect response data to match AuthResponse type
    // - TypeScript will check that response.data has user and token

    const response = await apiClient.post<AuthResponse>(
      API_CONFIG.ENDPOINTS.AUTH.LOGIN, // URL: /api/auth/login
      credentials                      // Body: { email, password }
    );
    
    // Return just the data (response.data), not the whole axios response object
    // response.data is { user: {...}, token: '...' }
    return response.data;
  },

  /**
   * register: Create new user account
   * 
   * FLOW:
   * Similar to login, but creates account first
   * 1. Makes POST request with username, email, password
   * 2. Backend creates user in database
   * 3. Backend returns new user object + token (auto-logs them in)
   * 
   * ERROR HANDLING:
   * - Email already exists: Backend returns 409 Conflict
   * - Invalid email format: Backend returns 400 Bad Request
   * - Caller should catch and show appropriate error message
   * 
   * @param userData - Object with username, email, password
   * @returns Promise<AuthResponse> - New user object and JWT token
   * @throws Error if registration fails
   */
  register: async (userData: RegisterRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>(
      API_CONFIG.ENDPOINTS.AUTH.REGISTER, // URL: /api/auth/register
      userData                            // Body: { username, email, password }
    );
    return response.data;
  },

  /**
   * logout: End user session
   * 
   * FLOW:
   * 1. Makes POST request to backend /api/auth/logout
   * 2. Backend invalidates the token (adds to blacklist)
   * 3. Caller (AuthContext) clears local state and localStorage
   * 
   * WHY CALL BACKEND FOR LOGOUT?
   * - Security: Prevents token reuse even if someone steals it
   * - Backend can track active sessions
   * - Good practice for audit trails
   * 
   * NOTE: Returns void (nothing) because logout just needs to succeed
   * 
   * @returns Promise<void>
   * @throws Error if logout call fails (usually ignored since we logout locally anyway)
   */
  logout: async (): Promise<void> => {
    // No response data needed, just need the request to succeed
    await apiClient.post(API_CONFIG.ENDPOINTS.AUTH.LOGOUT);
  },
};
