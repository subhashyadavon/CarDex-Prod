/**
 * Collection Service
 * Handles user collection operations
 */

import apiClient from '../api/apiClient';
import { API_CONFIG } from '../config/api.config';
import { Collection } from '../types/types';

export const collectionService = {
  /**
   * Get user's collection
   */
  getCollection: async (userId: number): Promise<Collection> => {
    const response = await apiClient.get<Collection>(
      API_CONFIG.ENDPOINTS.COLLECTIONS.GET_COLLECTION(userId)
    );
    return response.data;
  },

  /**
   * Update user's collection
   */
  updateCollection: async (
    userId: number,
    collection: Collection
  ): Promise<Collection> => {
    const response = await apiClient.put<Collection>(
      API_CONFIG.ENDPOINTS.COLLECTIONS.UPDATE_COLLECTION(userId),
      collection
    );
    return response.data;
  },
};
