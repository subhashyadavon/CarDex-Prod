using CarDexBackend.Domain.Entities;

namespace CarDexBackend.Repository.Interfaces
{
    public interface IPackRepository : IRepository<Pack>
    {
        Task<IEnumerable<Pack>> GetByUserIdAsync(Guid userId, Guid? collectionId);
    }
}
