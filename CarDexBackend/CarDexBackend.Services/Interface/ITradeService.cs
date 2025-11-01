using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Defines operations for managing trades between users, including open listings,
    /// trade execution, and historical trade data.
    /// </summary>
    /// <remarks>
    /// This interface provides methods for browsing and creating open trades,
    /// executing trade transactions, and retrieving historical trade records.
    /// It abstracts the marketplace and transaction logic between players.
    /// </remarks>
    public interface ITradeService
    {
        /// <summary>
        /// Retrieves a paginated list of all open trades with optional filters and sorting.
        /// </summary>
        /// <param name="type">Optional filter for trade type (FOR_CARD or FOR_PRICE).</param>
        /// <param name="collectionId">Optional filter to return only trades from a specific collection.</param>
        /// <param name="grade">Optional filter to return only cards of a specific grade (FACTORY, LIMITED_RUN, NISMO).</param>
        /// <param name="minPrice">Optional minimum price filter (for FOR_PRICE trades).</param>
        /// <param name="maxPrice">Optional maximum price filter (for FOR_PRICE trades).</param>
        /// <param name="vehicleId">Optional filter for trades involving a specific vehicle.</param>
        /// <param name="wantCardId">Optional filter for trades requesting a specific card in exchange.</param>
        /// <param name="sortBy">Sort order (price_asc, price_desc, date_asc, date_desc, etc.).</param>
        /// <param name="limit">Maximum number of results per page (default 50).</param>
        /// <param name="offset">Number of results to skip for pagination (default 0).</param>
        /// <returns>
        /// A <see cref="TradeListResponse"/> containing a filtered and sorted list of open trades.
        /// </returns>
        Task<TradeListResponse> GetOpenTrades(
            string? type,
            Guid? collectionId,
            string? grade,
            int? minPrice,
            int? maxPrice,
            Guid? vehicleId,
            Guid? wantCardId,
            string? sortBy,
            int limit,
            int offset);

        /// <summary>
        /// Retrieves detailed information about a specific open trade.
        /// </summary>
        /// <param name="tradeId">The unique identifier of the open trade to retrieve.</param>
        /// <returns>
        /// A <see cref="TradeDetailedResponse"/> containing the trade details, including user, card, and price data.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the trade cannot be found.</exception>
        Task<TradeDetailedResponse> GetOpenTradeById(Guid tradeId);

        /// <summary>
        /// Creates a new trade listing.
        /// </summary>
        /// <param name="request">The trade creation request containing type, offered card, and price or desired card.</param>
        /// <returns>
        /// A <see cref="TradeResponse"/> representing the newly created trade listing.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the request is invalid or contains conflicting parameters.</exception>
        Task<TradeResponse> CreateTrade(TradeCreateRequest request);

        /// <summary>
        /// Deletes (cancels) an open trade listing.
        /// </summary>
        /// <param name="tradeId">The unique identifier of the trade to delete.</param>
        /// <returns>A completed task when deletion is successful.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the trade cannot be found.</exception>
        Task DeleteTrade(Guid tradeId);

        /// <summary>
        /// Executes a trade between two users, transferring cards or currency as required.
        /// </summary>
        /// <param name="tradeId">The unique identifier of the trade to execute.</param>
        /// <param name="request">
        /// Optional execution request containing the buyer’s offered card (required for FOR_CARD trades).
        /// </param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><see cref="CompletedTradeResponse"/> — details of the completed trade.</description></item>
        /// <item><description><see cref="RewardResponse"/> — reward for the seller (e.g., currency or pack).</description></item>
        /// <item><description><see cref="RewardResponse"/> — reward for the buyer (e.g., card from the seller).</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the trade cannot be found.</exception>
        Task<(CompletedTradeResponse CompletedTrade, RewardResponse SellerReward, RewardResponse BuyerReward)> ExecuteTrade(Guid tradeId, TradeExecuteRequest? request);

        /// <summary>
        /// Retrieves a paginated list of completed trades, optionally filtered by user.
        /// </summary>
        /// <param name="userId">Optional filter to return trades involving a specific user as buyer or seller.</param>
        /// <param name="limit">Maximum number of results per page (default 50).</param>
        /// <param name="offset">Number of results to skip for pagination (default 0).</param>
        /// <returns>
        /// A <see cref="TradeHistoryResponse"/> containing a list of completed trades.
        /// </returns>
        Task<TradeHistoryResponse> GetTradeHistory(Guid? userId, int limit, int offset);

        /// <summary>
        /// Retrieves detailed information about a specific completed trade.
        /// </summary>
        /// <param name="tradeId">The unique identifier of the completed trade to retrieve.</param>
        /// <returns>
        /// A <see cref="CompletedTradeResponse"/> containing detailed post-trade data.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the completed trade cannot be found.</exception>
        Task<CompletedTradeResponse> GetCompletedTradeById(Guid tradeId);
    }
}
