/**
 * API Client
 * 
 * This is the central HTTP client for all API requests in the application.
 * It's built on Axios and provides:
 * 1. Automatic JWT token injection (so you never have to manually add auth headers)
 * 2. Automatic logout when tokens expire (401 errors)
 * 3. Centralized error handling
 * 
 * HOW IT WORKS:
 * - When you call any service (e.g., cardService.getCards()), it uses this client
 * - Before sending the request, it grabs the token from localStorage and adds it
 * - If the backend returns 401 (unauthorized), it automatically logs the user out
 * - All other errors are passed back to your code to handle
 * 
 * WHY WE DO THIS:
 * - DRY Principle: Write auth logic once, not in every API call
 * - Security: Centralized token management reduces mistakes
 * - User Experience: Automatic logout prevents confusing error states
 */

import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { API_CONFIG } from '../config/api.config';

// Create axios instance with base configuration
// This is like creating a customized version of axios with our defaults
export const apiClient = axios.create({
  baseURL: API_CONFIG.BASE_URL,       // ← connects to your backend (http://localhost:5090)
  timeout: API_CONFIG.TIMEOUT,        // ← 10 seconds default
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * REQUEST INTERCEPTOR
 * 
 * This runs BEFORE every API request is sent to the backend.
 * Think of it like a security checkpoint that stamps your passport before you travel.
 * 
 * WHAT IT DOES:
 * 1. Checks localStorage for an authentication token
 * 2. If found, adds it to the request headers as "Authorization: Bearer <token>"
 * 3. The backend then knows who is making the request
 * 
 * WHY WE DO THIS:
 * - Without this, you'd have to manually add the token to EVERY API call
 * - Centralizing it here means if the auth method changes (e.g., to refresh tokens),
 *   you only update this one place, not hundreds of API calls
 */
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Get the stored auth token (set during login in AuthContext)
    const token = localStorage.getItem('authToken');
    
    // If we have a token, add it to the Authorization header
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    // Return the modified config so the request can proceed
    return config;
  },
  (error: AxiosError) => {
    // If the interceptor itself fails (rare), reject the request
    return Promise.reject(error);
  }
);

/**
 * RESPONSE INTERCEPTOR
 * 
 * This runs AFTER every API response comes back from the backend.
 * Think of it like customs when you return from travel - checking for problems.
 * 
 * WHAT IT DOES:
 * 1. If response is successful (200-299), pass it through unchanged
 * 2. If response is 401 Unauthorized, automatically log the user out
 * 3. If there's no response (network error), log a helpful message
 * 
 * WHY WE DO THIS:
 * - 401 means the token expired or is invalid - user needs to log in again
 * - Rather than handling this in every component, we handle it once here
 * - This prevents weird states where the user is "logged in" but can't do anything
 */
apiClient.interceptors.response.use(
  (response) => {
    // Success! Just pass the response through to the caller
    return response;
  },
  (error: AxiosError) => {
    // Handle 401 Unauthorized - token expired or invalid
    // This happens when the token is old or the user was logged out server-side
    if (error.response?.status === 401) {
      // Clear all auth data from localStorage
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
    }

    // Handle network errors (no response from server at all)
    // This could mean the backend is down or the user's internet is disconnected
    if (!error.response) {
      console.error('Network error - please check your connection');
    }

    // Pass the error back to the caller so they can handle it
    // (e.g., show an error message to the user)
    return Promise.reject(error);
  }
);

export default apiClient;
