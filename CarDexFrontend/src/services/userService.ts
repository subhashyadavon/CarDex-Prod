/**
 * User Service
 * Handles user profile operations and rewards
 */

import apiClient from '../api/apiClient';
import { API_CONFIG } from '../config/api.config';
import { User, Reward, CollectionProgressResponse } from '../types/types';

// TEMPORARY: Import mock data for collection progress
import mockCollectionProgress from '../data/mockCollectionProgress.json';

export interface UpdateProfileRequest {
  username?: string;
  email?: string;
}

export const userService = {
  /**
   * Get user profile by ID
   */
  getProfile: async (userId: number): Promise<User> => {
    const response = await apiClient.get<User>(
      API_CONFIG.ENDPOINTS.USERS.GET_PROFILE(userId)
    );
    return response.data;
  },

  /**
   * Update user profile
   */
  updateProfile: async (
    userId: number,
    updates: UpdateProfileRequest
  ): Promise<User> => {
    const response = await apiClient.put<User>(
      API_CONFIG.ENDPOINTS.USERS.UPDATE_PROFILE(userId),
      updates
    );
    return response.data;
  },

  /**
   * Get user's rewards
   */
  getRewards: async (userId: number): Promise<Reward[]> => {
    const response = await apiClient.get<Reward[]>(
      API_CONFIG.ENDPOINTS.USERS.GET_REWARDS(userId)
    );
    return response.data;
  },

  /**
   * Get user's collection progress
   * Shows only collections where user owns at least 1 card
   * 
   * BACKEND NOT READY: Currently using mock data
   * TODO: Swap to API call when backend implements endpoint
   * 
   * TO SWITCH TO REAL API (when backend is ready):
   * 1. Uncomment the API call below
   * 2. Remove the mock implementation
   * 3. Delete mockCollectionProgress.json import at top
   */
  getCollectionProgress: async (userId: number): Promise<CollectionProgressResponse> => {
    // TEMPORARY: Return mock data
    // When backend is ready, uncomment below and remove mock return:
    /*
    const response = await apiClient.get<CollectionProgressResponse>(
      API_CONFIG.ENDPOINTS.USERS.GET_COLLECTION_PROGRESS(userId)
    );
    return response.data;
    */
    
    // MOCK IMPLEMENTATION (remove when backend ready)
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(mockCollectionProgress as CollectionProgressResponse);
      }, 500); // Simulate network delay
    });
  },
};
