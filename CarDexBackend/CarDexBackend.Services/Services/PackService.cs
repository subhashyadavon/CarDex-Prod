using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Repository.Interfaces;
using Microsoft.Extensions.Localization;
using CarDexBackend.Services.Resources;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Production implementation of <see cref="IPackService"/> using Repositories.
    /// </summary>
    /// <remarks>
    /// NOTE: This implementation uses a hardcoded test user ID for development.
    /// In production, this should be replaced with proper authentication/authorization.
    /// </remarks>
    public class PackService : IPackService
    {
        private readonly IStringLocalizer<SharedResources> _sr;
        private readonly IPackRepository _packRepo;
        private readonly ICollectionRepository _collectionRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRepository<Vehicle> _vehicleRepo;
        private readonly ICardRepository _cardRepo;
        private readonly Random _random = new Random();
        private readonly ICurrentUserService _currentUserService;

        public PackService(
            IPackRepository packRepo,
            ICollectionRepository collectionRepo,
            IUserRepository userRepo,
            IRepository<Vehicle> vehicleRepo,
            ICardRepository cardRepo,
            ICurrentUserService currentUserService,
            IStringLocalizer<SharedResources> sr)
        {
            _packRepo = packRepo;
            _collectionRepo = collectionRepo;
            _userRepo = userRepo;
            _vehicleRepo = vehicleRepo;
            _cardRepo = cardRepo;
            _currentUserService = currentUserService;
            _sr = sr;
        }

        /// <summary>
        /// Retrieves the list of packs owned by the user, optionally filtered by collection.
        /// </summary>
        public async Task<UserPackListResponse> GetUserPacks(Guid userId, Guid? collectionId)
        {
            // Note: User existence check should ideally be done by caller or handled by repository returning empty list
            // But for consistency with previous implementation, we might want to check user existence.
            // However, PackService shouldn't depend on UserRepository just for this check if it's not critical.
            // The previous implementation checked User existence.
            // Let's assume the Controller or a higher level service handles user validation, or we just return empty list.
            // If we strictly need to throw KeyNotFoundException for missing user, we need IUserRepository.
            // But usually GetUserPacks(userId) implies userId is valid (from token).
            // If we want to keep the behavior, we can inject IUserRepository.
            // For now, I'll skip the user check as it's redundant if userId comes from token.
            
            var packs = await _packRepo.GetByUserIdAsync(userId, collectionId);

            var packResponses = packs
                .Select(p => new UserPackResponse
                {
                    Id = p.Id,
                    CollectionId = p.CollectionId,
                    Value = p.Value
                })
                .ToList();

            return new UserPackListResponse
            {
                Packs = packResponses,
                Total = packResponses.Count
            };
        }

        /// <summary>
        /// Purchases a new pack for the specified collection.
        /// </summary>
        public async Task<PackPurchaseResponse> PurchasePack(PackPurchaseRequest request)
        {
            var userId = _currentUserService.UserId;    //grab authenticated user's ID
            
            // Get the collection
            var collection = await _collectionRepo.GetByIdAsync(request.CollectionId);
            if (collection == null)
                throw new ArgumentException(_sr["CollectionNotFoundError"]);

            // Get the user
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            // Check if user has enough currency (use PackPrice, not BasePackValue)
            var packPrice = collection.PackPrice;
            if (user.Currency < packPrice)
                throw new InvalidOperationException(_sr["InsufficientCurrencyError", "pack"]);

            // Deduct currency from user
            user.DeductCurrency(packPrice);
            await _userRepo.UpdateAsync(user);

            // Create the pack using constructor: Pack(Guid id, Guid userId, Guid collectionId, int value)
            var packId = Guid.NewGuid();
            var pack = new Pack(packId, userId, request.CollectionId, packPrice);

            await _packRepo.AddAsync(pack);
            await _packRepo.SaveChangesAsync();

            // PackPurchaseResponse: {Pack: PackResponse, UserCurrency: int}
            // PackResponse: {Id, CollectionId, CollectionName, PurchasedAt, IsOpened}
            return new PackPurchaseResponse
            {
                Pack = new PackResponse
                {
                    Id = pack.Id,
                    CollectionId = pack.CollectionId,
                    CollectionName = collection.Name,
                    PurchasedAt = DateTime.UtcNow,
                    IsOpened = false
                },
                UserCurrency = user.Currency
            };
        }

        /// <summary>
        /// Retrieves details of a specific pack by ID.
        /// </summary>
        public async Task<PackDetailedResponse> GetPackById(Guid packId)
        {
            // PackDetailedResponse extends PackResponse and adds: PreviewCards, EstimatedValue
            // Need to join with Collections to get CollectionName
            var pack = await _packRepo.GetByIdAsync(packId);
            if (pack == null)
                throw new KeyNotFoundException(_sr["PackNotFoundError"]);

            var collection = await _collectionRepo.GetByIdAsync(pack.CollectionId);
            if (collection == null)
                throw new KeyNotFoundException(_sr["CollectionNotFoundError"]);
            
            // Get preview cards (first 3 vehicles from this collection)
            // Note: Inefficient if collection has many vehicles, but simpler than custom query for now
            var allVehicles = await _vehicleRepo.FindAsync(v => collection.Vehicles.Contains(v.Id));
            var previewCards = allVehicles
                .Take(3)
                .Select(v => new CardResponse
                {
                    Id = Guid.NewGuid(), // Preview cards don't have real IDs yet
                    Name = $"{v.Year} {v.Make} {v.Model}",
                    Grade = GradeEnum.FACTORY.ToString(), // Default grade for preview - "FACTORY"
                    Value = v.Value,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            return new PackDetailedResponse
            {
                Id = pack.Id,
                CollectionId = pack.CollectionId,
                CollectionName = collection.Name,
                PurchasedAt = DateTime.UtcNow,
                IsOpened = pack.IsOpened,
                PreviewCards = previewCards,
                EstimatedValue = pack.Value
            };
        }

        /// <summary>
        /// Opens a pack to generate and return its contained cards.
        /// </summary>
        public async Task<PackOpenResponse> OpenPack(Guid packId)
        {
            var pack = await _packRepo.GetByIdAsync(packId);
            if (pack == null)
                throw new KeyNotFoundException(_sr["PackNotFoundError"]);

            var collection = await _collectionRepo.GetByIdAsync(pack.CollectionId);
            if (collection == null)
                throw new KeyNotFoundException(_sr["CollectionNotFoundError"]);
            
            // if attempting to open a pack they do not own, throw a Pack not found error
            if (pack.UserId != _currentUserService.UserId)
                throw new KeyNotFoundException(_sr["PackNotFoundError"]);

            // Get all vehicles from this collection using the Vehicles array
            var vehicles = (await _vehicleRepo.FindAsync(v => collection.Vehicles.Contains(v.Id))).ToList();

            if (!vehicles.Any())
                throw new InvalidOperationException(_sr["EmptyCollectionError"]);

            // Generate 5 random cards (typical pack size)
            var cards = new List<Card>();
            var cardDetailedResponses = new List<CardDetailedResponse>();

            for (int i = 0; i < 5; i++)
            {
                var randomVehicle = vehicles[_random.Next(vehicles.Count)];
                var grade = GenerateRandomGrade();
                var value = CalculateCardValue(pack.Value, grade);

                // Card constructor: Card(Guid id, Guid userId, Guid vehicleId, Guid collectionId, GradeEnum grade, int value)
                var cardId = Guid.NewGuid();
                var card = new Card(cardId, pack.UserId, randomVehicle.Id, pack.CollectionId, grade, value);

                cards.Add(card);
                
                // PackOpenResponse needs CardDetailedResponse, not CardResponse
                cardDetailedResponses.Add(new CardDetailedResponse
                {
                    Id = card.Id,
                    Name = $"{randomVehicle.Year} {randomVehicle.Make} {randomVehicle.Model}",
                    Grade = card.Grade.ToString(),
                    Value = card.Value,
                    CreatedAt = DateTime.UtcNow,
                    Description = $"{randomVehicle.Make} {randomVehicle.Model} - {grade} grade",
                    VehicleId = card.VehicleId.ToString(),
                    CollectionId = card.CollectionId.ToString(),
                    OwnerId = card.UserId.ToString(),
                    ImageUrl = randomVehicle.Image

                });
            }

            // Add cards to database
            await _cardRepo.AddRangeAsync(cards);

            // Mark pack as opened
            pack.Open(); // Use domain behavior
            await _packRepo.UpdateAsync(pack);

            await _packRepo.SaveChangesAsync();

            // PackOpenResponse: {Cards: IEnumerable<CardDetailedResponse>, Pack: PackResponse}
            return new PackOpenResponse
            {
                Cards = cardDetailedResponses,
                Pack = new PackResponse
                {
                    Id = pack.Id,
                    CollectionId = pack.CollectionId,
                    CollectionName = collection?.Name ?? "Unknown",
                    PurchasedAt = DateTime.UtcNow,
                    IsOpened = pack.IsOpened
                }
            };
        }

        /// <summary>
        /// Generates a random card grade with weighted probabilities.
        /// </summary>
        private GradeEnum GenerateRandomGrade()
        {
            var roll = _random.Next(100);

            if (roll < 10) // 10% chance for NISMO
                return GradeEnum.NISMO;
            else if (roll < 35) // 25% chance for LIMITED_RUN
                return GradeEnum.LIMITED_RUN;
            else // 65% chance for FACTORY
                return GradeEnum.FACTORY;
        }

        /// <summary>
        /// Calculates card value based on pack value and grade.
        /// </summary>
        private int CalculateCardValue(int basePackValue, GradeEnum grade)
        {
            var baseCardValue = basePackValue / 5; // Divide pack value by 5 cards

            return grade switch
            {
                GradeEnum.NISMO => (int)(baseCardValue * 3.0), // 3x multiplier
                GradeEnum.LIMITED_RUN => (int)(baseCardValue * 1.5), // 1.5x multiplier
                GradeEnum.FACTORY => baseCardValue, // 1x multiplier
                _ => baseCardValue
            };
        }
    }
}
