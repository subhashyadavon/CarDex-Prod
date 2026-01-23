using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Domain.Enums;
using CarDexBackend.Repository.Interfaces;
using CarDexBackend.Domain.Entities;
using Microsoft.Extensions.Localization;
using CarDexBackend.Services.Resources;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Production implementation of <see cref="ICardService"/> using Repositories.
    /// </summary>
    public class CardService : ICardService
    {
        private readonly IStringLocalizer<SharedResources> _sr;
        private readonly ICardRepository _cardRepo;
        private readonly IRepository<Vehicle> _vehicleRepo;
        private readonly IUserRepository _userRepo;
        private readonly IOpenTradeRepository _openTradeRepo;
        private readonly ICurrentUserService _currentUserService;

        public CardService(
            ICardRepository cardRepo, 
            IRepository<Vehicle> vehicleRepo, 
            IUserRepository userRepo,
            IOpenTradeRepository openTradeRepo,
            ICurrentUserService currentUserService,
            IStringLocalizer<SharedResources> sr)
        {
            _cardRepo = cardRepo;
            _vehicleRepo = vehicleRepo;
            _userRepo = userRepo;
            _openTradeRepo = openTradeRepo;
            _currentUserService = currentUserService;
            _sr = sr;
        }

        /// <summary>
        /// Retrieves all available cards with optional filtering and pagination.
        /// </summary>
        public async Task<CardListResponse> GetAllCards(Guid? userId, Guid? collectionId, Guid? vehicleId, string? grade, int? minValue, int? maxValue, string? sortBy, int limit, int offset)
        {
            var (cards, totalCount) = await _cardRepo.GetCardsAsync(
                userId, 
                collectionId, 
                vehicleId, 
                grade, 
                minValue, 
                maxValue, 
                sortBy, 
                limit, 
                offset);

            var cardResponses = new List<CardResponse>();
            foreach (var card in cards)
            {
                var vehicle = await _vehicleRepo.GetByIdAsync(card.VehicleId);
                var vehicleName = vehicle != null ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}" : _sr["UnknownVehicle"];

                cardResponses.Add(new CardResponse
                {
                    Id = card.Id,
                    Name = vehicleName,
                    Grade = card.Grade.ToString(),
                    Value = card.Value
                });
            }

            return new CardListResponse
            {
                Cards = cardResponses,
                Total = totalCount,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Retrieves detailed information about a specific card.
        /// </summary>
        public async Task<CardDetailedResponse> GetCardById(Guid cardId)
        {
            var card = await _cardRepo.GetCardByIdRawAsync(cardId);
            if (card == null)
                throw new KeyNotFoundException(_sr["CardNotFoundError"]);

            var vehicle = await _vehicleRepo.GetByIdAsync(card.VehicleId);
            var vehicleName = vehicle != null ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}" : _sr["UnknownVehicle"];

            return new CardDetailedResponse
            {
                Id = card.Id,
                Name = vehicleName,
                Grade = card.Grade.ToString(),
                Value = card.Value,
                Description = vehicleName,
                VehicleId = card.VehicleId.ToString(),
                CollectionId = card.CollectionId.ToString(),
                OwnerId = card.UserId.ToString()
            };
        }

        /// <summary>
        /// Retrieves detailed information about a specific vehicle.
        /// <summary>
        public async Task<VehicleDetailedResponse> GetVehicleById(Guid vehicleId)
        {
            var vehicle = await _vehicleRepo.GetByIdAsync(vehicleId);
            if (vehicle == null)
                throw new KeyNotFoundException(_sr["UnknownVehicleError"]);

            var vehicleName = vehicle != null ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}" : _sr["UnknownVehicle"];

            return new VehicleDetailedResponse
            {
                Id = vehicleId,
                Year = vehicle.Year,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Stat1 = vehicle.Stat1,
                Stat2 = vehicle.Stat2,
                Stat3 = vehicle.Stat3,
                Value = vehicle.Value,
                ImageUrl = vehicle.Image
            };
        }


        /// <summary>
        /// Retrieves a list with detailed information about all vehicles.
        /// </summary>
        public async Task<VehicleListResponse> GetAllVehicles()
        {
            var vehicles = await _vehicleRepo.GetAllAsync();

            var vehicleResponses = new List<VehicleDetailedResponse>();
            foreach (var v in vehicles)
            {
                vehicleResponses.Add(new VehicleDetailedResponse
                {
                    Id = v.Id,
                    Year = v.Year,
                    Make = v.Make,
                    Model = v.Model,
                    Stat1 = v.Stat1,
                    Stat2 = v.Stat2,
                    Stat3 = v.Stat3,
                    Value = v.Value,
                    ImageUrl = v.Image
                });
            }

            return new VehicleListResponse
            {
                Vehicles = vehicleResponses
            };
        }

        /// <summary>
        /// Sells a card directly to the system for a fraction of its value.
        /// </summary>
        public async Task<CardQuickSellResponse> QuickSellCard(Guid cardId)
        {
            var userId = _currentUserService.UserId;
            
            var card = await _cardRepo.GetCardByIdRawAsync(cardId);
            if (card == null)
                throw new KeyNotFoundException(_sr["CardNotFoundError"]);

            if (card.UserId != userId)
                throw new InvalidOperationException(_sr["OnlySellYourCardsError"]);

            // Check if card is in an open trade
            var openTrades = await _openTradeRepo.FindAsync(t => t.CardId == cardId);
            if (openTrades.Any())
                throw new InvalidOperationException(_sr["CardInTradeError"]);

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            // Sell price is 50% of card value
            int sellPrice = (int)(card.Value * 0.5);
            
            user.AddCurrency(sellPrice);
            await _userRepo.UpdateAsync(user);

            await _cardRepo.DeleteAsync(card);
            await _cardRepo.SaveChangesAsync();

            return new CardQuickSellResponse
            {
                CardId = cardId,
                SellPrice = sellPrice,
                NewUserCurrency = user.Currency
            };
        }
    }
}

