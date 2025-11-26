using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CarDexBackend.Services;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexDatabase;
using CarDexBackend.Domain.Enums;
using Npgsql;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using CarDexBackend.Services.Resources;

using CarDexBackend.Repository.Implementations;
using CarDexBackend.Repository.Interfaces;
using CarDexBackend.Domain.Entities;

namespace DefaultNamespace
{
    public class UserServiceTest : IDisposable
    {
        private readonly CarDexDbContext _context;
        private readonly UserService _userService;
        private readonly IUserRepository _userRepo;
        private readonly ICardRepository _cardRepo;
        private readonly IPackRepository _packRepo;
        private readonly IOpenTradeRepository _openTradeRepo;
        private readonly ICompletedTradeRepository _completedTradeRepo;
        private readonly IRewardRepository _rewardRepo;
        private readonly IRepository<Vehicle> _vehicleRepo;
        private readonly ICollectionRepository _collectionRepo;
        
        //Used ChatGPT to set up the base code
        public UserServiceTest()
        {
            // Use In-Memory Database for isolated testing
            var options = new DbContextOptionsBuilder<CarDexDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_UserService_" + Guid.NewGuid())
                .Options;

            _context = new CarDexDbContext(options);
            _userRepo = new UserRepository(_context);
            _cardRepo = new CardRepository(_context);
            _packRepo = new PackRepository(_context);
            _openTradeRepo = new OpenTradeRepository(_context);
            _completedTradeRepo = new CompletedTradeRepository(_context);
            _rewardRepo = new RewardRepository(_context);
            _vehicleRepo = new Repository<Vehicle>(_context);
            _collectionRepo = new CollectionRepository(_context);

            _userService = new UserService(
                _userRepo, 
                _cardRepo, 
                _packRepo, 
                _openTradeRepo, 
                _completedTradeRepo, 
                _rewardRepo, 
                _vehicleRepo, 
                _collectionRepo,
                new NullStringLocalizer<SharedResources>());
        }

        // Dispose method to clean up the DbContext
        public void Dispose()
        {
            _context.Dispose();
        }

        // Test for getting user profile by ID
        [Fact]
        public async Task GetUserProfile_ShouldReturnCorrectUser()
        {
            // Arrange: Create a test user in the database
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act: Call the service method to get the user profile
            var result = await _userService.GetUserProfile(user.Id);

            // Assert: Check that the user profile returned matches the test user
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal("TestUser", result.Username);
        }

        // Test for updating user profile
        [Fact]
        public async Task UpdateUserProfile_ShouldUpdateUserDetails()
        {
            // Arrange: Create a test user in the database
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Update request
            var updateRequest = new CarDexBackend.Shared.Dtos.Requests.UserUpdateRequest
            {
                Username = "UpdatedUser"
            };

            // Act: Call the service method to update the user profile
            var result = await _userService.UpdateUserProfile(user.Id, updateRequest);

            // Assert: Check that the user profile was updated
            Assert.NotNull(result);
            Assert.Equal("UpdatedUser", result.Username);
        }

        // Test for retrieving user cards (assuming cards exist)
        [Fact]
        public async Task GetUserCards_ShouldReturnCorrectCards()
        {
            // Arrange: Create a test user in the database
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act: Get the user's cards (empty at the moment)
            var result = await _userService.GetUserCards(user.Id, null, null, 10, 0);

            // Assert: Check the returned result
            Assert.NotNull(result);
            Assert.Equal(0, result.Total); 
        }

        // Test for retrieving user packs
        [Fact]
        public async Task GetUserPacks_ShouldReturnCorrectPacks()
        {
            // Arrange: Create a test user in the database
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act: Get the user's packs (empty at the moment)
            var result = await _userService.GetUserPacks(user.Id, null);

            // Assert: Check the returned result
            Assert.NotNull(result);
            Assert.Equal(0, result.Total); 
        }

        [Fact]
        public async Task GetUserTrades_ShouldReturnCorrectTrades()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100,
            };

