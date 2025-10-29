using CarDexBackend.Shared.Dtos.Responses;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Services
{
    // Helper class for raw SQL queries
    public class CardRawData
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public Guid vehicle_id { get; set; }
        public Guid collection_id { get; set; }
        public string grade { get; set; } = string.Empty;
        public int value { get; set; }
        public DateTime created_at { get; set; }
    }

    /// <summary>
    /// Production implementation of <see cref="ICardService"/> using Entity Framework Core and PostgreSQL.
    /// </summary>
    public class CardService : ICardService
    {
        private readonly CarDexDbContext _context;

        public CardService(CarDexDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of cards with optional filtering and pagination.
        /// </summary>
        public async Task<CardListResponse> GetAllCards(
            Guid? userId = null,
            Guid? collectionId = null,
            Guid? vehicleId = null,
            string? grade = null,
            int? minValue = null,
            int? maxValue = null,
            string? sortBy = "date_desc",
            int limit = 50,
            int offset = 0)
        {
            var query = _context.Cards.AsQueryable();

            // Apply filters
            if (userId.HasValue)
                query = query.Where(c => c.UserId == userId);

            if (collectionId.HasValue)
                query = query.Where(c => c.CollectionId == collectionId);

            if (vehicleId.HasValue)
                query = query.Where(c => c.VehicleId == vehicleId);

            if (!string.IsNullOrEmpty(grade))
                query = query.Where(c => c.Grade.ToString() == grade);

            if (minValue.HasValue)
                query = query.Where(c => c.Value >= minValue);

            if (maxValue.HasValue)
                query = query.Where(c => c.Value <= maxValue);
            
            //Used AI to help with sorting implementation
            // Apply sorting (date sorting removed - no CreatedAt in DB)
            query = sortBy?.ToLower() switch
            {
                "value_asc" => query.OrderBy(c => c.Value),
                "value_desc" => query.OrderByDescending(c => c.Value),
                "grade_asc" => query.OrderBy(c => c.Grade),
                "grade_desc" => query.OrderByDescending(c => c.Grade),
                "date_asc" => query.OrderBy(c => c.Id),  // Fallback to ID ordering
                "date_desc" => query.OrderByDescending(c => c.Id),
                _ => query.OrderByDescending(c => c.Id)  // Default order by ID descending
            };

            var total = await query.CountAsync();

            // Join with vehicles to get the name
            var cards = await query
                .Skip(offset)
                .Take(limit)
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

            return new CardListResponse
            {
                Cards = cards,
                Total = total,
                Limit = limit,
                Offset = offset
            };
        }

        /// <summary>
        /// Retrieves detailed information about a specific card.
        /// </summary>
        public async Task<CardDetailedResponse> GetCardById(Guid cardId)
        {
            // Use raw SQL query to work around enum mapping issues with Npgsql
            var cardData = await _context.Database.SqlQueryRaw<CardRawData>(
                @"SELECT c.id, c.user_id, c.vehicle_id, c.collection_id, c.grade::text as grade, c.value, c.created_at
                   FROM card c
                   WHERE c.id = @p0", cardId).FirstOrDefaultAsync();
            
            if (cardData == null)
                throw new KeyNotFoundException("Card not found");

            var vehicle = await _context.Vehicles.FindAsync(cardData.vehicle_id);
            var vehicleName = vehicle != null ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}" : "Unknown Vehicle";

            return new CardDetailedResponse
            {
                Id = cardData.id,
                Name = vehicleName,
                Grade = cardData.grade,  
                Value = cardData.value,
                CreatedAt = DateTime.UtcNow,
                Description = vehicleName,
                VehicleId = cardData.vehicle_id.ToString(),
                CollectionId = cardData.collection_id.ToString(),
                OwnerId = cardData.user_id.ToString()
            };
        }
    }
}
