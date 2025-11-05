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
    /// Contains unit tests for the <see cref="TradesController"/> class.
    /// </summary>
    public class TradesControllerTests
    {
        private readonly Mock<ITradeService> _mockTradeService;
        private readonly TradesController _controller;

        /// <summary>
        /// Initializes the test class with mocked dependencies.
        /// </summary>
        public TradesControllerTests()
        {
            _mockTradeService = new Mock<ITradeService>();
            _controller = new TradesController(_mockTradeService.Object);
        }

        // ===== SUCCESSES =====

        /// <summary>
        /// Ensures that <see cref="TradesController.GetOpenTrades"/> returns an <see cref="OkObjectResult"/> with trade listings.
        /// </summary>
        [Fact]
        public async Task GetOpenTrades_Succeeds()
        {
            var expected = new TradeListResponse
            {
                Trades = new List<TradeResponse>
                {
                    new TradeResponse
                    {
                        Id = Guid.NewGuid(),
                        Type = "FOR_PRICE",
                        Username = "seller1",
                        Price = 500
                    }
                },
                Total = 1,
                Limit = 50,
                Offset = 0
            };

            _mockTradeService.Setup(s => s.GetOpenTrades(
                It.IsAny<string?>(), It.IsAny<Guid?>(), It.IsAny<string?>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Guid?>(),
                It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>())
                ).ReturnsAsync(expected);

            var result = await _controller.GetOpenTrades(null, null, null, null, null, null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<TradeListResponse>(ok.Value);
            Assert.Equal(expected.Total, value.Total);
        }

        /// <summary>
        /// Ensures that <see cref="TradesController.GetOpenTradeById"/> returns an <see cref="OkObjectResult"/> when the trade exists.
        /// </summary>
        [Fact]
        public async Task GetOpenTradeById_Succeeds()
        {
            var tradeId = Guid.NewGuid();
            var expected = new TradeDetailedResponse
            {
                Id = tradeId,
                Type = "FOR_PRICE",
                Price = 1000
            };

            _mockTradeService.Setup(s => s.GetOpenTradeById(tradeId)).ReturnsAsync(expected);

            var result = await _controller.GetOpenTradeById(tradeId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<TradeDetailedResponse>(ok.Value);
            Assert.Equal(tradeId, value.Id);
        }

        /// <summary>
        /// Ensures that <see cref="TradesController.CreateTrade"/> returns a <see cref="CreatedAtActionResult"/> when the trade is created successfully.
        /// </summary>
        [Fact]
        public async Task CreateTrade_Succeeds()
        {
            var request = new TradeCreateRequest
            {
                Type = "FOR_PRICE",
                CardId = Guid.NewGuid(),
                Price = 500
            };

            var expected = new TradeResponse
            {
                Id = Guid.NewGuid(),
                Type = "FOR_PRICE",
                CardId = request.CardId,
                Price = 500,
                CreatedAt = DateTime.UtcNow
            };

            _mockTradeService.Setup(s => s.CreateTrade(It.IsAny<TradeCreateRequest>())).ReturnsAsync(expected);

            var result = await _controller.CreateTrade(request);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var value = Assert.IsType<TradeResponse>(created.Value);
            Assert.Equal(expected.Id, value.Id);
        }

        /// <summary>
        /// Ensures that <see cref="TradesController.CancelTrade"/> returns a <see cref="NoContentResult"/> when cancel trade succeeds.
        /// </summary>
        [Fact]
        public async Task CancelTrade_Succeeds()
        {
            var tradeId = Guid.NewGuid();

            _mockTradeService.Setup(s => s.DeleteTrade(tradeId)).Returns(Task.CompletedTask);

            var result = await _controller.CancelTrade(tradeId);

            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// Ensures that <see cref="TradesController.ExecuteTrade"/> returns an <see cref="OkObjectResult"/> when execution succeeds.
        /// </summary>
        [Fact]
        public async Task ExecuteTrade_Succeeds()
        {
            var tradeId = Guid.NewGuid();

            var completedTrade = new CompletedTradeResponse
            {
                Id = tradeId,
                Type = "FOR_PRICE",
                Price = 1000,
                ExecutedDate = DateTime.UtcNow
            };

            var sellerReward = new RewardResponse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = "CURRENCY_FROM_TRADE",
                Amount = 1000,
                CreatedAt = DateTime.UtcNow
            };

            var buyerReward = new RewardResponse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Type = "CARD_FROM_TRADE",
                ItemId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            var mockResult = (
                CompletedTrade: completedTrade,
                SellerReward: sellerReward,
                BuyerReward: buyerReward
            );

            _mockTradeService.Setup(s => s.ExecuteTrade(tradeId, It.IsAny<TradeExecuteRequest?>())).ReturnsAsync(mockResult);

            var result = await _controller.ExecuteTrade(tradeId, new TradeExecuteRequest());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        /// <summary>
        /// Ensures that <see cref="TradesController.GetTradeHistory"/> returns an <see cref="OkObjectResult"/> with completed trades.
        /// </summary>
        [Fact]
        public async Task GetTradeHistory_Succeeds()
        {
            var expected = new TradeHistoryResponse
            {
                Trades = new List<CompletedTradeResponse>
                {
                    new CompletedTradeResponse { Id = Guid.NewGuid(), Type = "FOR_PRICE", Price = 2500 }
                },
                Total = 1
            };

            _mockTradeService.Setup(s => s.GetTradeHistory(It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(expected);

            var result = await _controller.GetTradeHistory(null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<TradeHistoryResponse>(ok.Value);
            Assert.Equal(expected.Total, value.Total);
        }

        /// <summary>
        /// Ensures that <see cref="TradesController.GetCompletedTrade"/> returns an <see cref="OkObjectResult"/> when the trade exists.
        /// </summary>
        [Fact]
        public async Task GetCompletedTrade_Succeeds()
        {
            var tradeId = Guid.NewGuid();
            var expected = new CompletedTradeResponse
            {
                Id = tradeId,
                Type = "FOR_CARD",
                Price = 0
            };

            _mockTradeService.Setup(s => s.GetCompletedTradeById(tradeId)).ReturnsAsync(expected);

            var result = await _controller.GetCompletedTrade(tradeId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<CompletedTradeResponse>(ok.Value);
            Assert.Equal(tradeId, value.Id);
        }

        // ===== FAILURES =====

        [Fact]
        public async Task GetOpenTradeById_NotFound()
        {
            var tradeId = Guid.NewGuid();

            _mockTradeService.Setup(s => s.GetOpenTradeById(tradeId)).ThrowsAsync(new KeyNotFoundException("Trade not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetOpenTradeById(tradeId));
        }

        [Fact]
        public async Task CreateTrade_BadRequest()
        {
            var request = new TradeCreateRequest { Type = "FOR_CARD", CardId = Guid.NewGuid() };

            _mockTradeService.Setup(s => s.CreateTrade(It.IsAny<TradeCreateRequest>())).ThrowsAsync(new ArgumentException("Invalid trade request."));

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.CreateTrade(request));
        }

        [Fact]
        public async Task CancelTrade_NotFound()
        {
            var tradeId = Guid.NewGuid();

            _mockTradeService.Setup(s => s.DeleteTrade(tradeId)).ThrowsAsync(new KeyNotFoundException("Trade not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.CancelTrade(tradeId));
        }

        [Fact]
        public async Task ExecuteTrade_NotFound()
        {
            var tradeId = Guid.NewGuid();

            _mockTradeService.Setup(s => s.ExecuteTrade(tradeId, It.IsAny<TradeExecuteRequest?>())).ThrowsAsync(new KeyNotFoundException("Trade not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.ExecuteTrade(tradeId, new TradeExecuteRequest()));
        }

        [Fact]
        public async Task ExecuteTrade_BadRequest()
        {
            var tradeId = Guid.NewGuid();

            _mockTradeService.Setup(s => s.ExecuteTrade(tradeId, It.IsAny<TradeExecuteRequest?>())).ThrowsAsync(new ArgumentException("Invalid execution request."));

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.ExecuteTrade(tradeId, new TradeExecuteRequest()));
        }

        [Fact]
        public async Task GetCompletedTrade_NotFound()
        {
            var tradeId = Guid.NewGuid();

            _mockTradeService.Setup(s => s.GetCompletedTradeById(tradeId)).ThrowsAsync(new KeyNotFoundException("Completed trade not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetCompletedTrade(tradeId));
        }
    }
}
