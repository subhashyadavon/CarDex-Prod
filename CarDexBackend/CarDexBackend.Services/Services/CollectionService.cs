using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using CarDexBackend.Services.Resources;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Production implementation of <see cref="ICollectionService"/> using Repositories.
    /// </summary>
    public class CollectionService : ICollectionService
    {
        private readonly IStringLocalizer<SharedResources> _sr;
        private readonly ICollectionRepository _collectionRepo;
        private readonly ICardRepository _cardRepo;

        public CollectionService(ICollectionRepository collectionRepo, ICardRepository cardRepo, IStringLocalizer<SharedResources> sr)
        {
            _collectionRepo = collectionRepo;
            _cardRepo = cardRepo;
            _sr = sr;
        }

        /// <summary>
        /// Retrieves a list of all available collections.
        /// </summary>
        public async Task<CollectionListResponse> GetAllCollections()
        {
            var collectionsWithCounts = await _collectionRepo.GetAllWithCardCountsAsync();
            
            var collections = collectionsWithCounts
                .Select(c => new CollectionResponse
                {
                    Id = c.Collection.Id,
                    Name = c.Collection.Name,
                    Theme = c.Collection.Name, // Using name as theme since theme field doesn't exist in entity
                    Description = c.Collection.Name, // Using name as description since description field doesn't exist
                    CardCount = c.CardCount,
                    Price = c.Collection.PackPrice,
                    ImageUrl = c.Collection.Image
                })
                .ToList();

            return new CollectionListResponse
            {
                Collections = collections,
                Total = collections.Count
            };
        }

        /// <summary>
        /// Retrieves detailed information about a specific collection.
        /// </summary>
        public async Task<CollectionDetailedResponse> GetCollectionById(Guid collectionId)
        {
            var collection = await _collectionRepo.GetByIdAsync(collectionId);
            if (collection == null)
                throw new KeyNotFoundException(_sr["CollectionNotFoundError"]);

            // Get all cards in this collection
            var cardsWithVehicles = await _cardRepo.GetCardsWithVehiclesByCollectionIdAsync(collectionId);
            
            var cards = cardsWithVehicles
                .Select(cv => new CardResponse
                {
                    Id = cv.Card.Id,
                    Name = $"{cv.Vehicle.Year} {cv.Vehicle.Make} {cv.Vehicle.Model}",
                    Grade = cv.Card.Grade.ToString(),  // Will be "FACTORY", "LIMITED_RUN", or "NISMO"
                    Value = cv.Card.Value,
                    CreatedAt = DateTime.UtcNow,  // Not in DB, using current time
                    ImageUrl = cv.Vehicle.Image  // Added from feature/pages
                })
                .ToList();

            return new CollectionDetailedResponse
            {
                Id = collection.Id,
                Name = collection.Name,
                Theme = collection.Name,
                Description = collection.Name,
                CardCount = cards.Count,
                Cards = cards,
                Price = collection.PackPrice,  
                ImageUrl = collection.Image  
            };
        }
    }
}
