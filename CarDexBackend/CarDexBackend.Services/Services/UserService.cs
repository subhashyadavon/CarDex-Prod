using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Repository.Interfaces;
using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;
using Microsoft.Extensions.Localization;
using CarDexBackend.Services.Resources;
using CarDexBackend.Domain.Entities;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Production implementation of <see cref="IUserService"/> using Repositories.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IStringLocalizer<SharedResources> _sr;
        private readonly IUserRepository _userRepo;
        private readonly ICardRepository _cardRepo;
        private readonly IPackRepository _packRepo;
        private readonly IOpenTradeRepository _openTradeRepo;
        private readonly ICompletedTradeRepository _tradeRepo;
        private readonly IRewardRepository _rewardRepo;
        private readonly IRepository<Vehicle> _vehicleRepo;

        public UserService(
            IUserRepository userRepo,
            ICardRepository cardRepo,
            IPackRepository packRepo,
            IOpenTradeRepository openTradeRepo,
            ICompletedTradeRepository tradeRepo,
            IRewardRepository rewardRepo,
            IRepository<Vehicle> vehicleRepo,
            IStringLocalizer<SharedResources> sr)
        {
            _userRepo = userRepo;
            _cardRepo = cardRepo;
            _packRepo = packRepo;
            _openTradeRepo = openTradeRepo;
            _tradeRepo = tradeRepo;
            _rewardRepo = rewardRepo;
            _vehicleRepo = vehicleRepo;
            _sr = sr;
        }

        /// <summary>
        /// Retrieves the profile information for a specific user.
        /// </summary>
        public async Task<UserPublicResponse> GetUserProfile(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            return new UserPublicResponse
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            };
        }

        /// <summary>
        /// Updates the profile information for a user.
        /// </summary>
        public async Task<UserResponse> UpdateUserProfile(Guid userId, UserUpdateRequest request)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Username)) 
                user.Username = request.Username;
            
            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();

            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Currency = user.Currency,
                CreatedAt = user.CreatedAt
            };
        }

        /// <summary>
        /// Retrieves a paginated list of cards owned by the user.
        /// </summary>
        public async Task<UserCardListResponse> GetUserCards(Guid userId, Guid? collectionId, string? grade, int limit, int offset)
        {
            // Verify user exists
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            var (cards, total) = await _cardRepo.GetCardsAsync(
                userId: userId,
                collectionId: collectionId,
                vehicleId: null,
                grade: grade,
                minValue: null,
                maxValue: null,
                sortBy: null,
                limit: limit,
                offset: offset);

            var cardResponses = cards.Select(c => new UserCardResponse
            {
                Id = c.Id,
                VehicleId = c.VehicleId,
                CollectionId = c.CollectionId,
                Grade = c.Grade.ToString(),
                Value = c.Value
            }).ToList();

            return new UserCardListResponse
            {
                Cards = cardResponses,
                Total = total,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Retrieves a list of packs owned by the user.
        /// </summary>
        public async Task<UserPackListResponse> GetUserPacks(Guid userId, Guid? collectionId)
        {
            // Verify user exists
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            var packs = await _packRepo.GetByUserIdAsync(userId, collectionId);

            var packResponses = packs.Select(p => new UserPackResponse
            {
                Id = p.Id,
                CollectionId = p.CollectionId,
                Value = p.Value
            }).ToList();

            return new UserPackListResponse
            {
                Packs = packResponses,
                Total = packResponses.Count
            };
        }

        /// <summary>
        /// Retrieves all open trades created by a specific user.
        /// </summary>
        public async Task<UserTradeListResponse> GetUserTrades(Guid userId, string? type)
        {
            // Verify user exists
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            var trades = await _openTradeRepo.FindAsync(t => t.UserId == userId);
            
            if (!string.IsNullOrEmpty(type))
                trades = trades.Where(t => t.Type.ToString() == type);

            var tradeResponses = trades.Select(t => new UserTradeResponse
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                CardId = t.CardId,
                Price = t.Price > 0 ? t.Price : null,
                WantCardId = t.WantCardId
            }).ToList();

            return new UserTradeListResponse
            {
                Trades = tradeResponses,
                Total = tradeResponses.Count()
            };
        }

        /// <summary>
        /// Retrieves the trade history for a user.
        /// </summary>
        public async Task<UserTradeHistoryListResponse> GetUserTradeHistory(Guid userId, string role, int limit, int offset)
        {
            var (trades, total) = await _tradeRepo.GetHistoryAsync(userId, role, limit, offset);

            var tradeResponses = trades.Select(t => new UserTradeHistoryResponse
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                SellerUserId = t.SellerUserId,
                SellerCardId = t.SellerCardId,
                BuyerUserId = t.BuyerUserId,
                BuyerCardId = t.BuyerCardId,
                Price = t.Price,
                ExecutedDate = t.ExecutedDate
            }).ToList();

            return new UserTradeHistoryListResponse
            {
                Trades = tradeResponses,
                Total = total,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Retrieves a list of rewards for the user.
        /// </summary>
        public async Task<UserRewardListResponse> GetUserRewards(Guid userId, bool claimed)
        {
            // Verify user exists
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            var rewards = await _rewardRepo.GetByUserIdAsync(userId, claimed);

            var rewardResponses = rewards.Select(r => new UserRewardResponse
            {
                Id = r.Id,
                UserId = r.UserId,
                ItemId = r.ItemId,
                Amount = r.Amount,
                Type = r.Type.ToString(),
                CreatedAt = r.CreatedAt,
                ClaimedAt = r.ClaimedAt
            }).ToList();

            return new UserRewardListResponse
            {
                Rewards = rewardResponses,
                Total = rewardResponses.Count
            };
        }

        /// <summary>
        /// Retrieves all cards owned by a user with full vehicle details embedded.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="collectionId">Optional collection filter.</param>
        /// <param name="grade">Optional grade filter.</param>
        /// <param name="limit">Results per page.</param>
        /// <param name="offset">Results to skip.</param>
        /// <returns>A <see cref="UserCardWithVehicleListResponse"/> containing cards with vehicle details.</returns>
        public async Task<UserCardWithVehicleListResponse> GetUserCardsWithVehicles(
            Guid userId, 
            Guid? collectionId, 
            string? grade, 
            int limit, 
            int offset)
        {
            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            // Build query with vehicle join
            IQueryable<Card> query = _context.Cards
                .Where(c => c.UserId == userId)
                .Include(c => c.Vehicle);  // Join with Vehicle table

            // Apply optional filters
            if (collectionId.HasValue)
                query = query.Where(c => c.CollectionId == collectionId);

            if (!string.IsNullOrEmpty(grade))
                query = query.Where(c => c.Grade.ToString() == grade);

            var total = await query.CountAsync();

            // Fetch cards with vehicle details
            var cards = await query
                .Skip(offset)
                .Take(limit)
                .Select(c => new UserCardWithVehicleResponse
                {
                    // Card properties
                    Id = c.Id,
                    VehicleId = c.VehicleId,
                    CollectionId = c.CollectionId,
                    Grade = c.Grade.ToString(),
                    Value = c.Value,
                    // Vehicle properties
                    Year = c.Vehicle.Year,
                    Make = c.Vehicle.Make,
                    Model = c.Vehicle.Model,
                    Stat1 = c.Vehicle.Stat1,
                    Stat2 = c.Vehicle.Stat2,
                    Stat3 = c.Vehicle.Stat3,
                    VehicleImage = c.Vehicle.Image
                })
                .ToListAsync();

            return new UserCardWithVehicleListResponse
            {
                Cards = cards,
                Total = total,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Retrieves collection completion progress for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>A <see cref="CollectionProgressResponse"/> with completion data for each collection.</returns>
        public async Task<CollectionProgressResponse> GetCollectionProgress(Guid userId)
        {
            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            // Get all cards owned by the user (grouped by collection and vehicle)
            var userCards = await _context.Cards
                .Where(c => c.UserId == userId)
                .Select(c => new { c.CollectionId, c.VehicleId })
                .ToListAsync();

            // If user has no cards, return empty response
            if (!userCards.Any())
            {
                return new CollectionProgressResponse
                {
                    Collections = new List<CollectionProgressDto>(),
                    TotalCollections = 0
                };
            }

            // Get unique collection IDs from user's cards
            var collectionIds = userCards.Select(c => c.CollectionId).Distinct().ToList();

            // Fetch collection details for those collections
            var collections = await _context.Collections
                .Where(c => collectionIds.Contains(c.Id))
                .ToListAsync();

            // Calculate progress for each collection
            var progressList = collections.Select(collection =>
            {
                // Get unique vehicles owned by user in this collection
                var ownedVehicleIds = userCards
                    .Where(c => c.CollectionId == collection.Id)
                    .Select(c => c.VehicleId)
                    .Distinct()
                    .ToList();

                int ownedCount = ownedVehicleIds.Count;
                int totalCount = collection.Vehicles.Length;
                int percentage = totalCount > 0 ? (int)((ownedCount * 100.0) / totalCount) : 0;

                return new CollectionProgressDto
                {
                    CollectionId = collection.Id,
                    CollectionName = collection.Name,
                    CollectionImage = collection.Image,
                    OwnedVehicles = ownedCount,
                    TotalVehicles = totalCount,
                    Percentage = percentage
                };
            })
            .OrderByDescending(p => p.Percentage)  // Sort by completion percentage (highest first)
            .ToList();

            return new CollectionProgressResponse
            {
                Collections = progressList,
                TotalCollections = progressList.Count
            };
        }
    }
}
