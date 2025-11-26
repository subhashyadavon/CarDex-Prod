using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Defines operations related to user profiles, inventories, trades, and rewards.
    /// </summary>
    /// <remarks>
    /// This interface abstracts all user-related functionality for both mock and production implementations.
    /// It provides access to user data, collections, packs, trades, and reward tracking.
    /// </remarks>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves public profile information for a given user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to retrieve.</param>
        /// <returns>
        /// A <see cref="UserPublicResponse"/> containing the user's public profile data.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the specified user cannot be found.</exception>
        Task<UserPublicResponse> GetUserProfile(Guid userId);

        /// <summary>
        /// Updates the profile of an existing user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="request">The update request containing new profile data such as username or password.</param>
        /// <returns>
        /// An updated <see cref="UserResponse"/> reflecting the new user information.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user cannot be found.</exception>
        Task<UserResponse> UpdateUserProfile(Guid userId, UserUpdateRequest request);

        /// <summary>
        /// Retrieves all cards owned by a specific user with optional filtering and pagination.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose cards to retrieve.</param>
        /// <param name="collectionId">Optional filter to return cards from a specific collection.</param>
        /// <param name="grade">Optional filter to return only cards of a specific grade (FACTORY, LIMITED_RUN, NISMO).</param>
        /// <param name="limit">The number of results per page (default 50).</param>
        /// <param name="offset">The number of results to skip for pagination (default 0).</param>
        /// <returns>
        /// A <see cref="UserCardListResponse"/> containing a filtered and paginated list of the user’s cards.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        Task<UserCardListResponse> GetUserCards(Guid userId, Guid? collectionId, string? grade, int limit, int offset);

        /// <summary>
        /// Retrieves all unopened packs owned by a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose packs to retrieve.</param>
        /// <param name="collectionId">Optional filter to return packs from a specific collection.</param>
        /// <returns>
        /// A <see cref="UserPackListResponse"/> containing the user's unopened packs.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        Task<UserPackListResponse> GetUserPacks(Guid userId, Guid? collectionId);

        /// <summary>
        /// Retrieves all open trades created by a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose trades to retrieve.</param>
        /// <param name="type">Optional filter for trade type (FOR_CARD or FOR_PRICE).</param>
        /// <returns>
        /// A <see cref="UserTradeListResponse"/> containing the user's active trades.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        Task<UserTradeListResponse> GetUserTrades(Guid userId, string? type);

        /// <summary>
        /// Retrieves the trade history for a user, including completed trades where they were the buyer or seller.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose trade history to retrieve.</param>
        /// <param name="role">Filter by user role in trade (seller, buyer, or all).</param>
        /// <param name="limit">The number of results per page (default 50).</param>
        /// <param name="offset">The number of results to skip for pagination (default 0).</param>
        /// <returns>
        /// A <see cref="UserTradeHistoryListResponse"/> containing completed trade entries.
        /// </returns>
        Task<UserTradeHistoryListResponse> GetUserTradeHistory(Guid userId, string role, int limit, int offset);

        /// <summary>
        /// Retrieves the user's pending or claimed rewards.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose rewards to retrieve.</param>
        /// <param name="claimed">If true, includes claimed rewards; otherwise returns only unclaimed ones.</param>
        /// <returns>
        /// A <see cref="UserRewardListResponse"/> containing the user's rewards.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        Task<UserRewardListResponse> GetUserRewards(Guid userId, bool claimed);

        /// <summary>
        /// Retrieves all cards owned by a user with full vehicle details embedded.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose cards to retrieve.</param>
        /// <param name="collectionId">Optional filter to return cards from a specific collection.</param>
        /// <param name="grade">Optional filter to return only cards of a specific grade.</param>
        /// <param name="limit">The number of results per page (default 50).</param>
        /// <param name="offset">The number of results to skip for pagination (default 0).</param>
        /// <returns>
        /// A <see cref="UserCardWithVehicleListResponse"/> containing cards with embedded vehicle details.
        /// </returns>
        /// <remarks>
        /// This method joins card data with vehicle information to provide all details needed
        /// for card display without requiring additional API calls. More efficient than separate
        /// calls to get cards and then vehicles individually.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        Task<UserCardWithVehicleListResponse> GetUserCardsWithVehicles(Guid userId, Guid? collectionId, string? grade, int limit, int offset);

        /// <summary>
        /// Retrieves collection completion progress for a user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// A <see cref="CollectionProgressResponse"/> containing progress data for all collections
        /// where the user owns at least one card.
        /// </returns>
        /// <remarks>
        /// Shows how many unique vehicles the user owns from each collection and calculates
        /// completion percentage. Only includes collections with at least one owned card.
        /// </remarks>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        Task<CollectionProgressResponse> GetCollectionProgress(Guid userId);
    }
}
