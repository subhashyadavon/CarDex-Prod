using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Mock implementation of <see cref="IUserService"/> for development and parallel frontend integration.
    /// </summary>
    /// <remarks>
    /// Simulates user accounts, owned cards, packs, trades, and rewards using in-memory collections.
    /// Provides predictable data for frontend development before database integration.
    /// </remarks>
    public class MockUserService : IUserService
    {
        /// <summary>Static list of mock users.</summary>
        private readonly List<UserResponse> _users = new();

        /// <summary>Simulated per-user inventory of cards.</summary>
        private readonly Dictionary<Guid, List<UserCardResponse>> _cards = new();

        /// <summary>Simulated per-user inventory of unopened packs.</summary>
        private readonly Dictionary<Guid, List<UserPackResponse>> _packs = new();

        /// <summary>Simulated per-user list of open trades.</summary>
        private readonly Dictionary<Guid, List<UserTradeResponse>> _trades = new();

        /// <summary>Simulated per-user rewards such as currency or packs.</summary>
        private readonly Dictionary<Guid, List<UserRewardResponse>> _rewards = new();

        /// <summary>
        /// Initializes the mock service with one static user and sample inventory data.
        /// </summary>
        public MockUserService()
        {
            // Create a fake, fixed user for testing
            var userId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
            var now = DateTime.UtcNow;

            _users.Add(new UserResponse
            {
                Id = userId,
                Username = "spellmai123",
                Currency = 50000,
                CreatedAt = now,
                UpdatedAt = now
            });

            // Simulate user-owned cards
            _cards[userId] = new List<UserCardResponse>
            {
                new() { Id = Guid.NewGuid(), VehicleId = Guid.NewGuid(), CollectionId = Guid.NewGuid(), Grade = "FACTORY", Value = 35000 },
                new() { Id = Guid.NewGuid(), VehicleId = Guid.NewGuid(), CollectionId = Guid.NewGuid(), Grade = "NISMO", Value = 28000 }
            };

            // Simulate user-owned packs
            _packs[userId] = new List<UserPackResponse>
            {
                new() { Id = Guid.NewGuid(), CollectionId = Guid.NewGuid(), Value = 20000 }
            };

            // Simulate active trades
            _trades[userId] = new List<UserTradeResponse>
            {
                new() { Id = Guid.NewGuid(), Type = "FOR_PRICE", CardId = _cards[userId][0].Id, Price = 15000 }
            };

            // Simulate reward list
            _rewards[userId] = new List<UserRewardResponse>
            {
                new() { Id = Guid.NewGuid(), Type = "PACK", ItemId = Guid.NewGuid(), CreatedAt = now }
            };
        }

        /// <summary>
        /// Retrieves a user's public profile by ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A <see cref="UserPublicResponse"/> containing username and creation date.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        public Task<UserPublicResponse> GetUserProfile(Guid userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return Task.FromResult(new UserPublicResponse
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Updates an existing user’s profile information.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="request">The update request (e.g. username).</param>
        /// <returns>The updated <see cref="UserResponse"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        public Task<UserResponse> UpdateUserProfile(Guid userId, UserUpdateRequest request)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!string.IsNullOrWhiteSpace(request.Username))
                user.Username = request.Username;

            user.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(user);
        }

        /// <summary>
        /// Retrieves a list of the user's owned cards with optional filtering.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="collectionId">Optional collection filter.</param>
        /// <param name="grade">Optional grade filter.</param>
        /// <param name="limit">Number of results to return.</param>
        /// <param name="offset">Number of results to skip.</param>
        /// <returns>A paginated <see cref="UserCardListResponse"/>.</returns>
        public Task<UserCardListResponse> GetUserCards(Guid userId, Guid? collectionId, string? grade, int limit, int offset)
        {
            if (!_cards.TryGetValue(userId, out var cards))
                throw new KeyNotFoundException("User not found");

            var result = cards.AsEnumerable();
            if (collectionId.HasValue) result = result.Where(c => c.CollectionId == collectionId);
            if (!string.IsNullOrEmpty(grade)) result = result.Where(c => c.Grade == grade);

            var list = result.Skip(offset).Take(limit);
            return Task.FromResult(new UserCardListResponse
            {
                Cards = list,
                Total = result.Count(),
                Limit = limit,
                Offset = offset
            });
        }

        /// <summary>
        /// Retrieves the list of packs owned by the user, optionally filtered by collection.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="collectionId">Optional collection filter.</param>
        /// <returns>A <see cref="UserPackListResponse"/> of all owned packs.</returns>
        public Task<UserPackListResponse> GetUserPacks(Guid userId, Guid? collectionId)
        {
            if (!_packs.TryGetValue(userId, out var packs))
                throw new KeyNotFoundException("User not found");

            var result = packs.AsEnumerable();
            if (collectionId.HasValue)
                result = result.Where(p => p.CollectionId == collectionId);

            return Task.FromResult(new UserPackListResponse
            {
                Packs = result,
                Total = result.Count()
            });
        }

        /// <summary>
        /// Retrieves all open trades owned by a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="type">Optional filter for trade type.</param>
        /// <returns>A <see cref="UserTradeListResponse"/> representing current open trades.</returns>
        public Task<UserTradeListResponse> GetUserTrades(Guid userId, string? type)
        {
            if (!_trades.TryGetValue(userId, out var trades))
                throw new KeyNotFoundException("User not found");

            var result = string.IsNullOrEmpty(type) ? trades : trades.Where(t => t.Type == type);
            return Task.FromResult(new UserTradeListResponse
            {
                Trades = result,
                Total = result.Count()
            });
        }

        /// <summary>
        /// Retrieves a paginated list of completed trades for a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="role">Specifies which role to include ("buyer", "seller", or "all").</param>
        /// <param name="limit">Number of results to return.</param>
        /// <param name="offset">Number of results to skip.</param>
        /// <returns>A <see cref="UserTradeHistoryListResponse"/> containing trade history records.</returns>
        public Task<UserTradeHistoryListResponse> GetUserTradeHistory(Guid userId, string role, int limit, int offset)
        {
            // Mock implementation reuses a static trade example for simplicity
            var trades = new List<UserTradeHistoryResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Type = "FOR_PRICE",
                    SellerUserId = userId,
                    SellerCardId = Guid.NewGuid(),
                    BuyerUserId = Guid.NewGuid(),
                    BuyerCardId = null,
                    Price = 15000,
                    ExecutedDate = DateTime.UtcNow
                }
            };

            return Task.FromResult(new UserTradeHistoryListResponse
            {
                Trades = trades.Skip(offset).Take(limit),
                Total = trades.Count,
                Limit = limit,
                Offset = offset
            });
        }

        /// <summary>
        /// Retrieves all rewards associated with a user, optionally filtered by claim status.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="claimed">If true, returns only claimed rewards; otherwise returns unclaimed.</param>
        /// <returns>A <see cref="UserRewardListResponse"/> of rewards for the user.</returns>
        public Task<UserRewardListResponse> GetUserRewards(Guid userId, bool claimed)
        {
            if (!_rewards.TryGetValue(userId, out var rewards))
                throw new KeyNotFoundException("User not found");

            var filtered = rewards.Where(r => claimed || r.ClaimedAt == null);
            return Task.FromResult(new UserRewardListResponse
            {
                Rewards = filtered,
                Total = filtered.Count()
            });
        }
    }
}
