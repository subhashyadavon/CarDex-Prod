using CarDexBackend.Domain.Entities;
using CarDexBackend.Repository.Interfaces;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Repository.Implementations
{
    public class CompletedTradeRepository : Repository<CompletedTrade>, ICompletedTradeRepository
    {
        public CompletedTradeRepository(CarDexDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<CompletedTrade> Trades, int Total)> GetHistoryAsync(Guid? userId, string? role, int limit, int offset)
        {
            var query = _dbSet.AsQueryable();

            if (userId.HasValue)
            {
                if (string.IsNullOrEmpty(role) || role.ToLower() == "all")
                {
                    query = query.Where(t => t.SellerUserId == userId.Value || t.BuyerUserId == userId.Value);
                }
                else if (role.ToLower() == "seller")
                {
                    query = query.Where(t => t.SellerUserId == userId.Value);
                }
                else if (role.ToLower() == "buyer")
                {
                    query = query.Where(t => t.BuyerUserId == userId.Value);
                }
            }

            var total = await query.CountAsync();
            
            var trades = await query
                .OrderByDescending(t => t.ExecutedDate)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return (trades, total);
        }
    }
}
