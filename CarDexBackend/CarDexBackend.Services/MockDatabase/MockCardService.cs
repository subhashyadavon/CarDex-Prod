using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Mock implementation of <see cref="ICardService"/> used for testing and development.
    /// </summary>
    /// <remarks>
    /// This mock provides a static in-memory list of collectible vehicle cards.
    /// It supports filtering, pagination, and basic lookups by card ID, simulating real database queries.
    /// </remarks>
    public class MockCardService : ICardService
    {   
        /// <summary>
        /// Static in-memory list of detailed vehicle cards used for mock data.
        /// </summary>
        /// <remarks>
        /// Each card represents a real-world performance vehicle with attributes like
        /// grade, value, and creation timestamp. These values are fixed at runtime and
        /// reset when the API restarts.
        /// </remarks>
        private readonly List<CardDetailedResponse> _cards = new()
        {
            new() { Id = Guid.NewGuid(), Name = "Nissan Skyline GT-R R34", Grade = "NISMO", Value = 35000, CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { Id = Guid.NewGuid(), Name = "Mazda RX-7 FD3S", Grade = "LIMITED_RUN", Value = 28000, CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Id = Guid.NewGuid(), Name = "Toyota Supra Mk4", Grade = "FACTORY", Value = 32000, CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new() { Id = Guid.NewGuid(), Name = "Honda NSX Type R (NA1)", Grade = "LIMITED_RUN", Value = 40000, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), Name = "Subaru Impreza WRX STI 22B", Grade = "NISMO", Value = 45000, CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { Id = Guid.NewGuid(), Name = "Mitsubishi Lancer Evolution VI TME", Grade = "FACTORY", Value = 30000, CreatedAt = DateTime.UtcNow.AddDays(-12) },
            new() { Id = Guid.NewGuid(), Name = "Nissan 300ZX Twin Turbo (Z32)", Grade = "FACTORY", Value = 18000, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { Id = Guid.NewGuid(), Name = "Toyota AE86 Trueno", Grade = "LIMITED_RUN", Value = 22000, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new() { Id = Guid.NewGuid(), Name = "Mazda Miata NA", Grade = "FACTORY", Value = 12000, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = Guid.NewGuid(), Name = "Honda Civic Type R (EK9)", Grade = "NISMO", Value = 25000, CreatedAt = DateTime.UtcNow.AddDays(-6) }
        };

        /// <summary>
        /// Retrieves all available cards with optional filtering and pagination.
        /// </summary>
        /// <param name="userId">Optional user ID filter (unused in mock).</param>
        /// <param name="collectionId">Optional collection filter (unused in mock).</param>
        /// <param name="vehicleId">Optional vehicle ID filter (unused in mock).</param>
        /// <param name="grade">Optional grade filter (e.g., FACTORY, NISMO, LIMITED_RUN).</param>
        /// <param name="minValue">Optional minimum value filter.</param>
        /// <param name="maxValue">Optional maximum value filter.</param>
        /// <param name="sortBy">Optional sort order (currently unused in mock).</param>
        /// <param name="limit">Maximum number of results to return (default 50).</param>
        /// <param name="offset">Number of results to skip for pagination.</param>
        /// <returns>A <see cref="CardListResponse"/> containing filtered and paginated cards.</returns>
        public Task<CardListResponse> GetAllCards(
            Guid? userId = null,
            Guid? collectionId = null,
            Guid? vehicleId = null,
            string? grade = null,
            int? minValue = null,
            int? maxValue = null,
            string? sortBy = "date_desc",
            int limit = 50,
            int offset = 0
        )
        {
            // Start with full card list
            var filtered = _cards.AsEnumerable();

            // Apply filters (grade, minValue, maxValue)
            if (!string.IsNullOrEmpty(grade))
                filtered = filtered.Where(c => c.Grade.Equals(grade, StringComparison.OrdinalIgnoreCase));

            if (minValue.HasValue)
                filtered = filtered.Where(c => c.Value >= minValue);

            if (maxValue.HasValue)
                filtered = filtered.Where(c => c.Value <= maxValue);

            // Prepare paginated response
            var result = new CardListResponse
            {
                Cards = filtered.Skip(offset).Take(limit),
                Total = filtered.Count(),
                Limit = limit,
                Offset = offset
            };

            return Task.FromResult(result);
        }

        /// <summary>
        /// Retrieves a single card by its unique identifier.
        /// </summary>
        /// <param name="cardId">The unique card ID.</param>
        /// <returns>The detailed card information.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the card ID does not exist.</exception>
        public Task<CardDetailedResponse> GetCardById(Guid cardId)
        {
            var card = _cards.FirstOrDefault(c => c.Id == cardId);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            return Task.FromResult(card);
        }
    }
}
