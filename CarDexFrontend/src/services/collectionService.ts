/**
 * Collection Service
 * Handles user collection operations
 */

import apiClient from '../api/apiClient';
import { API_CONFIG } from '../config/api.config';
import { Collection, CollectionDetailedResponse } from '../types/types';

interface CollectionListResponse {
  collections: Collection[];
}

export const collectionService = {
  /**
   * Get user's collection
   */
  getCollection: async (userId: string): Promise<Collection> => {
    const response = await apiClient.get<Collection>(
      API_CONFIG.ENDPOINTS.COLLECTIONS.GET_COLLECTION(userId)
    );
    return response.data;
  },

  /**
   * Update user's collection
   */
  updateCollection: async (
    userId: string,
    collection: Collection
  ): Promise<Collection> => {
    const response = await apiClient.put<Collection>(
      API_CONFIG.ENDPOINTS.COLLECTIONS.UPDATE_COLLECTION(userId),
      collection
    );
    return response.data;
  },

  // Get all collections
  getAllCollections: async (): Promise<Collection[]> => {
    const response = await apiClient.get<CollectionListResponse>(
      API_CONFIG.ENDPOINTS.COLLECTIONS.GET_ALL()
    );
    // unwrap to a plain array:
    return response.data.collections;
  },

  /**
   * Get a single collection by its GUID (detailed: with cards)
   */
  getCollectionById: async (
    collectionId: string
  ): Promise<CollectionDetailedResponse> => {
    const response = await apiClient.get<CollectionDetailedResponse>(
      API_CONFIG.ENDPOINTS.COLLECTIONS.GET_COLLECTION_BY_ID(collectionId)
    );
    return response.data;
  },
};
