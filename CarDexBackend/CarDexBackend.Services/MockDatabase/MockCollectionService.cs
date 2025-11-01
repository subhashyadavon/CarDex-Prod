using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Mock implementation of <see cref="ICollectionService"/> for development and testing.
    /// </summary>
    /// <remarks>
    /// Provides an in-memory representation of collectible car collections,
    /// each containing themed vehicle cards. This mock simulates how a real
    /// service might return data from a persistent database.
    /// </remarks>
    public class MockCollectionService : ICollectionService
    {
        /// <summary>
        /// Static list of mock collections preloaded at service initialization.
        /// </summary>
        /// <remarks>
        /// Each collection contains a group of <see cref="CardResponse"/> objects
        /// representing themed car sets such as <i>JDM Legends</i> or <i>Trail Conquerors</i>.
        /// </remarks>
        private readonly List<CollectionDetailedResponse> _collections = new();

        /// <summary>
        /// Initializes the mock collection data with predefined sample content.
        /// </summary>
        public MockCollectionService()
        {
            // --- Collection 1: JDM Legends ---
            var coll1 = new CollectionDetailedResponse
            {
                Id = Guid.NewGuid(),
                Name = "JDM Legends",
                Theme = "High-performance Japanese sports cars",
                Description = "A tribute to iconic JDM machines that defined a generation of speed and tuning culture.",
                CardCount = 3,
                Cards = new List<CardResponse>
                {
                    new() { Id = Guid.NewGuid(), Name = "Nissan Skyline GT-R R34", Grade = "NISMO", Value = 35000, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Subaru Impreza WRX STI 22B", Grade = "NISMO", Value = 45000, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Honda Civic Type R (EK9)", Grade = "NISMO", Value = 25000, CreatedAt = DateTime.UtcNow }
                }
            };

            // --- Collection 2: Roadster Rivals ---
            var coll2 = new CollectionDetailedResponse
            {
                Id = Guid.NewGuid(),
                Name = "Roadster Rivals",
                Theme = "Lightweight sports cars built for handling and pure driving joy",
                Description = "Iconic RWD machines that deliver thrills through corners and speed on the open road.",
                CardCount = 2,
                Cards = new List<CardResponse>
                {
                    new() { Id = Guid.NewGuid(), Name = "Mazda Miata NA", Grade = "FACTORY", Value = 12000, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Toyota Supra Mk4", Grade = "FACTORY", Value = 32000, CreatedAt = DateTime.UtcNow },
                }
            };

            // --- Collection 3: Trail Conquerors ---
            var coll3 = new CollectionDetailedResponse
            {
                Id = Guid.NewGuid(),
                Name = "Trail Conquerors",
                Theme = "Off-road mastery and unstoppable endurance",
                Description = "Built for mud, mountains, and mayhem, these beasts laugh at paved roads.",
                CardCount = 4,
                Cards = new List<CardResponse>
                {
                    new() { Id = Guid.NewGuid(), Name = "Toyota Land Cruiser 70 Series", Grade = "FACTORY", Value = 40000, CreatedAt = DateTime.UtcNow },
                    new() { Id = Guid.NewGuid(), Name = "Jeep Wrangler Rubicon", Grade = "LIMITED_RUN", Value = 45000, CreatedAt = DateTime.UtcNow.AddDays(-3) },
                    new() { Id = Guid.NewGuid(), Name = "Ford F-150 Raptor", Grade = "NISMO", Value = 55000, CreatedAt = DateTime.UtcNow.AddDays(-7) },
                    new() { Id = Guid.NewGuid(), Name = "Land Rover Defender 110", Grade = "FACTORY", Value = 48000, CreatedAt = DateTime.UtcNow.AddDays(-1) }
                }
            };

            // Add all collections to mock storage
            _collections.AddRange(new[] { coll1, coll2, coll3 });
        }

        /// <summary>
        /// Retrieves all available collections with summarized information.
        /// </summary>
        /// <returns>
        /// A <see cref="CollectionListResponse"/> containing all collections with basic details.
        /// </returns>
        public Task<CollectionListResponse> GetAllCollections()
        {
            var response = new CollectionListResponse
            {
                Collections = _collections.Select(c => new CollectionResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Theme = c.Theme,
                    Description = c.Description,
                    CardCount = c.CardCount
                }),
                Total = _collections.Count
            };

            return Task.FromResult(response);
        }

        /// <summary>
        /// Retrieves a detailed view of a specific collection by its unique identifier.
        /// </summary>
        /// <param name="collectionId">The unique identifier of the collection.</param>
        /// <returns>
        /// A <see cref="CollectionDetailedResponse"/> object containing all cards in the collection.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown when the specified collection does not exist.</exception>
        public Task<CollectionDetailedResponse> GetCollectionById(Guid collectionId)
        {
            var collection = _collections.FirstOrDefault(c => c.Id == collectionId);
            if (collection == null)
                throw new KeyNotFoundException("Collection not found");

            return Task.FromResult(collection);
        }
    }
}
