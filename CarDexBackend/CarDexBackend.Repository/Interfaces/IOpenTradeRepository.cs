using CarDexBackend.Domain.Entities;

namespace CarDexBackend.Repository.Interfaces
{
    public interface IOpenTradeRepository : IRepository<OpenTrade>
    {
        Task<(IEnumerable<OpenTrade> Trades, int Total)> GetOpenTradesAsync(
            string? type,
            Guid? collectionId,
            string? grade,
            int? minPrice,
            int? maxPrice,
            Guid? vehicleId,
            Guid? wantCardId,
            string? sortBy,
            int limit,
            int offset);
    }
}
