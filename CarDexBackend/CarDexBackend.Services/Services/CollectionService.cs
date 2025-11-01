using CarDexBackend.Shared.Dtos.Responses;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Production implementation of <see cref="ICollectionService"/> using Entity Framework Core and PostgreSQL.
    /// </summary>
    public class CollectionService : ICollectionService
    {
        private readonly CarDexDbContext _context;

        public CollectionService(CarDexDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all available collections.
        /// </summary>
        public async Task<CollectionListResponse> GetAllCollections()
        {
            var collections = await _context.Collections
                .Select(c => new CollectionResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Theme = c.Name, // Using name as theme since theme field doesn't exist in entity
                    Description = c.Name, // Using name as description since description field doesn't exist
                    CardCount = _context.Cards.Count(card => card.CollectionId == c.Id)
                })
                .ToListAsync();

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
            var collection = await _context.Collections.FindAsync(collectionId);
            if (collection == null)
                throw new KeyNotFoundException("Collection not found");

            // Get all cards in this collection
            var cards = await _context.Cards
                .Where(c => c.CollectionId == collectionId)
                .Join(_context.Vehicles,
                    card => card.VehicleId,
                    vehicle => vehicle.Id,
                    (card, vehicle) => new CardResponse
                    {
                        Id = card.Id,
                        Name = $"{vehicle.Year} {vehicle.Make} {vehicle.Model}",
                        Grade = card.Grade.ToString(),  // Will be "FACTORY", "LIMITED_RUN", or "NISMO"
                        Value = card.Value,
                        CreatedAt = DateTime.UtcNow  // Not in DB, using current time
                    })
                .ToListAsync();

            return new CollectionDetailedResponse
            {
                Id = collection.Id,
                Name = collection.Name,
                Theme = collection.Name,
                Description = collection.Name,
                CardCount = cards.Count,
                Cards = cards
            };
        }
    }
}
