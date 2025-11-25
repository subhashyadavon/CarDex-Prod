using CarDexBackend.Domain.Entities;

namespace CarDexBackend.Repository.Interfaces
{
    public interface ICardRepository : IRepository<Card>
    {
        Task<(IEnumerable<Card> Cards, int Total)> GetCardsAsync(
            Guid? userId, 
            Guid? collectionId, 
            Guid? vehicleId, 
            string? grade, 
            int? minValue, 
            int? maxValue, 
            string? sortBy, 
            int limit, 
            int offset);
            
        Task<Card?> GetCardByIdRawAsync(Guid id);
        Task<IEnumerable<(Card Card, Vehicle Vehicle)>> GetCardsWithVehiclesByCollectionIdAsync(Guid collectionId);
    }
}
