using CarDexBackend.Domain.Entities;

namespace CarDexBackend.Repository.Interfaces
{
    public interface IRewardRepository : IRepository<Reward>
    {
        Task<IEnumerable<Reward>> GetByUserIdAsync(Guid userId, bool? claimed);
    }
}
