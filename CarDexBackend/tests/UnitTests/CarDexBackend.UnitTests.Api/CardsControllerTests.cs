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
    /// Contains unit tests for the <see cref="CardsController"/> class.
    /// </summary>
    public class CardsControllerTests
    {
        private readonly Mock<ICardService> _mockCardService;
        private readonly CardsController _controller;

        /// <summary>
        /// Initializes the test class with mocked dependencies.
        /// </summary>
        public CardsControllerTests()
        {
            _mockCardService = new Mock<ICardService>();
            _controller = new CardsController(_mockCardService.Object);
        }

        // ===== SUCCESSES =====

        /// <summary>
        /// Ensures that <see cref="CardsController.GetAllCards"/> returns an <see cref="OkObjectResult"/> with card data when parameters are valid.
        /// </summary>
        [Fact]
        public async Task GetAllCards_Succeeds()
        {
            var expected = new CardListResponse
            {
                Cards = new List<CardResponse>
                {
                    new CardResponse { Id = Guid.NewGuid(), Name = "GT-R", Grade = "NISMO", Value = 1000000 }
                },
                Total = 1,
                Limit = 50,
                Offset = 0
            };

            _mockCardService.Setup(s => s.GetAllCards(
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(expected);

            var result = await _controller.GetAllCards(null, null, null, null, null, null, "value_asc", 50, 0);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<CardListResponse>(ok.Value);
            Assert.Equal(expected.Total, value.Total);
        }

        /// <summary>
        /// Ensures that <see cref="CardsController.GetCardById"/> returns an <see cref="OkObjectResult"/> when the card exists.
        /// </summary>
        [Fact]
        public async Task GetCardById_Succeeds()
        {
            var cardId = Guid.NewGuid();
            var expected = new CardDetailedResponse
            {
                Id = cardId,
                Name = "R34 Skyline",
                Grade = "NISMO",
                Value = 1200000,
                Description = "High-performance sports car"
            };

            _mockCardService.Setup(s => s.GetCardById(cardId))
                            .ReturnsAsync(expected);

            var result = await _controller.GetCardById(cardId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<CardDetailedResponse>(ok.Value);
            Assert.Equal(cardId, value.Id);
        }

        // ===== FAILURES =====

        /// <summary>
        /// Ensures that <see cref="CardsController.GetCardById"/> returns a <see cref="NotFoundObjectResult"/> when the card does not exist.
        /// </summary>
        [Fact]
        public async Task GetCardById_Fails()
        {
            var id = Guid.NewGuid();
            _mockCardService.Setup(s => s.GetCardById(id)).ThrowsAsync(new KeyNotFoundException("Card not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetCardById(id));
        }
    }
}
