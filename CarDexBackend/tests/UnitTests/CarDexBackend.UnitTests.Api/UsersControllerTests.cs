using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CarDexBackend.Controllers;
using CarDexBackend.Services;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarDexBackend.UnitTests.Api.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="UsersController"/>.
    /// </summary>
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }

        /// <summary>
        /// Ensures that GetUserProfile returns 200 OK when the user exists.
        /// </summary>
        [Fact]
        public async Task GetUserProfile_Succeeds()
        {
            var userId = Guid.NewGuid();
            var mockResponse = new UserPublicResponse { Id = userId, Username = "Player1" };

            _mockUserService.Setup(s => s.GetUserProfile(userId)).ReturnsAsync(mockResponse);

            var result = await _controller.GetUserProfile(userId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserPublicResponse>(ok.Value);
            Assert.Equal(userId, value.Id);
        }

        /// <summary>
        /// Ensures that GetUserProfile returns 404 NotFound when the user does not exist.
        /// </summary>
        [Fact]
        public async Task GetUserProfile_NotFound()
        {
            var userId = Guid.NewGuid();
            _mockUserService.Setup(s => s.GetUserProfile(userId)).ThrowsAsync(new KeyNotFoundException("User not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetUserProfile(userId));
        }

        /// <summary>
        /// Ensures that UpdateUserProfile returns 200 OK when update succeeds.
        /// </summary>
        [Fact]
        public async Task UpdateUserProfile_Succeeds()
        {
            var userId = Guid.NewGuid();
            var request = new UserUpdateRequest { Username = "UpdatedUser" };
            var response = new UserResponse { Id = userId, Username = "UpdatedUser" };

            _mockUserService.Setup(s => s.UpdateUserProfile(userId, request)).ReturnsAsync(response);

            var result = await _controller.UpdateUserProfile(userId, request);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserResponse>(ok.Value);
            Assert.Equal("UpdatedUser", value.Username);
        }

        /// <summary>
        /// Ensures that UpdateUserProfile returns 404 NotFound when user is missing.
        /// </summary>
        [Fact]
        public async Task UpdateUserProfile_NotFound()
        {
            var userId = Guid.NewGuid();
            var request = new UserUpdateRequest { Username = "GhostUser" };

            _mockUserService.Setup(s => s.UpdateUserProfile(userId, request)).ThrowsAsync(new KeyNotFoundException("User not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateUserProfile(userId, request));
        }

        /// <summary>
        /// Ensures GetUserCards returns 200 OK with data.
        /// </summary>
        [Fact]
        public async Task GetUserCards_Succeeds()
        {
            var userId = Guid.NewGuid();
            var mockCards = new UserCardListResponse
            {
                Total = 2,
                Cards = new List<UserCardResponse>
                {
                    new() { Id = Guid.NewGuid(), Grade = "FACTORY", Value = 100 },
                    new() { Id = Guid.NewGuid(), Grade = "NISMO", Value = 500 }
                }
            };

            _mockUserService.Setup(s => s.GetUserCards(userId, null, null, 50, 0)).ReturnsAsync(mockCards);

            var result = await _controller.GetUserCards(userId, null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserCardListResponse>(ok.Value);
            Assert.Equal(2, value.Total);
        }

        /// <summary>
        /// Ensures GetUserPacks returns 200 OK with data.
        /// </summary>
        [Fact]
        public async Task GetUserPacks_Succeeds()
        {
            var userId = Guid.NewGuid();
            var mockPacks = new UserPackListResponse
            {
                Total = 1,
                Packs = new List<UserPackResponse> { new() { Id = Guid.NewGuid(), Value = 200 } }
            };

            _mockUserService.Setup(s => s.GetUserPacks(userId, null)).ReturnsAsync(mockPacks);

            var result = await _controller.GetUserPacks(userId, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserPackListResponse>(ok.Value);
            Assert.Equal(1, value.Total);
        }

        /// <summary>
        /// Ensures GetUserTrades returns 200 OK with data.
        /// </summary>
        [Fact]
        public async Task GetUserTrades_Succeeds()
        {
            var userId = Guid.NewGuid();
            var mockTrades = new UserTradeListResponse
            {
                Total = 1,
                Trades = new List<UserTradeResponse> { new() { Id = Guid.NewGuid(), Type = "FOR_CARD" } }
            };

            _mockUserService.Setup(s => s.GetUserTrades(userId, null)).ReturnsAsync(mockTrades);

            var result = await _controller.GetUserTrades(userId, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserTradeListResponse>(ok.Value);
            Assert.Equal(1, value.Total);
        }

        /// <summary>
        /// Ensures GetUserTradeHistory returns 200 OK with data.
        /// </summary>
        [Fact]
        public async Task GetUserTradeHistory_Succeeds()
        {
            var userId = Guid.NewGuid();
            var mockHistory = new UserTradeHistoryListResponse
            {
                Total = 3,
                Trades = new List<UserTradeHistoryResponse>
                {
                    new() { Id = Guid.NewGuid(), Type = "FOR_PRICE", Price = 500 }
                }
            };

            _mockUserService.Setup(s => s.GetUserTradeHistory(userId, "all", 50, 0)).ReturnsAsync(mockHistory);

            var result = await _controller.GetUserTradeHistory(userId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserTradeHistoryListResponse>(ok.Value);
            Assert.Equal(3, value.Total);
        }

        /// <summary>
        /// Ensures GetUserRewards returns 200 OK with data.
        /// </summary>
        [Fact]
        public async Task GetUserRewards_Succeeds()
        {
            var userId = Guid.NewGuid();
            var mockRewards = new UserRewardListResponse
            {
                Total = 2,
                Rewards = new List<UserRewardResponse>
                {
                    new() { Id = Guid.NewGuid(), Type = "PACK" },
                    new() { Id = Guid.NewGuid(), Type = "CURRENCY" }
                }
            };

            _mockUserService.Setup(s => s.GetUserRewards(userId, false)).ReturnsAsync(mockRewards);

            var result = await _controller.GetUserRewards(userId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserRewardListResponse>(ok.Value);
            Assert.Equal(2, value.Total);
        }

        /// <summary>
        /// Ensures GetUserCardsWithVehicles returns 200 OK with cards including vehicle details.
        /// </summary>
        [Fact]
        public async Task GetUserCardsWithVehicles_Succeeds()
        {
            var userId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            var collectionId = Guid.NewGuid();
            
            var mockResponse = new UserCardWithVehicleListResponse
            {
                Cards = new List<UserCardWithVehicleResponse>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        VehicleId = vehicleId,
                        CollectionId = collectionId,
                        Grade = "NISMO",
                        Value = 50000,
                        Year = "1999",
                        Make = "Nissan",
                        Model = "Skyline GT-R",
                        Stat1 = 276,
                        Stat2 = 165,
                        Stat3 = 1560,
                        VehicleImage = "/images/gtr.jpg"
                    }
                },
                Total = 1,
                Limit = 50,
                Offset = 0
            };

            _mockUserService
                .Setup(s => s.GetUserCardsWithVehicles(userId, null, null, 50, 0))
                .ReturnsAsync(mockResponse);

            var result = await _controller.GetUserCardsWithVehicles(userId, null, null, 50, 0);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<UserCardWithVehicleListResponse>(ok.Value);
            Assert.Single(value.Cards);
            Assert.Equal("Nissan", value.Cards.First().Make);
            Assert.Equal("Skyline GT-R", value.Cards.First().Model);
        }

        /// <summary>
        /// Ensures GetUserCardsWithVehicles returns 404 when user not found.
        /// </summary>
        [Fact]
        public async Task GetUserCardsWithVehicles_NotFound()
        {
            var userId = Guid.NewGuid();
            
            _mockUserService
                .Setup(s => s.GetUserCardsWithVehicles(userId, null, null, 50, 0))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _controller.GetUserCardsWithVehicles(userId, null, null, 50, 0));
        }

        /// <summary>
        /// Ensures GetCollectionProgress returns 200 OK with progress data.
        /// </summary>
        [Fact]
        public async Task GetCollectionProgress_Succeeds()
        {
            var userId = Guid.NewGuid();
            var collectionId = Guid.NewGuid();
            
            var mockResponse = new CollectionProgressResponse
            {
                Collections = new List<CollectionProgressDto>
                {
                    new()
                    {
                        CollectionId = collectionId,
                        CollectionName = "JDM Legends",
                        CollectionImage = "/images/collections/jdm.jpg",
                        OwnedVehicles = 5,
                        TotalVehicles = 10,
                        Percentage = 50
                    }
                },
                TotalCollections = 1
            };

            _mockUserService
                .Setup(s => s.GetCollectionProgress(userId))
                .ReturnsAsync(mockResponse);

            var result = await _controller.GetCollectionProgress(userId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<CollectionProgressResponse>(ok.Value);
            Assert.Single(value.Collections);
            Assert.Equal(50, value.Collections.First().Percentage);
            Assert.Equal("JDM Legends", value.Collections.First().CollectionName);
        }

        /// <summary>
        /// Ensures GetCollectionProgress returns empty list when user has no cards.
        /// </summary>
        [Fact]
        public async Task GetCollectionProgress_EmptyWhenNoCards()
        {
            var userId = Guid.NewGuid();
            
            var mockResponse = new CollectionProgressResponse
            {
                Collections = new List<CollectionProgressDto>(),
                TotalCollections = 0
            };

            _mockUserService
                .Setup(s => s.GetCollectionProgress(userId))
                .ReturnsAsync(mockResponse);

            var result = await _controller.GetCollectionProgress(userId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<CollectionProgressResponse>(ok.Value);
            Assert.Empty(value.Collections);
            Assert.Equal(0, value.TotalCollections);
        }

        /// <summary>
        /// Ensures GetCollectionProgress returns 404 when user not found.
        /// </summary>
        [Fact]
        public async Task GetCollectionProgress_NotFound()
        {
            var userId = Guid.NewGuid();
            
            _mockUserService
                .Setup(s => s.GetCollectionProgress(userId))
                .ThrowsAsync(new KeyNotFoundException("User not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _controller.GetCollectionProgress(userId));
        }
    }
}
