using CarDexBackend.Domain.Entities;
using CarDexBackend.Repository.Interfaces;
using CarDexDatabase;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.Repository.Implementations
{
    public class CollectionRepository : Repository<Collection>, ICollectionRepository
    {
        public CollectionRepository(CarDexDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<(Collection Collection, int CardCount)>> GetAllWithCardCountsAsync()
        {
            var collections = await _dbSet.ToListAsync();
            
            var cardCounts = await _context.Cards
                .GroupBy(c => c.CollectionId)
                .Select(g => new { CollectionId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CollectionId, x => x.Count);

            var result = new List<(Collection, int)>();
            foreach (var collection in collections)
            {
                result.Add((collection, cardCounts.ContainsKey(collection.Id) ? cardCounts[collection.Id] : 0));
            }
            
            return result;
        }
    }
}
