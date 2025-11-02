using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Production implementation of <see cref="IUserService"/> using Entity Framework Core and PostgreSQL.
    /// </summary>
    /// <remarks>
    /// Provides database-backed operations for user profiles, inventories, trades, and rewards.
    /// </remarks>
    public class UserService : IUserService
    {
        private readonly CarDexDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="context">The database context for accessing user data.</param>
        public UserService(CarDexDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a user's public profile by ID.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A <see cref="UserPublicResponse"/> containing username and creation date.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        public async Task<UserPublicResponse> GetUserProfile(Guid userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserPublicResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    CreatedAt = DateTime.UtcNow  // CreatedAt not in DB, using current time
                })
                .FirstOrDefaultAsync();

            if (user == null)
                throw new KeyNotFoundException("User not found");

            return user;
        }

        /// <summary>
        /// Updates an existing user's profile information.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="request">The update request (e.g. username).</param>
        /// <returns>The updated <see cref="UserResponse"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the user does not exist.</exception>
        public async Task<UserResponse> UpdateUserProfile(Guid userId, UserUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!string.IsNullOrWhiteSpace(request.Username))
                user.Username = request.Username;

            await _context.SaveChangesAsync();

            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Currency = user.Currency,
                CreatedAt = DateTime.UtcNow,  // Not in DB, using current time
                UpdatedAt = DateTime.UtcNow   // Not in DB, using current time
            };
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
        public async Task<UserCardListResponse> GetUserCards(Guid userId, Guid? collectionId, string? grade, int limit, int offset)
        {
            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new KeyNotFoundException("User not found");

            var query = _context.Cards
                .Where(c => c.UserId == userId);

            if (collectionId.HasValue)
                query = query.Where(c => c.CollectionId == collectionId);

            if (!string.IsNullOrEmpty(grade))
                query = query.Where(c => c.Grade.ToString() == grade);

            var total = await query.CountAsync();

            var cards = await query
                .Skip(offset)
                .Take(limit)
                .Select(c => new UserCardResponse
                {
                    Id = c.Id,
                    VehicleId = c.VehicleId,
                    CollectionId = c.CollectionId,
                    Grade = c.Grade.ToString(),  // Will be "FACTORY", "LIMITED_RUN", or "NISMO"
                    Value = c.Value
                })
                .ToListAsync();

            return new UserCardListResponse
            {
                Cards = cards,
                Total = total,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Retrieves the list of packs owned by the user, optionally filtered by collection.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="collectionId">Optional collection filter.</param>
        /// <returns>A <see cref="UserPackListResponse"/> of all owned packs.</returns>
        public async Task<UserPackListResponse> GetUserPacks(Guid userId, Guid? collectionId)
        {
            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new KeyNotFoundException("User not found");

            var query = _context.Packs
                .Where(p => p.UserId == userId);

            if (collectionId.HasValue)
                query = query.Where(p => p.CollectionId == collectionId);

            var packs = await query
                .Select(p => new UserPackResponse
                {
                    Id = p.Id,
                    CollectionId = p.CollectionId,
                    Value = p.Value
                })
                .ToListAsync();

            return new UserPackListResponse
            {
                Packs = packs,
                Total = packs.Count
            };
        }

        /// <summary>
        /// Retrieves all open trades owned by a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="type">Optional filter for trade type.</param>
        /// <returns>A <see cref="UserTradeListResponse"/> representing current open trades.</returns>
        public async Task<UserTradeListResponse> GetUserTrades(Guid userId, string? type)
        {
            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new KeyNotFoundException("User not found");

            var query = _context.OpenTrades
                .Where(t => t.UserId == userId);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.Type.ToString() == type);

            var trades = await query
                .Select(t => new UserTradeResponse
                {
                    Id = t.Id,
                    Type = t.Type.ToString(),
                    CardId = t.CardId,
                    Price = t.Price
                })
                .ToListAsync();

            return new UserTradeListResponse
            {
                Trades = trades,
                Total = trades.Count
            };
        }

        /// <summary>
        /// Retrieves a paginated list of completed trades for a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="role">Specifies which role to include ("buyer", "seller", or "all").</param>
        /// <param name="limit">Number of results to return.</param>
        /// <param name="offset">Number of results to skip.</param>
        /// <returns>A <see cref="UserTradeHistoryListResponse"/> containing trade history records.</returns>
        public async Task<UserTradeHistoryListResponse> GetUserTradeHistory(Guid userId, string role, int limit, int offset)
        {
            var query = _context.CompletedTrades.AsQueryable();

            if (role.ToLower() == "seller")
                query = query.Where(t => t.SellerUserId == userId);
            else if (role.ToLower() == "buyer")
                query = query.Where(t => t.BuyerUserId == userId);
            else
                query = query.Where(t => t.SellerUserId == userId || t.BuyerUserId == userId);

            var total = await query.CountAsync();

            var trades = await query
                .OrderByDescending(t => t.ExecutedDate)
                .Skip(offset)
                .Take(limit)
                .Select(t => new UserTradeHistoryResponse
                {
                    Id = t.Id,
                    Type = t.Type.ToString(),
                    SellerUserId = t.SellerUserId,
                    SellerCardId = t.SellerCardId,
                    BuyerUserId = t.BuyerUserId,
                    BuyerCardId = t.BuyerCardId,
                    Price = t.Price,
                    ExecutedDate = t.ExecutedDate  // Already DateTime, not nullable
                })
                .ToListAsync();

            return new UserTradeHistoryListResponse
            {
                Trades = trades,
                Total = total,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Retrieves all rewards associated with a user, optionally filtered by claim status.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="claimed">If true, returns only claimed rewards; otherwise returns unclaimed.</param>
        /// <returns>A <see cref="UserRewardListResponse"/> of rewards for the user.</returns>
        public async Task<UserRewardListResponse> GetUserRewards(Guid userId, bool claimed)
        {
            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new KeyNotFoundException("User not found");

            var query = _context.Rewards
                .Where(r => r.UserId == userId);

            if (claimed)
                query = query.Where(r => r.ClaimedAt != null);
            else
                query = query.Where(r => r.ClaimedAt == null);

            var rewards = await query
                .Select(r => new UserRewardResponse
                {
                    Id = r.Id,
                    Type = r.Type.ToString(),  // Will be "PACK", "CURRENCY", "CARD_FROM_TRADE", etc.
                    ItemId = r.ItemId,
                    CreatedAt = DateTime.UtcNow,  // Not in DB, using current time
                    ClaimedAt = r.ClaimedAt
                })
                .ToListAsync();

            return new UserRewardListResponse
            {
                Rewards = rewards,
                Total = rewards.Count
            };
        }
    }
}
