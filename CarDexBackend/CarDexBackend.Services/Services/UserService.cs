using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Repository.Interfaces;
using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;
using Microsoft.Extensions.Localization;
using CarDexBackend.Services.Resources;

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
    }
}
