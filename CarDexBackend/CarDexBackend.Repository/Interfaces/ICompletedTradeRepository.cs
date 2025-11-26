using CarDexBackend.Domain.Entities;

namespace CarDexBackend.Repository.Interfaces
{
    public interface ICompletedTradeRepository : IRepository<CompletedTrade>
    {
        Task<(IEnumerable<CompletedTrade> Trades, int Total)> GetHistoryAsync(Guid? userId, string? role, int limit, int offset);
    }
}
