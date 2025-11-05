using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CarDexBackend.Services.Resources;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Production implementation of <see cref="IPackService"/> using Entity Framework Core and PostgreSQL.
    /// </summary>
    /// <remarks>
    /// NOTE: This implementation uses a hardcoded test user ID for development.
    /// In production, this should be replaced with proper authentication/authorization.
    /// </remarks>
    public class PackService : IPackService
    {
        private readonly IStringLocalizer<SharedResources> _sr;
        private readonly CarDexDbContext _context;
        private readonly Random _random = new Random();
        
        // TODO: Replace with actual authenticated user ID from JWT/claims
        private readonly Guid _testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");

        public PackService(CarDexDbContext context, IStringLocalizer<SharedResources> sr)
        {
            _context = context;
            _sr = sr;
        }

        /// <summary>
        /// Purchases a new pack for the specified collection.
        /// </summary>
        public async Task<PackPurchaseResponse> PurchasePack(PackPurchaseRequest request)
        {
            // TODO: Get actual authenticated user ID instead of using hardcoded test user
            var userId = _testUserId;
            
            // Get the collection
            var collection = await _context.Collections.FindAsync(request.CollectionId);
            if (collection == null)
                throw new ArgumentException(_sr["CollectionNotFoundError"]);

            // Get the user
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException(_sr["UserNotFoundError"]);

            // Check if user has enough currency (use PackPrice, not BasePackValue)
            var packPrice = collection.PackPrice;
            if (user.Currency < packPrice)
                throw new InvalidOperationException(_sr["InsufficientCurrencyError", "pack"]);

            // Deduct currency from user
            user.Currency -= packPrice;

            // Create the pack using constructor: Pack(Guid id, Guid userId, Guid collectionId, int value)
            var packId = Guid.NewGuid();
            var pack = new Pack(packId, userId, request.CollectionId, packPrice);

            _context.Packs.Add(pack);
            await _context.SaveChangesAsync();

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
            var pack = await _context.Packs.FindAsync(packId);
            if (pack == null)
                throw new KeyNotFoundException(_sr["PackNotFoundError"]);

            var collection = await _context.Collections.FindAsync(pack.CollectionId);
            if (collection == null)
                throw new KeyNotFoundException(_sr["CollectionNotFoundError"]);
            
            // Get preview cards (first 3 vehicles from this collection)
            var previewCards = await _context.Vehicles
                .Where(v => collection.Vehicles.Contains(v.Id))
                .Take(3)
                .Select(v => new CardResponse
                {
                    Id = Guid.NewGuid(), // Preview cards don't have real IDs yet
                    Name = $"{v.Year} {v.Make} {v.Model}",
                    Grade = GradeEnum.FACTORY.ToString(), // Default grade for preview - "FACTORY"
                    Value = v.Value,
                    CreatedAt = DateTime.UtcNow
                })
                .ToListAsync();

            return new PackDetailedResponse
            {
                Id = pack.Id,
                CollectionId = pack.CollectionId,
                CollectionName = collection.Name,
                PurchasedAt = pack.CreatedAt,
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
            var pack = await _context.Packs.FindAsync(packId);
            if (pack == null)
                throw new KeyNotFoundException(_sr["PackNotFoundError"]);

            var collection = await _context.Collections.FindAsync(pack.CollectionId);
            if (collection == null)
                throw new KeyNotFoundException(_sr["CollectionNotFoundError"]);
            
            // Get all vehicles from this collection using the Vehicles array
            var vehicles = await _context.Vehicles
                .Where(v => collection.Vehicles.Contains(v.Id))
                .ToListAsync();

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
                    OwnerId = card.UserId.ToString()
                });
            }

            // Add cards to database
            _context.Cards.AddRange(cards);

            // Mark pack as opened
            pack.Open(); // Use domain behavior

            await _context.SaveChangesAsync();

            // PackOpenResponse: {Cards: IEnumerable<CardDetailedResponse>, Pack: PackResponse}
            return new PackOpenResponse
            {
                Cards = cardDetailedResponses,
                Pack = new PackResponse
                {
                    Id = pack.Id,
                    CollectionId = pack.CollectionId,
                    CollectionName = collection?.Name ?? "Unknown",
                    PurchasedAt = pack.CreatedAt,
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
