using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;
using CarDexBackend.Repository.Interfaces;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Repository.Implementations
{
    public class CardRepository : Repository<Card>, ICardRepository
    {
        public CardRepository(CarDexDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Card> Cards, int Total)> GetCardsAsync(
            Guid? userId, 
            Guid? collectionId, 
            Guid? vehicleId, 
            string? grade, 
            int? minValue, 
            int? maxValue, 
            string? sortBy, 
            int limit, 
            int offset)
        {
            var query = _dbSet.AsQueryable();

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
                if (Enum.TryParse<GradeEnum>(grade, true, out var gradeEnum))
                {
                    query = query.Where(c => c.Grade == gradeEnum);
                }
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
                _ => query.OrderByDescending(c => c.Id) // Default: by Id descending
            };

            var totalCount = await query.CountAsync();
            var cards = await query.Skip(offset).Take(limit).ToListAsync();

            return (cards, totalCount);
        }

        public async Task<Card?> GetCardByIdRawAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<(Card Card, Vehicle Vehicle)>> GetCardsWithVehiclesByCollectionIdAsync(Guid collectionId)
        {
            var query = from c in _context.Cards
                        join v in _context.Vehicles on c.VehicleId equals v.Id
                        where c.CollectionId == collectionId
                        select new { c, v };
                
            var result = await query.ToListAsync();
            return result.Select(x => (x.c, x.v));
        }
    }
}
