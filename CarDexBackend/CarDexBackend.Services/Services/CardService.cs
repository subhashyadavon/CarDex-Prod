using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Domain.Enums;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;

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
        /// Retrieves all available cards with optional filtering and pagination.
        /// </summary>
        public async Task<CardListResponse> GetAllCards(Guid? userId, Guid? collectionId, Guid? vehicleId, string? grade, int? minValue, int? maxValue, string? sortBy, int limit, int offset)
        {
            var query = _context.Cards.AsQueryable();

            // Filter by user if provided
            if (userId.HasValue)
                query = query.Where(c => c.UserId == userId.Value);

            // Filter by collection if provided
            if (collectionId.HasValue)
                query = query.Where(c => c.CollectionId == collectionId.Value);

            // Filter by vehicle if provided
            if (vehicleId.HasValue)
                query = query.Where(c => c.VehicleId == vehicleId.Value);

            // Filter by grade if provided
            if (!string.IsNullOrWhiteSpace(grade))
            {
                query = query.Where(c => c.Grade.ToString() == grade);
            }

            // Filter by value range if provided
            if (minValue.HasValue)
                query = query.Where(c => c.Value >= minValue.Value);
            if (maxValue.HasValue)
                query = query.Where(c => c.Value <= maxValue.Value);

            // Apply sorting
            query = sortBy switch
            {
                "value_asc" => query.OrderBy(c => c.Value),
                "value_desc" => query.OrderByDescending(c => c.Value),
                "grade_asc" => query.OrderBy(c => c.Grade),
                "grade_desc" => query.OrderByDescending(c => c.Grade),
                _ => query.OrderByDescending(c => c.CreatedAt) // Default: newest first
            };

            var totalCount = await query.CountAsync();
            
            // Try to execute query, catch enum mapping errors for PostgreSQL
            List<CarDexBackend.Domain.Entities.Card> cards;
            try
            {
                cards = await query.Skip(offset).Take(limit).ToListAsync();
            }
            catch (InvalidCastException)
            {
                // If enum mapping fails, use raw SQL for cards with grade as text
                try
                {
                    var cardRawData = await _context.Database
                        .SqlQueryRaw<CardRawData>(
                            @"SELECT c.id, c.user_id as user_id, c.vehicle_id, c.collection_id, c.grade::text as grade, c.value, c.created_at 
                              FROM card c 
                              ORDER BY c.created_at DESC 
                              LIMIT {0} OFFSET {1}",
                            limit, offset)
                        .ToListAsync();

                    // Convert raw data to entities manually
                    cards = new List<CarDexBackend.Domain.Entities.Card>();
                    foreach (var raw in cardRawData)
                    {
                            if (Enum.TryParse<GradeEnum>(raw.grade, true, out var gradeEnum))
                        {
                            cards.Add(new Domain.Entities.Card
                            {
                                Id = raw.id,
                                UserId = raw.user_id,
                                VehicleId = raw.vehicle_id,
                                CollectionId = raw.collection_id,
                                Grade = gradeEnum,
                                Value = raw.value,
                                CreatedAt = raw.created_at
                            });
                        }
                    }
                }
                catch
                {
                    throw new InvalidOperationException("Unable to retrieve cards from database");
                }
            }

            var cardResponses = new List<CardResponse>();
            foreach (var card in cards)
            {
                var vehicle = await _context.Vehicles.FindAsync(card.VehicleId);
                var vehicleName = vehicle != null ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}" : "Unknown Vehicle";

                cardResponses.Add(new CardResponse
                {
                    Id = card.Id,
                    Name = vehicleName,
                    Grade = card.Grade.ToString(),
                    Value = card.Value,
                    CreatedAt = card.CreatedAt
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
            // Check if we're using a relational database provider
            bool isRelational = _context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory";
            
            if (isRelational)
            {
                // For PostgreSQL, use raw SQL to avoid enum mapping issues
                try
                {
                    var cardData = await _context.Database
                        .SqlQueryRaw<CardRawData>(
                            "SELECT id, user_id, vehicle_id, collection_id, grade::text as grade, value, created_at FROM card WHERE id = {0}",
                            cardId)
                        .FirstOrDefaultAsync();

                    if (cardData != null)
                    {
                        var vehicle = await _context.Vehicles.FindAsync(cardData.vehicle_id);
                        var vehicleName = vehicle != null ? $"{vehicle.Year} {vehicle.Make} {vehicle.Model}" : "Unknown Vehicle";

                        return new CardDetailedResponse
                        {
                            Id = cardData.id,
                            Name = vehicleName,
                            Grade = cardData.grade,
                            Value = cardData.value,
                            CreatedAt = cardData.created_at,
                            Description = vehicleName,
                            VehicleId = cardData.vehicle_id.ToString(),
                            CollectionId = cardData.collection_id.ToString(),
                            OwnerId = cardData.user_id.ToString()
                        };
                    }
                }
                catch (InvalidOperationException)
                {
                    // Fall through to standard query if raw SQL fails
                }
            }

            // For in-memory database or as fallback, use standard EF query
            var card = await _context.Cards.FindAsync(cardId);
            if (card == null)
                throw new KeyNotFoundException("Card not found");

            var vehicleStandard = await _context.Vehicles.FindAsync(card.VehicleId);
            var vehicleNameStandard = vehicleStandard != null ? $"{vehicleStandard.Year} {vehicleStandard.Make} {vehicleStandard.Model}" : "Unknown Vehicle";

            return new CardDetailedResponse
            {
                Id = card.Id,
                Name = vehicleNameStandard,
                Grade = card.Grade.ToString(),
                Value = card.Value,
                CreatedAt = card.CreatedAt,
                Description = vehicleNameStandard,
                VehicleId = card.VehicleId.ToString(),
                CollectionId = card.CollectionId.ToString(),
                OwnerId = card.UserId.ToString()
            };
        }
    }
}
