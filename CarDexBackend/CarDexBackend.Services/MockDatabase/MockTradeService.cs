using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Mock implementation of <see cref="ITradeService"/> for simulating trading functionality during development.
    /// </summary>
    /// <remarks>
    /// This mock manages in-memory trade listings for testing card-for-card and card-for-currency transactions.
    /// It simulates the trade lifecycle: creation, browsing, execution, and history tracking.
    /// </remarks>
    public class MockTradeService : ITradeService
    {
        /// <summary>
        /// List of all currently open trades (active listings).
        /// </summary>
        private readonly List<TradeDetailedResponse> _openTrades = new();

        /// <summary>
        /// List of all completed (executed) trades.
        /// </summary>
        private readonly List<CompletedTradeResponse> _completedTrades = new();

        /// <summary>
        /// Initializes the mock trade service with a default open trade for demonstration.
        /// </summary>
        public MockTradeService()
        {
            _openTrades.Add(new TradeDetailedResponse
            {
                Id = Guid.NewGuid(),
                Type = "FOR_PRICE",
                UserId = Guid.NewGuid(),
                Username = "DaleEarnhardt",
                CardId = Guid.NewGuid(),
                Price = 15000,
                CreatedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Retrieves a paginated list of open trades with optional filtering.
        /// </summary>
        /// <param name="type">Optional trade type filter (FOR_CARD or FOR_PRICE).</param>
        /// <param name="collectionId">Optional filter by collection (unused in mock).</param>
        /// <param name="grade">Optional card grade filter (unused in mock).</param>
        /// <param name="minPrice">Optional minimum price filter.</param>
        /// <param name="maxPrice">Optional maximum price filter.</param>
        /// <param name="vehicleId">Optional vehicle filter (unused in mock).</param>
        /// <param name="wantCardId">Optional filter for desired card ID (unused in mock).</param>
        /// <param name="sortBy">Optional sort order (unused in mock).</param>
        /// <param name="limit">Maximum number of results to return.</param>
        /// <param name="offset">Number of results to skip for pagination.</param>
        /// <returns>A <see cref="TradeListResponse"/> containing open trades.</returns>
        public Task<TradeListResponse> GetOpenTrades(string? type, Guid? collectionId, string? grade, int? minPrice,
            int? maxPrice, Guid? vehicleId, Guid? wantCardId, string? sortBy, int limit, int offset)
        {
            var result = new TradeListResponse
            {
                Trades = _openTrades.Skip(offset).Take(limit),
                Total = _openTrades.Count,
                Limit = limit,
                Offset = offset
            };
            return Task.FromResult(result);
        }

        /// <summary>
        /// Retrieves detailed information about a specific open trade.
        /// </summary>
        /// <param name="tradeId">The unique trade ID.</param>
        /// <returns>A <see cref="TradeDetailedResponse"/> representing the open trade.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the trade does not exist.</exception>
        public Task<TradeDetailedResponse> GetOpenTradeById(Guid tradeId)
        {
            var trade = _openTrades.FirstOrDefault(t => t.Id == tradeId);
            if (trade == null)
                throw new KeyNotFoundException("Trade not found.");
            return Task.FromResult(trade);
        }

        /// <summary>
        /// Creates a new trade listing, either card-for-card or card-for-currency.
        /// </summary>
        /// <param name="request">The trade creation request data.</param>
        /// <returns>A <see cref="TradeResponse"/> representing the newly created trade.</returns>
        /// <exception cref="ArgumentException">Thrown if the trade type is invalid or missing.</exception>
        public Task<TradeResponse> CreateTrade(TradeCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Type))
                throw new ArgumentException("Invalid trade type.");

            var newTrade = new TradeResponse
            {
                Id = Guid.NewGuid(),
                Type = request.Type,
                UserId = Guid.NewGuid(),
                Username = "trade_king",
                CardId = request.CardId,
                Price = request.Price,
                WantCardId = request.WantCardId,
                CreatedAt = DateTime.UtcNow
            };

            _openTrades.Add(new TradeDetailedResponse
            {
                Id = newTrade.Id,
                Type = newTrade.Type,
                UserId = newTrade.UserId,
                Username = newTrade.Username,
                CardId = newTrade.CardId,
                Price = newTrade.Price,
                WantCardId = newTrade.WantCardId,
                CreatedAt = newTrade.CreatedAt
            });

            return Task.FromResult(newTrade);
        }

        /// <summary>
        /// Cancels (deletes) an existing open trade.
        /// </summary>
        /// <param name="tradeId">The unique trade ID.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the trade does not exist.</exception>
        public Task DeleteTrade(Guid tradeId)
        {
            var trade = _openTrades.FirstOrDefault(t => t.Id == tradeId);
            if (trade == null)
                throw new KeyNotFoundException("Trade not found.");
            _openTrades.Remove(trade);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Executes a trade transaction, completing the exchange and generating rewards.
        /// </summary>
        /// <param name="tradeId">The ID of the trade to execute.</param>
        /// <param name="request">Optional buyer request (used for FOR_CARD trades).</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description>The completed trade record.</description></item>
        /// <item><description>The seller's reward (currency or pack).</description></item>
        /// <item><description>The buyer's reward (usually the traded card).</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown if the trade ID is invalid.</exception>
        public Task<(CompletedTradeResponse CompletedTrade, RewardResponse SellerReward, RewardResponse BuyerReward)> ExecuteTrade(Guid tradeId, TradeExecuteRequest? request)
        {
            var trade = _openTrades.FirstOrDefault(t => t.Id == tradeId);
            if (trade == null)
                throw new KeyNotFoundException("Trade not found.");

            _openTrades.Remove(trade);

            // Build a completed trade record
            var completed = new CompletedTradeResponse
            {
                Id = trade.Id,
                Type = trade.Type,
                SellerUserId = trade.UserId,
                SellerUsername = trade.Username,
                SellerCardId = trade.CardId,
                BuyerUserId = Guid.NewGuid(),
                BuyerUsername = "super_collector",
                BuyerCardId = request?.BuyerCardId,
                Price = trade.Price ?? 0,
                ExecutedDate = DateTime.UtcNow
            };
            _completedTrades.Add(completed);

            // Mock reward creation
            var sellerReward = new RewardResponse
            {
                Id = Guid.NewGuid(),
                UserId = completed.SellerUserId,
                Type = "CURRENCY_FROM_TRADE",
                Amount = completed.Price,
                CreatedAt = DateTime.UtcNow
            };

            var buyerReward = new RewardResponse
            {
                Id = Guid.NewGuid(),
                UserId = completed.BuyerUserId,
                Type = "CARD_FROM_TRADE",
                ItemId = completed.SellerCardId,
                CreatedAt = DateTime.UtcNow
            };

            return Task.FromResult((completed, sellerReward, buyerReward));
        }

        /// <summary>
        /// Retrieves a paginated list of completed trades (trade history).
        /// </summary>
        /// <param name="userId">Optional user filter (matches buyer or seller).</param>
        /// <param name="limit">Number of records to return per page.</param>
        /// <param name="offset">Number of records to skip.</param>
        /// <returns>A <see cref="TradeHistoryResponse"/> containing completed trades.</returns>
        public Task<TradeHistoryResponse> GetTradeHistory(Guid? userId, int limit, int offset)
        {
            var trades = _completedTrades.AsEnumerable();
            if (userId.HasValue)
                trades = trades.Where(t => t.SellerUserId == userId || t.BuyerUserId == userId);

            var response = new TradeHistoryResponse
            {
                Trades = trades.Skip(offset).Take(limit),
                Total = trades.Count(),
                Limit = limit,
                Offset = offset
            };
            return Task.FromResult(response);
        }

        /// <summary>
        /// Retrieves a completed trade by ID.
        /// </summary>
        /// <param name="tradeId">The trade ID.</param>
        /// <returns>A <see cref="CompletedTradeResponse"/> representing the completed trade.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the specified trade does not exist.</exception>
        public Task<CompletedTradeResponse> GetCompletedTradeById(Guid tradeId)
        {
            var trade = _completedTrades.FirstOrDefault(t => t.Id == tradeId);
            if (trade == null)
                throw new KeyNotFoundException("Trade not found.");
            return Task.FromResult(trade);
        }
    }
}