            var trade = new CarDexBackend.Domain.Entities.OpenTrade
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Type = CarDexBackend.Domain.Enums.TradeEnum.FOR_CARD,
                CardId = Guid.NewGuid(),
                Price = 500,
            };

            _context.Users.Add(user);
            _context.OpenTrades.Add(trade);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserTrades(user.Id, "FOR_CARD");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Trades);
            Assert.Equal(trade.Id, result.Trades.First().Id);
            Assert.Equal(trade.Type.ToString(), result.Trades.First().Type);
            Assert.Equal(trade.Price, result.Trades.First().Price);
        }

        [Fact]
        public async Task GetUserTradeHistory_ShouldReturnCorrectHistory()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100,
            };

            var completedTrade = new CarDexBackend.Domain.Entities.CompletedTrade
            {
                Id = Guid.NewGuid(),
                SellerUserId = user.Id,
                BuyerUserId = Guid.NewGuid(),
                Price = 500,
            };

            _context.Users.Add(user);
            _context.CompletedTrades.Add(completedTrade);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserTradeHistory(user.Id, "seller", 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Trades);
            Assert.Equal(completedTrade.Id, result.Trades.First().Id);
            Assert.Equal(completedTrade.Price, result.Trades.First().Price);
            Assert.Equal(completedTrade.SellerUserId, result.Trades.First().SellerUserId);
        }

        [Fact]
        public async Task GetUserRewards_ShouldReturnCorrectRewards()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100,
                
            };

            var reward = new CarDexBackend.Domain.Entities.Reward
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Type = CarDexBackend.Domain.Enums.RewardEnum.PACK,
                ItemId = Guid.NewGuid(),
                Amount = 1,
                ClaimedAt = null 
            };

            _context.Users.Add(user);
            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserRewards(user.Id, false); 

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rewards);
            Assert.Equal(reward.Id, result.Rewards.First().Id);
            Assert.Equal(reward.Type.ToString(), result.Rewards.First().Type);
            Assert.Null(result.Rewards.First().ClaimedAt); 
        }

        [Fact]
        public async Task GetUserProfile_ShouldThrowWhenUserNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _userService.GetUserProfile(Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateUserProfile_ShouldThrowWhenUserNotFound()
        {
            // Arrange
            var request = new CarDexBackend.Shared.Dtos.Requests.UserUpdateRequest
            {
                Username = "NewUsername"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _userService.UpdateUserProfile(Guid.NewGuid(), request));
        }

        [Fact]
        public async Task UpdateUserProfile_ShouldHandleEmptyUsername()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new CarDexBackend.Shared.Dtos.Requests.UserUpdateRequest
            {
                Username = null
            };

            // Act
            var result = await _userService.UpdateUserProfile(user.Id, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestUser", result.Username); // Should remain unchanged
        }

        [Fact]
        public async Task GetUserCards_ShouldThrowWhenUserNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _userService.GetUserCards(Guid.NewGuid(), null, null, 10, 0));
        }

        [Fact]
        public async Task GetUserCards_ShouldFilterByCollectionId()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.Add(user);
            
            var collectionId = Guid.NewGuid();
            var vehicle = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2021",
                Make = "Tesla",
                Model = "Model S",
                Value = 70000
            };
            _context.Vehicles.Add(vehicle);
            
            var card = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                VehicleId = vehicle.Id,
                CollectionId = collectionId,
                Grade = GradeEnum.FACTORY,
                Value = 70000
            };
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserCards(user.Id, collectionId, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Cards);
        }

        [Fact]
        public async Task GetUserCards_ShouldFilterByGrade()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.Add(user);
            
            var vehicle = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2021",
                Make = "Tesla",
                Model = "Model S",
                Value = 70000
            };
            _context.Vehicles.Add(vehicle);
            
            var card = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                VehicleId = vehicle.Id,
                CollectionId = Guid.NewGuid(),
                Grade = GradeEnum.NISMO,
                Value = 70000
            };
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserCards(user.Id, null, "NISMO", 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Cards);
            Assert.Equal("NISMO", result.Cards.First().Grade);
        }

        [Fact]
        public async Task GetUserCards_ShouldHandlePagination()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.Add(user);
            
            var vehicle = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2021",
                Make = "Tesla",
                Model = "Model S",
                Value = 70000
            };
            _context.Vehicles.Add(vehicle);

            for (int i = 0; i < 5; i++)
            {
                var card = new CarDexBackend.Domain.Entities.Card
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    VehicleId = vehicle.Id,
                    CollectionId = Guid.NewGuid(),
                    Grade = GradeEnum.FACTORY,
                    Value = 70000
                };
                _context.Cards.Add(card);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserCards(user.Id, null, null, limit: 2, offset: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Limit);
            Assert.Equal(1, result.Offset);
            Assert.True(result.Cards.Count() <= 2);
        }

        [Fact]
        public async Task GetUserPacks_ShouldThrowWhenUserNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _userService.GetUserPacks(Guid.NewGuid(), null));
        }

        [Fact]
        public async Task GetUserPacks_ShouldFilterByCollectionId()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.Add(user);
            
            var collectionId = Guid.NewGuid();
            var pack = new CarDexBackend.Domain.Entities.Pack(
                Guid.NewGuid(), user.Id, collectionId, 500);
            _context.Packs.Add(pack);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserPacks(user.Id, collectionId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Packs);
        }

        [Fact]
        public async Task GetUserTrades_ShouldThrowWhenUserNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _userService.GetUserTrades(Guid.NewGuid(), null));
        }

        [Fact]
        public async Task GetUserTrades_ShouldFilterByType()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.Add(user);

            var cardId = Guid.NewGuid();
            var trade1 = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, cardId, 1000, null);
            var trade2 = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_CARD, user.Id, cardId, 0, Guid.NewGuid());

            _context.OpenTrades.AddRange(trade1, trade2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserTrades(user.Id, "FOR_PRICE");

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Trades, t => Assert.Equal("FOR_PRICE", t.Type));
        }

        [Fact]
        public async Task GetUserTradeHistory_ShouldFilterBySellerRole()
        {
            // Arrange
            var seller = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "Seller",
                Password = "Password123",
                Currency = 100
            };
            var buyer = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "Buyer",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.AddRange(seller, buyer);

            var trade = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, seller.Id, Guid.NewGuid(), buyer.Id, 1000, null);
            _context.CompletedTrades.Add(trade);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserTradeHistory(seller.Id, "seller", 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Trades);
            Assert.Equal(seller.Id, result.Trades.First().SellerUserId);
        }

        [Fact]
        public async Task GetUserTradeHistory_ShouldFilterByBuyerRole()
        {
            // Arrange
            var seller = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "Seller",
                Password = "Password123",
                Currency = 100
            };
            var buyer = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "Buyer",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.AddRange(seller, buyer);

            var trade = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, seller.Id, Guid.NewGuid(), buyer.Id, 1000, null);
            _context.CompletedTrades.Add(trade);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserTradeHistory(buyer.Id, "buyer", 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Trades);
            Assert.Equal(buyer.Id, result.Trades.First().BuyerUserId);
        }

        [Fact]
        public async Task GetUserTradeHistory_ShouldHandleAllRole()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "User",
                Password = "Password123",
                Currency = 100
            };
            var otherUser = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "Other",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.AddRange(user, otherUser);

            var trade1 = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, Guid.NewGuid(), otherUser.Id, 1000, null);
            var trade2 = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, otherUser.Id, Guid.NewGuid(), user.Id, 2000, null);

            _context.CompletedTrades.AddRange(trade1, trade2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserTradeHistory(user.Id, "all", 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Trades.Count());
        }

        [Fact]
        public async Task GetUserTradeHistory_ShouldHandlePagination()
        {
            // Arrange
            var seller = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "Seller",
                Password = "Password123",
                Currency = 100
            };
            var buyer = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "Buyer",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.AddRange(seller, buyer);

            for (int i = 0; i < 5; i++)
            {
                var trade = new CarDexBackend.Domain.Entities.CompletedTrade(
                    Guid.NewGuid(), TradeEnum.FOR_PRICE, seller.Id, Guid.NewGuid(), buyer.Id, 1000 + i, null);
                _context.CompletedTrades.Add(trade);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserTradeHistory(seller.Id, "seller", limit: 2, offset: 1);

            // Assert
            Assert.Equal(2, result.Limit);
            Assert.Equal(1, result.Offset);
            Assert.True(result.Trades.Count() <= 2);
        }

        [Fact]
        public async Task GetUserRewards_ShouldThrowWhenUserNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _userService.GetUserRewards(Guid.NewGuid(), false));
        }

        [Fact]
        public async Task GetUserRewards_ShouldFilterByClaimedStatus()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser",
                Password = "Password123",
                Currency = 100
            };
            _context.Users.Add(user);

            var claimedReward = new CarDexBackend.Domain.Entities.Reward
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Type = RewardEnum.PACK,
                ItemId = Guid.NewGuid(),
                Amount = 1,
                ClaimedAt = DateTime.UtcNow
            };
            var unclaimedReward = new CarDexBackend.Domain.Entities.Reward
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Type = RewardEnum.CURRENCY,
                ItemId = null,
                Amount = 100,
                ClaimedAt = null
            };

            _context.Rewards.AddRange(claimedReward, unclaimedReward);
            await _context.SaveChangesAsync();

            // Act - Get claimed rewards
            var claimedResult = await _userService.GetUserRewards(user.Id, true);

            // Assert
            Assert.NotNull(claimedResult);
            Assert.Single(claimedResult.Rewards);
            Assert.NotNull(claimedResult.Rewards.First().ClaimedAt);
        }
    }
}
