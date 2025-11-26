using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Repository.Interfaces;
using Microsoft.Extensions.Localization;
using Microsoft.Win32.SafeHandles;
using CarDexBackend.Services.Resources;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Production implementation of <see cref="ITradeService"/> using Repositories.
    /// </summary>
    /// <remarks>
    /// NOTE: This implementation uses a hardcoded test user ID for development.
    /// In production, this should be replaced with proper authentication/authorization.
    /// </remarks>
    public class TradeService : ITradeService
    {
        private readonly IStringLocalizer<SharedResources> _sr;
        private readonly IOpenTradeRepository _openTradeRepo;
        private readonly ICompletedTradeRepository _completedTradeRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICardRepository _cardRepo;
        private readonly IRepository<Vehicle> _vehicleRepo;
        private readonly IRewardRepository _rewardRepo;
        private readonly ICurrentUserService _currentUserService;
        
        // TODO: Replace with actual authenticated user ID from JWT/claims
        private readonly Guid _testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");

        public TradeService(
            IOpenTradeRepository openTradeRepo,
            ICompletedTradeRepository completedTradeRepo,
            IUserRepository userRepo,
            ICardRepository cardRepo,
            IRepository<Vehicle> vehicleRepo,
            IRewardRepository rewardRepo,
            ICurrentUserService currentUserService,
            IStringLocalizer<SharedResources> sr)
        {
            _openTradeRepo = openTradeRepo;
            _completedTradeRepo = completedTradeRepo;
            _userRepo = userRepo;
            _cardRepo = cardRepo;
            _vehicleRepo = vehicleRepo;
            _rewardRepo = rewardRepo;
            _currentUserService = currentUserService;
            _sr = sr;
        }

        /// <summary>
        /// Retrieves a paginated list of all open trades with optional filters and sorting.
        /// </summary>
        public async Task<TradeListResponse> GetOpenTrades(
            string? type,
            Guid? collectionId,
            string? grade,
            int? minPrice,
            int? maxPrice,
            Guid? vehicleId,
            Guid? wantCardId,
            string? sortBy,
            int limit,
            int offset)
        {
            var (trades, total) = await _openTradeRepo.GetOpenTradesAsync(
                type,
                collectionId,
                grade,
                minPrice,
                maxPrice,
                vehicleId,
                wantCardId,
                sortBy,
                limit,
                offset);

            // Fetch users for these trades
            var userIds = trades.Select(t => t.UserId).Distinct().ToList();
            var users = await _userRepo.FindAsync(u => userIds.Contains(u.Id));
            var userMap = users.ToDictionary(u => u.Id, u => u.Username);

            var tradeResponses = trades.Select(t => new TradeResponse
            {
                Id = t.Id,
                Type = t.Type.ToString(),  // Will be "FOR_CARD" or "FOR_PRICE"
                UserId = t.UserId,
                Username = userMap.ContainsKey(t.UserId) ? userMap[t.UserId] : _sr["UnknownMessage"],
                CardId = t.CardId,
                Price = t.Type == TradeEnum.FOR_PRICE ? t.Price : null,
                WantCardId = t.Type == TradeEnum.FOR_CARD ? t.WantCardId : null,
                CreatedAt = DateTime.UtcNow  // Not in DB, using current time
            }).ToList();

            return new TradeListResponse
            {
                Trades = tradeResponses,
                Total = total,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Retrieves detailed information about a specific open trade by ID.
        /// </summary>
        public async Task<TradeDetailedResponse> GetOpenTradeById(Guid tradeId)
        {
            var trade = await _openTradeRepo.GetByIdAsync(tradeId);
            if (trade == null)
                throw new KeyNotFoundException(_sr["TradeNotFoundError"]);

            var user = await _userRepo.GetByIdAsync(trade.UserId);
            var card = await _cardRepo.GetCardByIdRawAsync(trade.CardId);
            
            // TradeDetailedResponse extends TradeResponse and adds: Card (CardDetailedResponse), WantCard (CardDetailedResponse)
            var response = new TradeDetailedResponse
            {
                Id = trade.Id,
                Type = trade.Type.ToString(),  // Will be "FOR_CARD" or "FOR_PRICE"
                UserId = trade.UserId,
                Username = user?.Username ?? _sr["UnknownMessage"],
                CardId = trade.CardId,
                Price = trade.Type == TradeEnum.FOR_PRICE ? trade.Price : null,
                WantCardId = trade.Type == TradeEnum.FOR_CARD ? trade.WantCardId : null,
                CreatedAt = DateTime.UtcNow  // Not in DB, using current time
            };

            // Populate Card property if card exists
            if (card != null)
            {
                var vehicle = await _vehicleRepo.GetByIdAsync(card.VehicleId);
                response.Card = new CardDetailedResponse
                {
                    Id = card.Id,
                    Name = vehicle != null ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}" : _sr["UnknownMessage"],
                    Grade = card.Grade.ToString(),  // Will be "FACTORY", "LIMITED_RUN", or "NISMO"
                    Value = card.Value,
                    CreatedAt = DateTime.UtcNow,
                    Description = vehicle != null ? $"{vehicle.Make} {vehicle.Model}" : _sr["UnknownMessage"],
                    VehicleId = card.VehicleId.ToString(),
                    CollectionId = card.CollectionId.ToString(),
                    OwnerId = card.UserId.ToString()
                };
            }

            // Populate WantCard property if wantCardId exists
            if (trade.WantCardId.HasValue)
            {
                var wantCard = await _cardRepo.GetCardByIdRawAsync(trade.WantCardId.Value);
                if (wantCard != null)
                {
                    var wantVehicle = await _vehicleRepo.GetByIdAsync(wantCard.VehicleId);
                    response.WantCard = new CardDetailedResponse
                    {
                        Id = wantCard.Id,
                        Name = wantVehicle != null ? $"{wantVehicle.Year} {wantVehicle.Make} {wantVehicle.Model}" : _sr["UnknownMessage"],
                        Grade = wantCard.Grade.ToString(),  // Will be "FACTORY", "LIMITED_RUN", or "NISMO"
                        Value = wantCard.Value,
                        CreatedAt = DateTime.UtcNow,
                        Description = wantVehicle != null ? $"{wantVehicle.Make} {wantVehicle.Model}" : _sr["UnknownMessage"],
                        VehicleId = wantCard.VehicleId.ToString(),
                        CollectionId = wantCard.CollectionId.ToString(),
                        OwnerId = wantCard.UserId.ToString()
                    };
                }
            }

            return response;
        }

        /// <summary>
        /// Creates a new trade listing for a card.
        /// </summary>
        public async Task<TradeResponse> CreateTrade(TradeCreateRequest request)
        {
            var userId = _currentUserService.UserId;;

            // Validate the card exists and belongs to user
            var card = await _cardRepo.GetCardByIdRawAsync(request.CardId);
            if (card == null)
                throw new KeyNotFoundException(_sr["CardNotFoundError"]);

            if (card.UserId != userId)
                throw new InvalidOperationException(_sr["OnlyTradeYourCardsError"]);

            // Parse trade type
            var tradeType = Enum.Parse<TradeEnum>(request.Type);

            // Validate trade type and required fields
            if (tradeType == TradeEnum.FOR_PRICE && !request.Price.HasValue)
                throw new ArgumentException(_sr["PriceRequiredForForPriceError"]);

            if (tradeType == TradeEnum.FOR_CARD && !request.WantCardId.HasValue)
                throw new ArgumentException(_sr["WantCardIdRequiredForForCardError"]);

            // OpenTrade constructor: OpenTrade(Guid id, TradeEnum type, Guid userId, Guid cardId, int price, Guid? wantCardId)
            var tradeId = Guid.NewGuid();
            var trade = new OpenTrade(tradeId, tradeType, userId, request.CardId, request.Price ?? 0, request.WantCardId);

            await _openTradeRepo.AddAsync(trade);
            await _openTradeRepo.SaveChangesAsync();

            var user = await _userRepo.GetByIdAsync(userId);

            return new TradeResponse
            {
                Id = trade.Id,
                Type = trade.Type.ToString(),  // Will be "FOR_CARD" or "FOR_PRICE"
                UserId = trade.UserId,
                Username = user?.Username ?? _sr["UnknownMessage"],
                CardId = trade.CardId,
                Price = trade.Type == TradeEnum.FOR_PRICE ? trade.Price : null,
                WantCardId = trade.Type == TradeEnum.FOR_CARD ? trade.WantCardId : null,
                CreatedAt = DateTime.UtcNow  // Not in DB, using current time
            };
        }

        /// <summary>
        /// Executes a trade transaction between two users.
        /// </summary>
        public async Task<(CompletedTradeResponse CompletedTrade, RewardResponse SellerReward, RewardResponse BuyerReward)> ExecuteTrade(Guid tradeId, TradeExecuteRequest? request)
        {
            var buyerId = _currentUserService.UserId;;

            var trade = await _openTradeRepo.GetByIdAsync(tradeId);
            if (trade == null)
                throw new KeyNotFoundException(_sr["TradeNotFoundError"]);

            var seller = await _userRepo.GetByIdAsync(trade.UserId);
            var buyer = await _userRepo.GetByIdAsync(buyerId);
            var sellerCard = await _cardRepo.GetCardByIdRawAsync(trade.CardId);

            if (seller == null || buyer == null || sellerCard == null)
                throw new InvalidOperationException(_sr["InvalidTradeParticipantError"]);

            if (buyer.Id == seller.Id)
                throw new InvalidOperationException(_sr["SelfTradeError"]);

            // CompletedTrade properties
            Guid? buyerCardId = null;
            int price = 0;
            Reward sellerRewardEntity;
            Reward buyerRewardEntity;

            if (trade.Type == TradeEnum.FOR_PRICE)
            {
                // Currency trade
                price = trade.Price;
                if (buyer.Currency < price)
                    throw new InvalidOperationException(_sr["GenericInsufficientCurrencyError"]);

                // Transfer currency
                buyer.DeductCurrency(price);
                seller.AddCurrency(price);

                // Create seller reward (currency) - Reward(Guid id, Guid userId, RewardEnum type, int amount, Guid? itemId = null)
                sellerRewardEntity = new Reward(Guid.NewGuid(), seller.Id, RewardEnum.CURRENCY_FROM_TRADE, price, null);
                await _rewardRepo.AddAsync(sellerRewardEntity);
                
                // Create buyer reward (card from seller)
                buyerRewardEntity = new Reward(Guid.NewGuid(), buyer.Id, RewardEnum.CARD_FROM_TRADE, 0, sellerCard.Id);
            }
            else if (trade.Type == TradeEnum.FOR_CARD)
            {
                // Card-for-card trade
                if (request == null || !request.BuyerCardId.HasValue)
                    throw new ArgumentException(_sr["BuyerCardIdRequiredError"]);

                var buyerCard = await _cardRepo.GetCardByIdRawAsync(request.BuyerCardId.Value);
                if (buyerCard == null)
                    throw new KeyNotFoundException(_sr["BuyerCardNotFoundError"]);

                if (buyerCard.UserId != buyer.Id)
                    throw new InvalidOperationException(_sr["OnlyTradeYourCardsError"]);

                // Swap card ownership
                buyerCard.UserId = seller.Id;
                buyerCardId = buyerCard.Id;
                await _cardRepo.UpdateAsync(buyerCard);

                // Create seller reward (card from buyer)
                sellerRewardEntity = new Reward(Guid.NewGuid(), seller.Id, RewardEnum.CARD_FROM_TRADE, 0, buyerCard.Id);
                await _rewardRepo.AddAsync(sellerRewardEntity);
                
                // Create buyer reward (card from seller)
                buyerRewardEntity = new Reward(Guid.NewGuid(), buyer.Id, RewardEnum.CARD_FROM_TRADE, 0, sellerCard.Id);
            }
            else
            {
                throw new InvalidOperationException(_sr["InvalidTradeTypeError"]);
            }

            sellerRewardEntity.ClaimedAt = DateTime.UtcNow;
            buyerRewardEntity.ClaimedAt = DateTime.UtcNow;

            // Transfer seller's card to buyer
            sellerCard.UserId = buyer.Id;
            await _cardRepo.UpdateAsync(sellerCard);

            // Update users currency
            await _userRepo.UpdateAsync(buyer);
            await _userRepo.UpdateAsync(seller);

            // Add buyer reward
            await _rewardRepo.AddAsync(buyerRewardEntity);

            // CompletedTrade constructor: CompletedTrade(Guid id, TradeEnum type, Guid sellerUserId, Guid sellerCardId, Guid buyerUserId, int price = 0, Guid? buyerCardId = null)
            var completedTradeId = Guid.NewGuid();
            var completedTrade = new CompletedTrade(
                completedTradeId,
                trade.Type,
                seller.Id,
                sellerCard.Id,
                buyer.Id,
                price,
                buyerCardId
            );

            await _completedTradeRepo.AddAsync(completedTrade);
            await _openTradeRepo.DeleteAsync(trade);
            await _openTradeRepo.SaveChangesAsync();

            var completedTradeResponse = new CompletedTradeResponse
            {
                Id = completedTrade.Id,
                Type = completedTrade.Type.ToString(),  // Will be "FOR_CARD" or "FOR_PRICE"
                SellerUserId = completedTrade.SellerUserId,
                SellerUsername = seller.Username,
                SellerCardId = completedTrade.SellerCardId,
                BuyerUserId = completedTrade.BuyerUserId,
                BuyerUsername = buyer.Username,
                BuyerCardId = completedTrade.BuyerCardId,
                Price = completedTrade.Price,
                ExecutedDate = DateTime.UtcNow
            };

            var sellerRewardResponse = new RewardResponse
            {
                Id = sellerRewardEntity.Id,
                UserId = sellerRewardEntity.UserId,
                ItemId = sellerRewardEntity.ItemId,
                Amount = sellerRewardEntity.Amount,
                Type = sellerRewardEntity.Type.ToString(),  // Will be "CURRENCY_FROM_TRADE" or "CARD_FROM_TRADE"
                CreatedAt = DateTime.UtcNow,
                ClaimedAt = sellerRewardEntity.ClaimedAt
            };

            var buyerRewardResponse = new RewardResponse
            {
                Id = buyerRewardEntity.Id,
                UserId = buyerRewardEntity.UserId,
                ItemId = buyerRewardEntity.ItemId,
                Amount = buyerRewardEntity.Amount,
                Type = buyerRewardEntity.Type.ToString(),  // Will be "CARD_FROM_TRADE"
                CreatedAt = DateTime.UtcNow,
                ClaimedAt = buyerRewardEntity.ClaimedAt
            };

            return (completedTradeResponse, sellerRewardResponse, buyerRewardResponse);
        }

        /// <summary>
        /// Deletes (cancels) an open trade listing.
        /// </summary>
        public async Task DeleteTrade(Guid tradeId)
        {
            var trade = await _openTradeRepo.GetByIdAsync(tradeId);
            if (trade == null)
                throw new KeyNotFoundException(_sr["TradeNotFound"]);

            //Verify authenticated user owns this trade
            if (trade.UserId != _currentUserService.UserId)
                throw new InvalidOperationException(_sr["OnlyDeleteYourTradeError"]);

            await _openTradeRepo.DeleteAsync(trade);
            await _openTradeRepo.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves detailed information about a specific completed trade.
        /// </summary>
        public async Task<CompletedTradeResponse> GetCompletedTradeById(Guid tradeId)
        {
            var trade = await _completedTradeRepo.GetByIdAsync(tradeId);
            if (trade == null)
                throw new KeyNotFoundException(_sr["CompletedTradeNotFoundError"]);

            var seller = await _userRepo.GetByIdAsync(trade.SellerUserId);
            var buyer = await _userRepo.GetByIdAsync(trade.BuyerUserId);

            return new CompletedTradeResponse
            {
                Id = trade.Id,
                Type = trade.Type.ToString(),  // Will be "FOR_CARD" or "FOR_PRICE"
                SellerUserId = trade.SellerUserId,
                SellerUsername = seller?.Username ?? _sr["UnknownMessage"],
                SellerCardId = trade.SellerCardId,
                BuyerUserId = trade.BuyerUserId,
                BuyerUsername = buyer?.Username ?? _sr["UnknownMessage"],
                BuyerCardId = trade.BuyerCardId,
                Price = trade.Price,
                ExecutedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Retrieves a paginated list of completed trades, optionally filtered by user.
        /// </summary>
        public async Task<TradeHistoryResponse> GetTradeHistory(Guid? userId, int limit, int offset)
        {
            var (trades, total) = await _completedTradeRepo.GetHistoryAsync(userId, null, limit, offset);

            // Fetch users
            var userIds = new List<Guid>();
            userIds.AddRange(trades.Select(t => t.SellerUserId));
            userIds.AddRange(trades.Select(t => t.BuyerUserId));
            userIds = userIds.Distinct().ToList();

            var users = await _userRepo.FindAsync(u => userIds.Contains(u.Id));
            var userMap = users.ToDictionary(u => u.Id, u => u.Username);

            var tradeResponses = trades.Select(t => new CompletedTradeResponse
            {
                Id = t.Id,
                Type = t.Type.ToString(),  // Will be "FOR_CARD" or "FOR_PRICE"
                SellerUserId = t.SellerUserId,
                SellerUsername = userMap.ContainsKey(t.SellerUserId) ? userMap[t.SellerUserId] : _sr["UnknownMessage"],
                SellerCardId = t.SellerCardId,
                BuyerUserId = t.BuyerUserId,
                BuyerUsername = userMap.ContainsKey(t.BuyerUserId) ? userMap[t.BuyerUserId] : _sr["UnknownMessage"],
                BuyerCardId = t.BuyerCardId,
                Price = t.Price,
                ExecutedDate = DateTime.UtcNow
            }).ToList();

            return new TradeHistoryResponse
            {
                Trades = tradeResponses,
                Total = total,
                Limit = limit,
                Offset = offset
            };
        }
    }
}
