/**
 * User Service
 * Handles user profile operations and rewards
 */

import apiClient from '../api/apiClient';
import { API_CONFIG } from '../config/api.config';
import { User, Reward } from '../types/types';

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
};
