using CarDexBackend.Domain.Entities;
using CarDexBackend.Domain.Enums;
using CarDexBackend.Repository.Interfaces;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Repository.Implementations
{
    public class OpenTradeRepository : Repository<OpenTrade>, IOpenTradeRepository
    {
        public OpenTradeRepository(CarDexDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<OpenTrade> Trades, int Total)> GetOpenTradesAsync(
            string? type,
            Guid? collectionId,
            string? grade,
            int? minPrice,
            int? maxPrice,
            Guid? vehicleId,
            Guid? wantCardId,
            string? sortBy,
            int limit,
            int offset)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.Type.ToString() == type);

            if (collectionId.HasValue)
            {
                var cardIds = _context.Cards
                    .Where(c => c.CollectionId == collectionId.Value)
                    .Select(c => c.Id);
                query = query.Where(t => cardIds.Contains(t.CardId));
            }

            if (!string.IsNullOrEmpty(grade))
            {
                var cardIds = _context.Cards
                    .Where(c => c.Grade.ToString() == grade)
                    .Select(c => c.Id);
                query = query.Where(t => cardIds.Contains(t.CardId));
            }

            if (minPrice.HasValue)
                query = query.Where(t => t.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(t => t.Price <= maxPrice);

            if (vehicleId.HasValue)
            {
                var cardIds = _context.Cards
                    .Where(c => c.VehicleId == vehicleId.Value)
                    .Select(c => c.Id);
                query = query.Where(t => cardIds.Contains(t.CardId));
            }

            if (wantCardId.HasValue)
                query = query.Where(t => t.WantCardId == wantCardId);

            query = sortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(t => t.Price),
                "price_desc" => query.OrderByDescending(t => t.Price),
                "date_asc" => query.OrderBy(t => t.Id),
                "date_desc" => query.OrderByDescending(t => t.Id),
                _ => query.OrderByDescending(t => t.Id)
            };

            var total = await query.CountAsync();
            var trades = await query.Skip(offset).Take(limit).ToListAsync();

            return (trades, total);
        }
    }
}
