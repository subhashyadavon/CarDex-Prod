using CarDexBackend.Domain.Entities;
using CarDexBackend.Repository.Interfaces;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Repository.Implementations
{
    public class RewardRepository : Repository<Reward>, IRewardRepository
    {
        public RewardRepository(CarDexDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reward>> GetByUserIdAsync(Guid userId, bool? claimed)
        {
            var query = _dbSet.Where(r => r.UserId == userId);
            
            if (claimed.HasValue)
            {
                if (claimed.Value)
                    query = query.Where(r => r.ClaimedAt != null);
                else
                    query = query.Where(r => r.ClaimedAt == null);
            }
                
            return await query.ToListAsync();
        }
    }
}
