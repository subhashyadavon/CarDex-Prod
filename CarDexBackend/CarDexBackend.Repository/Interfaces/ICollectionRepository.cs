using CarDexBackend.Domain.Entities;

namespace CarDexBackend.Repository.Interfaces
{
    public interface ICollectionRepository : IRepository<Collection>
    {
        Task<IEnumerable<(Collection Collection, int CardCount)>> GetAllWithCardCountsAsync();
    }
}
