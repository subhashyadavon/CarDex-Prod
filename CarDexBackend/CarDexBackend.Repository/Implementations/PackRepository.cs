using CarDexBackend.Domain.Entities;
using CarDexBackend.Repository.Interfaces;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Repository.Implementations
{
    public class PackRepository : Repository<Pack>, IPackRepository
    {
        public PackRepository(CarDexDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Pack>> GetByUserIdAsync(Guid userId, Guid? collectionId)
        {
            var query = _dbSet.Where(p => p.UserId == userId);
            
            if (collectionId.HasValue)
                query = query.Where(p => p.CollectionId == collectionId);
                
            return await query.ToListAsync();
        }
    }
}
