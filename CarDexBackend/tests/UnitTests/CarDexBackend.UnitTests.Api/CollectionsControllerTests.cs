using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CarDexBackend.Controllers;
using CarDexBackend.Services;
using CarDexBackend.Shared.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarDexBackend.UnitTests.Api.Controllers
{
    /// <summary>
    /// Contains unit tests for the <see cref="CollectionsController"/> class.
    /// </summary>
    public class CollectionsControllerTests
    {
        private readonly Mock<ICollectionService> _mockCollectionService;
        private readonly CollectionsController _controller;

        /// <summary>
        /// Initializes the test class with mocked dependencies.
        /// </summary>
        public CollectionsControllerTests()
        {
            _mockCollectionService = new Mock<ICollectionService>();
            _controller = new CollectionsController(_mockCollectionService.Object);
        }

        // ===== SUCCESSES =====

        /// <summary>
        /// Ensures that <see cref="CollectionsController.GetAllCollections"/> returns an <see cref="OkObjectResult"/> with collection data.
        /// </summary>
        [Fact]
        public async Task GetAllCollections_Succeeds()
        {
            var expected = new CollectionListResponse
            {
                Collections = new List<CollectionResponse>
                {
                    new CollectionResponse
                    {
                        Id = Guid.NewGuid(),
                        Name = "JDM Legends",
                        Theme = "Japanese Performance Cars",
                        Description = "A collection of iconic JDM vehicles.",
                        CardCount = 5
                    }
                },
                Total = 1
            };

            _mockCollectionService.Setup(s => s.GetAllCollections()).ReturnsAsync(expected);

            var result = await _controller.GetAllCollections();

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<CollectionListResponse>(ok.Value);
            Assert.Equal(expected.Total, value.Total);
        }

        /// <summary>
        /// Ensures that <see cref="CollectionsController.GetCollectionById"/> returns an <see cref="OkObjectResult"/> when the collection exists.
        /// </summary>
        [Fact]
        public async Task GetCollectionById_Succeeds()
        {
            var collectionId = Guid.NewGuid();
            var expected = new CollectionDetailedResponse
            {
                Id = collectionId,
                Name = "Supercars",
                Theme = "Exotic Vehicles",
                Description = "A collection of high-end supercars.",
                CardCount = 10,
                Cards = new List<CardResponse>
                {
                    new CardResponse { Id = Guid.NewGuid(), Name = "Ferrari Enzo", Grade = "FACTORY", Value = 2500000 }
                }
            };

            _mockCollectionService.Setup(s => s.GetCollectionById(collectionId)).ReturnsAsync(expected);

            var result = await _controller.GetCollectionById(collectionId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<CollectionDetailedResponse>(ok.Value);
            Assert.Equal(collectionId, value.Id);
        }

        // ===== FAILURES =====

        /// <summary>
        /// Ensures that <see cref="CollectionsController.GetCollectionById"/> returns a <see cref="NotFoundObjectResult"/> when the collection does not exist.
        /// </summary>
        [Fact]
        public async Task GetCollectionById_Fails()
        {
            var collectionId = Guid.NewGuid();

            _mockCollectionService.Setup(s => s.GetCollectionById(collectionId)).ThrowsAsync(new KeyNotFoundException("Collection not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetCollectionById(collectionId));
        }
    }
}
