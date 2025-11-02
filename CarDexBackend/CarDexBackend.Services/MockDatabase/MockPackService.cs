using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;

namespace CarDexBackend.Services
{
    /// <summary>
    /// Mock implementation of <see cref="IPackService"/> used for development and testing.
    /// </summary>
    /// <remarks>
    /// This mock service simulates purchasing and opening card packs without a real backend or database.
    /// It includes a simple in-memory list of purchased packs and a basic currency counter to imitate user transactions.
    /// </remarks>
    public class MockPackService : IPackService
    {
        /// <summary>
        /// Simulated list of all purchased packs.
        /// </summary>
        private readonly List<PackDetailedResponse> _packs = new();

        /// <summary>
        /// Simulated in-memory currency balance for the current user.
        /// </summary>
        private int _userCurrency = 50000;

        /// <summary>
        /// Purchases a new pack from the specified collection.
        /// </summary>
        /// <param name="request">The request containing the collection ID from which the pack is purchased.</param>
        /// <returns>
        /// A <see cref="PackPurchaseResponse"/> containing the newly created pack and the user's updated currency balance.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the provided collection ID is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the user does not have enough currency to complete the purchase.</exception>
        public Task<PackPurchaseResponse> PurchasePack(PackPurchaseRequest request)
        {
            if (request.CollectionId == Guid.Empty)
                throw new ArgumentException("Invalid collection ID.");

            const int PACK_COST = 15000;

            if (_userCurrency < PACK_COST)
                throw new InvalidOperationException("Insufficient currency.");

            // Create a new mock pack
            var newPack = new PackDetailedResponse
            {
                Id = Guid.NewGuid(),
                CollectionId = request.CollectionId,
                CollectionName = "JDM Legends", // mock collection name for testing
                PurchasedAt = DateTime.UtcNow,
                IsOpened = false,
                EstimatedValue = 10000
            };

            _packs.Add(newPack);
            _userCurrency -= PACK_COST;

            return Task.FromResult(new PackPurchaseResponse
            {
                Pack = newPack,
                UserCurrency = _userCurrency
            });
        }

        /// <summary>
        /// Retrieves a pack by its unique identifier.
        /// </summary>
        /// <param name="packId">The unique pack ID.</param>
        /// <returns>A <see cref="PackDetailedResponse"/> representing the pack details.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the pack cannot be found.</exception>
        public Task<PackDetailedResponse> GetPackById(Guid packId)
        {
            var pack = _packs.FirstOrDefault(p => p.Id == packId);
            if (pack == null)
                throw new KeyNotFoundException("Pack not found.");

            return Task.FromResult(pack);
        }

        /// <summary>
        /// Opens an existing pack and generates mock card rewards.
        /// </summary>
        /// <param name="packId">The unique pack ID to open.</param>
        /// <returns>
        /// A <see cref="PackOpenResponse"/> containing the opened pack and the generated cards.
        /// </returns>
        /// <exception cref="KeyNotFoundException">Thrown when the pack ID does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the pack has already been opened.</exception>
        public Task<PackOpenResponse> OpenPack(Guid packId)
        {
            var pack = _packs.FirstOrDefault(p => p.Id == packId);
            if (pack == null)
                throw new KeyNotFoundException("Pack not found.");

            if (pack.IsOpened)
                throw new InvalidOperationException("Pack already opened.");

            pack.IsOpened = true;

            // Generate a few mock cards as the "opened" result
            var generatedCards = new List<CardDetailedResponse>
            {
                new() { Id = Guid.NewGuid(), Name = "Toyota AE86 Trueno", Grade = "LIMITED_RUN", Value = 22000, CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new() { Id = Guid.NewGuid(), Name = "Mazda Miata NA", Grade = "FACTORY", Value = 12000, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new() { Id = Guid.NewGuid(), Name = "Honda Civic Type R (EK9)", Grade = "NISMO", Value = 25000, CreatedAt = DateTime.UtcNow.AddDays(-6) }
            };

            var response = new PackOpenResponse
            {
                Pack = pack,
                Cards = generatedCards
            };

            return Task.FromResult(response);
        }
    }
}
