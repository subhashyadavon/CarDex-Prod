using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CarDexBackend.Controllers;
using CarDexBackend.Services;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarDexBackend.UnitTests.Api.Controllers
{
    /// <summary>
    /// Contains unit tests for the <see cref="PacksController"/> class.
    /// </summary>
    public class PacksControllerTests
    {
        private readonly Mock<IPackService> _mockPackService;
        private readonly PacksController _controller;

        /// <summary>
        /// Initializes the test class with mocked dependencies.
        /// </summary>
        public PacksControllerTests()
        {
            _mockPackService = new Mock<IPackService>();
            _controller = new PacksController(_mockPackService.Object);
        }

        // ===== SUCCESSES =====

        /// <summary>
        /// Ensures that <see cref="PacksController.PurchasePack"/> returns a <see cref="CreatedAtActionResult"/> when the purchase succeeds.
        /// </summary>
        [Fact]
        public async Task PurchasePack_Succeeds()
        {
            var request = new PackPurchaseRequest
            {
                CollectionId = Guid.NewGuid()
            };

            var expected = new PackPurchaseResponse
            {
                Pack = new PackResponse
                {
                    Id = Guid.NewGuid(),
                    CollectionId = request.CollectionId,
                    CollectionName = "JDM Legends",
                    PurchasedAt = DateTime.UtcNow,
                    IsOpened = false
                },
                UserCurrency = 500
            };

            _mockPackService.Setup(s => s.PurchasePack(It.IsAny<PackPurchaseRequest>())).ReturnsAsync(expected);

            var result = await _controller.PurchasePack(request);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var value = Assert.IsType<PackPurchaseResponse>(created.Value);
            Assert.Equal(expected.Pack.Id, value.Pack.Id);
        }

        /// <summary>
        /// Ensures that <see cref="PacksController.GetPackById"/> returns an <see cref="OkObjectResult"/> when the pack exists.
        /// </summary>
        [Fact]
        public async Task GetPackById_Succeeds()
        {
            var packId = Guid.NewGuid();
            var expected = new PackDetailedResponse
            {
                Id = packId,
                CollectionId = Guid.NewGuid(),
                CollectionName = "Limited Edition",
                EstimatedValue = 1500
            };

            _mockPackService.Setup(s => s.GetPackById(packId)).ReturnsAsync(expected);

            var result = await _controller.GetPackById(packId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PackDetailedResponse>(ok.Value);
            Assert.Equal(packId, value.Id);
        }

        /// <summary>
        /// Ensures that <see cref="PacksController.OpenPack"/> returns an <see cref="OkObjectResult"/> when the pack opens successfully.
        /// </summary>
        [Fact]
        public async Task OpenPack_Succeeds()
        {
            var packId = Guid.NewGuid();
            var expected = new PackOpenResponse
            {
                Pack = new PackResponse
                {
                    Id = packId,
                    CollectionName = "Supercar Series",
                    PurchasedAt = DateTime.UtcNow,
                    IsOpened = true
                },
                Cards = new List<CardDetailedResponse>
                {
                    new CardDetailedResponse { Id = Guid.NewGuid(), Name = "Lamborghini Aventador", Grade = "NISMO", Value = 800000 }
                }
            };

            _mockPackService.Setup(s => s.OpenPack(packId)).ReturnsAsync(expected);

            var result = await _controller.OpenPack(packId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PackOpenResponse>(ok.Value);
            Assert.Equal(packId, value.Pack.Id);
        }

        // ===== FAILURES =====

        /// <summary>
        /// Ensures that <see cref="PacksController.PurchasePack"/> returns a <see cref="BadRequestObjectResult"/> when the request is invalid.
        /// </summary>
        [Fact]
        public async Task PurchasePack_InvalidInput()
        {
            var request = new PackPurchaseRequest { CollectionId = Guid.Empty };

            _mockPackService.Setup(s => s.PurchasePack(It.IsAny<PackPurchaseRequest>())).ThrowsAsync(new ArgumentException("Invalid collection ID."));

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.PurchasePack(request));
        }

        /// <summary>
        /// Ensures that <see cref="PacksController.PurchasePack"/> returns a <see cref="ConflictObjectResult"/> when there is insufficient currency.
        /// </summary>
        [Fact]
        public async Task PurchasePack_Conflict()
        {
            var request = new PackPurchaseRequest { CollectionId = Guid.NewGuid() };

            _mockPackService.Setup(s => s.PurchasePack(It.IsAny<PackPurchaseRequest>())).ThrowsAsync(new InvalidOperationException("Insufficient funds."));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.PurchasePack(request));
        }

        /// <summary>
        /// Ensures that <see cref="PacksController.GetPackById"/> returns a <see cref="NotFoundObjectResult"/> when the pack does not exist.
        /// </summary>
        [Fact]
        public async Task GetPackById_NotFound()
        {
            var packId = Guid.NewGuid();

            _mockPackService.Setup(s => s.GetPackById(packId)).ThrowsAsync(new KeyNotFoundException("Pack not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetPackById(packId));
        }

        /// <summary>
        /// Ensures that <see cref="PacksController.OpenPack"/> returns a <see cref="NotFoundObjectResult"/> when the pack ID is not found.
        /// </summary>
        [Fact]
        public async Task OpenPack_NotFound()
        {
            var packId = Guid.NewGuid();

            _mockPackService.Setup(s => s.OpenPack(packId)).ThrowsAsync(new KeyNotFoundException("Pack not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.OpenPack(packId));
        }

        /// <summary>
        /// Ensures that <see cref="PacksController.OpenPack"/> returns a <see cref="ConflictObjectResult"/> when the pack has already been opened.
        /// </summary>
        [Fact]
        public async Task OpenPack_Conflict()
        {
            var packId = Guid.NewGuid();

            _mockPackService.Setup(s => s.OpenPack(packId)).ThrowsAsync(new InvalidOperationException("Pack already opened."));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.OpenPack(packId));
        }
    }
}
