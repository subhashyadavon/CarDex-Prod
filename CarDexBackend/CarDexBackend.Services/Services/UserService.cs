using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Repository.Interfaces;
using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;
using Microsoft.Extensions.Localization;
using CarDexBackend.Services.Resources;
using System.Linq;

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
        private readonly ICollectionRepository _collectionRepo;

        public UserService(
            IUserRepository userRepo,
            ICardRepository cardRepo,
            IPackRepository packRepo,
            IOpenTradeRepository openTradeRepo,
            ICompletedTradeRepository tradeRepo,
            IRewardRepository rewardRepo,
            IRepository<Vehicle> vehicleRepo,
            ICollectionRepository collectionRepo,
            IStringLocalizer<SharedResources> sr)
        {
            _userRepo = userRepo;
            _cardRepo = cardRepo;
            _packRepo = packRepo;
            _openTradeRepo = openTradeRepo;
            _tradeRepo = tradeRepo;
            _rewardRepo = rewardRepo;
            _vehicleRepo = vehicleRepo;
            _collectionRepo = collectionRepo;
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
            
            // Update password if provided (hash it first)
            if (!string.IsNullOrWhiteSpace(request.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
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
                Total = tradeResponses.Count
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
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            // Get cards using repository
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

            // Fetch vehicle details for each card
            var cardResponses = new List<UserCardWithVehicleResponse>();
            foreach (var card in cards)
            {
                var vehicle = await _vehicleRepo.GetByIdAsync(card.VehicleId);
                if (vehicle != null)
                {
                    cardResponses.Add(new UserCardWithVehicleResponse
                    {
                        // Card properties
                        Id = card.Id,
                        VehicleId = card.VehicleId,
                        CollectionId = card.CollectionId,
                        Grade = card.Grade.ToString(),
                        Value = card.Value,
                        // Vehicle properties
                        Year = vehicle.Year,
                        Make = vehicle.Make,
                        Model = vehicle.Model,
                        Stat1 = vehicle.Stat1,
                        Stat2 = vehicle.Stat2,
                        Stat3 = vehicle.Stat3,
                        VehicleImage = vehicle.Image
                    });
                }
            }

            return new UserCardWithVehicleListResponse
            {
                Cards = cardResponses,
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
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            // Get all cards owned by the user
            var (userCards, _) = await _cardRepo.GetCardsAsync(
                userId: userId,
                collectionId: null,
                vehicleId: null,
                grade: null,
                minValue: null,
                maxValue: null,
                sortBy: null,
                limit: int.MaxValue,
                offset: 0);

            var userCardsList = userCards.ToList();

            // If user has no cards, return empty response
            if (!userCardsList.Any())
            {
                return new CollectionProgressResponse
                {
                    Collections = new List<CollectionProgressDto>(),
                    TotalCollections = 0
                };
            }

            // Get unique collection IDs from user's cards
            var collectionIds = userCardsList.Select(c => c.CollectionId).Distinct().ToList();

            // Fetch collection details for those collections
            var collections = await _collectionRepo.FindAsync(c => collectionIds.Contains(c.Id));
            var collectionsList = collections.ToList();

            // Calculate progress for each collection
            var progressList = collectionsList.Select(collection =>
            {
                // Get unique vehicles owned by user in this collection
                var ownedVehicleIds = userCardsList
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
