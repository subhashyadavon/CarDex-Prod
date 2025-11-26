using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CarDexBackend.Services;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Shared.Dtos.Requests;
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
    public class TradeServiceTest : IDisposable
    {
        private readonly CarDexDbContext _context;
        private readonly TradeService _tradeService;
        private readonly IOpenTradeRepository _openTradeRepo;
        private readonly ICompletedTradeRepository _completedTradeRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICardRepository _cardRepo;
        private readonly IRepository<Vehicle> _vehicleRepo;
        private readonly IRewardRepository _rewardRepo;

        //Used ChatGPT to get the base code and get help seeding the data, and to write the test for GetOpenTrade with filters.
        public TradeServiceTest()
        {
            // Use In-Memory Database for isolated testing
            var options = new DbContextOptionsBuilder<CarDexDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_TradeService_" + Guid.NewGuid())
                .Options;

            _context = new CarDexDbContext(options);
            _openTradeRepo = new OpenTradeRepository(_context);
            _completedTradeRepo = new CompletedTradeRepository(_context);
            _userRepo = new UserRepository(_context);
            _cardRepo = new CardRepository(_context);
            _vehicleRepo = new Repository<Vehicle>(_context);
            _rewardRepo = new RewardRepository(_context);

            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var currentUserService = new TestCurrentUserService { UserId = testUserId };

            _tradeService = new TradeService(
                _openTradeRepo, 
                _completedTradeRepo, 
                _userRepo, 
                _cardRepo, 
                _vehicleRepo, 
                _rewardRepo, 
                currentUserService,
                new NullStringLocalizer<SharedResources>());

            // Seed test data
            SeedTestData();
        }

        // Dispose method to clean up the DbContext after each test
        public void Dispose()
        {
            _context.Dispose();
        }

        private void SeedTestData()
        {
            // Add test collections 
            var collection1 = new CarDexBackend.Domain.Entities.Collection
            {
                Id = Guid.NewGuid(),
                Name = "Collection 1",
                Vehicles = new Guid[] { Guid.NewGuid(), Guid.NewGuid() },  
                PackPrice = 500
            };

            var collection2 = new CarDexBackend.Domain.Entities.Collection
            {
                Id = Guid.NewGuid(),
                Name = "Collection 2",
                Vehicles = new Guid[] { Guid.NewGuid() },  
                PackPrice = 300
            };

            _context.Collections.Add(collection1);
            _context.Collections.Add(collection2);
            _context.SaveChanges();

            // Add test users
            var user1 = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser1",
                Password = "Password123",
                Currency = 1000 
            };

            var user2 = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "TestUser2",
                Password = "Password456",
                Currency = 500 
            };

            _context.Users.Add(user1);
            _context.Users.Add(user2);
            _context.SaveChanges();

            // Add test vehicles
            var vehicle1 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(), 
                Year = "2021",
                Make = "Tesla",
                Model = "Model S",
                Value = 70000
            };

            var vehicle2 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2020",
                Make = "Ford",
                Model = "Mustang",
                Value = 50000
            };

            var vehicle3 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2022",
                Make = "Chevrolet",
                Model = "Camaro",
                Value = 60000
            };

            _context.Vehicles.Add(vehicle1);
            _context.Vehicles.Add(vehicle2);
            _context.Vehicles.Add(vehicle3);
            _context.SaveChanges();

            // Add test cards
            var card1 = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = user1.Id,
                VehicleId = vehicle1.Id,
                CollectionId = collection1.Id,
                Grade = GradeEnum.FACTORY,
                Value = 70000
            };

            var card2 = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = user2.Id,
                VehicleId = vehicle2.Id,
                CollectionId = collection2.Id,
                Grade = GradeEnum.LIMITED_RUN,
                Value = 50000
            };

            _context.Cards.Add(card1);
            _context.Cards.Add(card2);
            _context.SaveChanges();
            
            // Add card for test service user (used by CreateTrade test)
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var testUserCard = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = testUserId,
                VehicleId = vehicle3.Id,
                CollectionId = collection1.Id,
                Grade = GradeEnum.FACTORY,
                Value = 60000
            };
            _context.Cards.Add(testUserCard);
            _context.SaveChanges();
        }

        // Test for CreateTrade
        [Fact]
        public async Task CreateTrade_ShouldCreateTradeSuccessfully()
        {
            // Arrange - Create a card owned by the test user (TradeService uses hardcoded test user)
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            
            // Create or get the test user
            var testUser = _context.Users.FirstOrDefault(u => u.Id == testUserId);
            if (testUser == null)
            {
                testUser = new CarDexBackend.Domain.Entities.User
                {
                    Id = testUserId,
                    Username = "TestServiceUser",
                    Password = "Password123",
                    Currency = 1000
                };
                _context.Users.Add(testUser);
                _context.SaveChanges();
            }
            
            // Get a card owned by the test user
            var card = _context.Cards.First(c => c.UserId == testUserId);
            
            var request = new TradeCreateRequest
            {
                CardId = card.Id,
                Type = "FOR_PRICE", 
                Price = 1000 
            };

            // Act
            var result = await _tradeService.CreateTrade(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(card.Id, result.CardId); 
            Assert.Equal(request.Price, result.Price); 
            Assert.Equal(testUser.Username, result.Username); 
        }

        // Test for GetOpenTrades with filters
        [Fact]
        public async Task GetOpenTrades_ShouldReturnFilteredOpenTrades()
        {
            // Arrange - Create an open trade first
            var user = _context.Users.First();
            
            // Find a FACTORY card to use
            var factoryCard = _context.Cards.FirstOrDefault(c => c.UserId == user.Id && c.Grade == GradeEnum.FACTORY);
            if (factoryCard == null)
            {
                // Create a FACTORY card if none exists
                var vehicle = _context.Vehicles.First();
                var collection = _context.Collections.First();
                factoryCard = new CarDexBackend.Domain.Entities.Card
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    VehicleId = vehicle.Id,
                    CollectionId = collection.Id,
                    Grade = GradeEnum.FACTORY,
                    Value = 50000
                };
                _context.Cards.Add(factoryCard);
                _context.SaveChanges();
            }
            
            var collectionId = factoryCard.CollectionId;
            
            var openTrade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(),
                TradeEnum.FOR_PRICE,
                user.Id,
                factoryCard.Id,
                1000,
                null
            );
            _context.OpenTrades.Add(openTrade);
            _context.SaveChanges();
            
            var grade = "FACTORY"; 
            var minPrice = 500;

            // Act
            var result = await _tradeService.GetOpenTrades(
                type: "FOR_PRICE", 
                collectionId: collectionId, 
                grade: grade, 
                minPrice: minPrice, 
                maxPrice: null, 
                vehicleId: null, 
                wantCardId: null, 
                sortBy: "price_asc", 
                limit: 10, 
                offset: 0
            );

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Trades); 
            Assert.True(result.Trades.All(t => t.Price >= minPrice)); 
            Assert.Equal(10, result.Limit); 
        }

        // Test for GetOpenTradeById
        [Fact]
        public async Task GetOpenTradeById_ShouldReturnCorrectTradeDetails()
        {
            // Arrange - Create an open trade
            var user = _context.Users.First();
            var card = _context.Cards.First(c => c.UserId == user.Id);
            
            var openTrade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(),
                TradeEnum.FOR_PRICE,
                user.Id,
                card.Id,
                1000,
                null
            );
            _context.OpenTrades.Add(openTrade);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTradeById(openTrade.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(openTrade.Id, result.Id);
            Assert.Equal(user.Username, result.Username); 
        }

        // Test for ExecuteTrade
        [Fact]
        public async Task ExecuteTrade_ShouldCompleteTheTradeAndTransferCurrency()
        {
            // Arrange - Create buyer (test user) with enough currency
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var buyer = _context.Users.FirstOrDefault(u => u.Id == testUserId);
            if (buyer == null)
            {
                buyer = new CarDexBackend.Domain.Entities.User
                {
                    Id = testUserId,
                    Username = "BuyerUser",
                    Password = "Password123",
                    Currency = 5000 // Enough to buy
                };
                _context.Users.Add(buyer);
                _context.SaveChanges();
            }
            else
            {
                buyer.Currency = 5000; // Ensure enough currency
                _context.SaveChanges();
            }
            
            // Create seller and their card
            var seller = _context.Users.First(u => u.Id != testUserId);
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);
            
            // Create open trade
            var openTrade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(),
                TradeEnum.FOR_PRICE,
                seller.Id,
                sellerCard.Id,
                1000,
                null
            );
            _context.OpenTrades.Add(openTrade);
            _context.SaveChanges();
            
            var request = new TradeExecuteRequest
            {
                BuyerCardId = null // FOR_PRICE trade doesn't need buyer card
            };

            // Act
            var result = await _tradeService.ExecuteTrade(openTrade.Id, request);

            // Assert
            Assert.NotNull(result.CompletedTrade);
            Assert.True(result.CompletedTrade.Price > 0); 
            Assert.Equal(seller.Id, result.CompletedTrade.SellerUserId); 
            Assert.Equal(buyer.Id, result.CompletedTrade.BuyerUserId); 
        }

        // Additional tests for comprehensive coverage

        [Fact]
        public async Task CreateTrade_ShouldThrowWhenCardNotFound()
        {
            // Arrange
            var request = new TradeCreateRequest
            {
                CardId = Guid.NewGuid(), // Non-existent card
                Type = "FOR_PRICE",
                Price = 1000
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _tradeService.CreateTrade(request));
        }

        [Fact]
        public async Task CreateTrade_ShouldThrowWhenCardNotOwnedByUser()
        {
            // Arrange - Use a card owned by another user
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var otherUserCard = _context.Cards.First(c => c.UserId != testUserId);

            var request = new TradeCreateRequest
            {
                CardId = otherUserCard.Id,
                Type = "FOR_PRICE",
                Price = 1000
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _tradeService.CreateTrade(request));
        }

        [Fact]
        public async Task CreateTrade_ShouldThrowWhenPriceMissingForPriceTrade()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var card = _context.Cards.First(c => c.UserId == testUserId);

            var request = new TradeCreateRequest
            {
                CardId = card.Id,
                Type = "FOR_PRICE",
                Price = null // Missing price
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _tradeService.CreateTrade(request));
        }

        [Fact]
        public async Task CreateTrade_ShouldCreateCardTradeSuccessfully()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var testUser = _context.Users.FirstOrDefault(u => u.Id == testUserId);
            if (testUser == null)
            {
                testUser = new CarDexBackend.Domain.Entities.User
                {
                    Id = testUserId,
                    Username = "TestServiceUser",
                    Password = "Password123",
                    Currency = 1000
                };
                _context.Users.Add(testUser);
                _context.SaveChanges();
            }

            var card = _context.Cards.First(c => c.UserId == testUserId);
            var wantCard = _context.Cards.First(c => c.UserId != testUserId);

            var request = new TradeCreateRequest
            {
                CardId = card.Id,
                Type = "FOR_CARD",
                WantCardId = wantCard.Id
            };

            // Act
            var result = await _tradeService.CreateTrade(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(card.Id, result.CardId);
            Assert.Equal(wantCard.Id, result.WantCardId);
            Assert.Null(result.Price);
        }

        [Fact]
        public async Task CreateTrade_ShouldThrowWhenWantCardIdMissingForCardTrade()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var card = _context.Cards.First(c => c.UserId == testUserId);

            var request = new TradeCreateRequest
            {
                CardId = card.Id,
                Type = "FOR_CARD",
                WantCardId = null // Missing wantCardId
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _tradeService.CreateTrade(request));
        }

        [Fact]
        public async Task GetOpenTrades_ShouldFilterByType()
        {
            // Arrange
            var user = _context.Users.First();
            var card = _context.Cards.First(c => c.UserId == user.Id);

            var priceTrade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 1000, null);
            var cardTrade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_CARD, user.Id, card.Id, 0, Guid.NewGuid());

            _context.OpenTrades.Add(priceTrade);
            _context.OpenTrades.Add(cardTrade);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTrades(
                type: "FOR_PRICE", null, null, null, null, null, null, null, 10, 0);

            // Assert
            Assert.All(result.Trades, t => Assert.Equal("FOR_PRICE", t.Type));
        }

        [Fact]
        public async Task GetOpenTrades_ShouldFilterByMaxPrice()
        {
            // Arrange
            var user = _context.Users.First();
            var card = _context.Cards.First(c => c.UserId == user.Id);

            var trade1 = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 500, null);
            var trade2 = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 1500, null);

            _context.OpenTrades.Add(trade1);
            _context.OpenTrades.Add(trade2);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTrades(
                null, null, null, null, maxPrice: 1000, null, null, null, 10, 0);

            // Assert
            Assert.All(result.Trades, t => Assert.True(t.Price <= 1000));
        }

        [Fact]
        public async Task GetOpenTrades_ShouldFilterByVehicleId()
        {
            // Arrange
            var user = _context.Users.First();
            var vehicle = _context.Vehicles.First();
            var card = _context.Cards.First(c => c.VehicleId == vehicle.Id);

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 1000, null);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTrades(
                null, null, null, null, null, vehicleId: vehicle.Id, null, null, 10, 0);

            // Assert
            Assert.NotEmpty(result.Trades);
            Assert.All(result.Trades, t => Assert.Equal(card.Id, t.CardId));
        }

        [Fact]
        public async Task GetOpenTrades_ShouldFilterByWantCardId()
        {
            // Arrange
            var user = _context.Users.First();
            var card = _context.Cards.First(c => c.UserId == user.Id);
            var wantCard = _context.Cards.First(c => c.Id != card.Id);

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_CARD, user.Id, card.Id, 0, wantCard.Id);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTrades(
                null, null, null, null, null, null, wantCardId: wantCard.Id, null, 10, 0);

            // Assert
            Assert.NotEmpty(result.Trades);
            Assert.All(result.Trades, t => Assert.Equal(wantCard.Id, t.WantCardId));
        }

        [Fact]
        public async Task GetOpenTrades_ShouldSortByPriceDesc()
        {
            // Arrange
            var user = _context.Users.First();
            var card = _context.Cards.First(c => c.UserId == user.Id);

            var trade1 = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 500, null);
            var trade2 = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 1000, null);
            var trade3 = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 750, null);

            _context.OpenTrades.AddRange(trade1, trade2, trade3);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTrades(
                null, null, null, null, null, null, null, "price_desc", 10, 0);

            // Assert
            Assert.True(result.Trades.Count() >= 3);
            var prices = result.Trades.Where(t => t.Price.HasValue).Select(t => t.Price!.Value).ToList();
            Assert.Equal(prices.OrderByDescending(p => p), prices);
        }

        [Fact]
        public async Task GetOpenTrades_ShouldSortByDateAsc()
        {
            // Arrange
            var user = _context.Users.First();
            var card = _context.Cards.First(c => c.UserId == user.Id);

            var trade1 = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 1000, null);
            var trade2 = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 2000, null);

            _context.OpenTrades.AddRange(trade1, trade2);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTrades(
                null, null, null, null, null, null, null, "date_asc", 10, 0);

            // Assert
            Assert.NotEmpty(result.Trades);
        }

        [Fact]
        public async Task GetOpenTrades_ShouldHandlePagination()
        {
            // Arrange
            var user = _context.Users.First();
            var card = _context.Cards.First(c => c.UserId == user.Id);

            for (int i = 0; i < 5; i++)
            {
                var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                    Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 1000 + i, null);
                _context.OpenTrades.Add(trade);
            }
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTrades(
                null, null, null, null, null, null, null, null, limit: 2, offset: 1);

            // Assert
            Assert.Equal(2, result.Limit);
            Assert.Equal(1, result.Offset);
            Assert.True(result.Trades.Count() <= 2);
        }

        [Fact]
        public async Task GetOpenTradeById_ShouldThrowWhenTradeNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _tradeService.GetOpenTradeById(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetOpenTradeById_ShouldReturnTradeWithWantCard()
        {
            // Arrange
            var user = _context.Users.First();
            var card = _context.Cards.First(c => c.UserId == user.Id);
            var wantCard = _context.Cards.First(c => c.Id != card.Id);

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_CARD, user.Id, card.Id, 0, wantCard.Id);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTradeById(trade.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.WantCard);
            Assert.Equal(wantCard.Id, result.WantCard.Id);
        }

        [Fact]
        public async Task GetOpenTradeById_ShouldHandleMissingVehicle()
        {
            // Arrange
            var user = _context.Users.First();
            var card = _context.Cards.First(c => c.UserId == user.Id);
            card.VehicleId = Guid.NewGuid(); // Non-existent vehicle
            _context.SaveChanges();

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, user.Id, card.Id, 1000, null);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetOpenTradeById(trade.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ExecuteTrade_ShouldThrowWhenTradeNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _tradeService.ExecuteTrade(Guid.NewGuid(), null));
        }

        [Fact]
        public async Task ExecuteTrade_ShouldThrowWhenTradingWithSelf()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var testUser = _context.Users.FirstOrDefault(u => u.Id == testUserId);
            if (testUser == null)
            {
                testUser = new CarDexBackend.Domain.Entities.User
                {
                    Id = testUserId,
                    Username = "TestUser",
                    Password = "Password123",
                    Currency = 1000
                };
                _context.Users.Add(testUser);
                _context.SaveChanges();
            }

            var card = _context.Cards.First(c => c.UserId == testUserId);
            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, testUserId, card.Id, 500, null);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tradeService.ExecuteTrade(trade.Id, null));
        }

        [Fact]
        public async Task ExecuteTrade_ShouldThrowWhenInsufficientCurrency()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var buyer = _context.Users.FirstOrDefault(u => u.Id == testUserId);
            if (buyer == null)
            {
                buyer = new CarDexBackend.Domain.Entities.User
                {
                    Id = testUserId,
                    Username = "Buyer",
                    Password = "Password123",
                    Currency = 100 // Not enough
                };
                _context.Users.Add(buyer);
            }
            else
            {
                buyer.Currency = 100;
            }
            _context.SaveChanges();

            var seller = _context.Users.First(u => u.Id != testUserId);
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, seller.Id, sellerCard.Id, 1000, null);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tradeService.ExecuteTrade(trade.Id, null));
        }

        [Fact]
        public async Task ExecuteTrade_ShouldExecuteCardTrade()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var buyer = _context.Users.FirstOrDefault(u => u.Id == testUserId);
            if (buyer == null)
            {
                buyer = new CarDexBackend.Domain.Entities.User
                {
                    Id = testUserId,
                    Username = "Buyer",
                    Password = "Password123",
                    Currency = 1000
                };
                _context.Users.Add(buyer);
                _context.SaveChanges();
            }

            var seller = _context.Users.First(u => u.Id != testUserId);
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);
            
            // Create buyer card
            var vehicle = _context.Vehicles.First();
            var collection = _context.Collections.First();
            var buyerCard = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = buyer.Id,
                VehicleId = vehicle.Id,
                CollectionId = collection.Id,
                Grade = GradeEnum.FACTORY,
                Value = 5000
            };
            _context.Cards.Add(buyerCard);
            _context.SaveChanges();

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_CARD, seller.Id, sellerCard.Id, 0, buyerCard.Id);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            var request = new TradeExecuteRequest { BuyerCardId = buyerCard.Id };

            // Act
            var result = await _tradeService.ExecuteTrade(trade.Id, request);

            // Assert
            Assert.NotNull(result.CompletedTrade);
            Assert.Equal(0, result.CompletedTrade.Price); // Price is 0 for card trades
            Assert.Equal(buyerCard.Id, result.CompletedTrade.BuyerCardId);
            Assert.NotNull(result.SellerReward);
            Assert.NotNull(result.BuyerReward);
        }

        [Fact]
        public async Task ExecuteTrade_ShouldThrowWhenBuyerCardMissingForCardTrade()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var buyer = _context.Users.FirstOrDefault(u => u.Id == testUserId);
            if (buyer == null)
            {
                buyer = new CarDexBackend.Domain.Entities.User
                {
                    Id = testUserId,
                    Username = "Buyer",
                    Password = "Password123",
                    Currency = 1000
                };
                _context.Users.Add(buyer);
                _context.SaveChanges();
            }

            var seller = _context.Users.First(u => u.Id != testUserId);
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_CARD, seller.Id, sellerCard.Id, 0, Guid.NewGuid());
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _tradeService.ExecuteTrade(trade.Id, null));
        }

        [Fact]
        public async Task ExecuteTrade_ShouldThrowWhenBuyerCardNotFound()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var buyer = _context.Users.FirstOrDefault(u => u.Id == testUserId);
            if (buyer == null)
            {
                buyer = new CarDexBackend.Domain.Entities.User
                {
                    Id = testUserId,
                    Username = "Buyer",
                    Password = "Password123",
                    Currency = 1000
                };
                _context.Users.Add(buyer);
                _context.SaveChanges();
            }

            var seller = _context.Users.First(u => u.Id != testUserId);
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_CARD, seller.Id, sellerCard.Id, 0, Guid.NewGuid());
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            var request = new TradeExecuteRequest { BuyerCardId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _tradeService.ExecuteTrade(trade.Id, request));
        }

        [Fact]
        public async Task ExecuteTrade_ShouldThrowWhenBuyerCardNotOwned()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var seller = _context.Users.First(u => u.Id != testUserId);
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);
            var otherUserCard = _context.Cards.First(c => c.UserId != testUserId && c.UserId != seller.Id);

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_CARD, seller.Id, sellerCard.Id, 0, otherUserCard.Id);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            var request = new TradeExecuteRequest { BuyerCardId = otherUserCard.Id };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tradeService.ExecuteTrade(trade.Id, request));
        }

        [Fact]
        public async Task DeleteTrade_ShouldDeleteSuccessfully()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var testUser = _context.Users.FirstOrDefault(u => u.Id == testUserId);
            if (testUser == null)
            {
                testUser = new CarDexBackend.Domain.Entities.User
                {
                    Id = testUserId,
                    Username = "TestUser",
                    Password = "Password123",
                    Currency = 1000
                };
                _context.Users.Add(testUser);
                _context.SaveChanges();
            }

            var card = _context.Cards.First(c => c.UserId == testUserId);
            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, testUserId, card.Id, 1000, null);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            // Act
            await _tradeService.DeleteTrade(trade.Id);

            // Assert
            var deletedTrade = await _context.OpenTrades.FindAsync(trade.Id);
            Assert.Null(deletedTrade);
        }

        [Fact]
        public async Task DeleteTrade_ShouldThrowWhenTradeNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _tradeService.DeleteTrade(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteTrade_ShouldThrowWhenNotOwned()
        {
            // Arrange
            var otherUser = _context.Users.First(u => u.Id != Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11"));
            var card = _context.Cards.First(c => c.UserId == otherUser.Id);
            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, otherUser.Id, card.Id, 1000, null);
            _context.OpenTrades.Add(trade);
            _context.SaveChanges();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _tradeService.DeleteTrade(trade.Id));
        }

        [Fact]
        public async Task GetCompletedTradeById_ShouldReturnCompletedTrade()
        {
            // Arrange
            var seller = _context.Users.First();
            var buyer = _context.Users.Skip(1).First();
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);

            var completedTrade = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, seller.Id, sellerCard.Id, buyer.Id, 1000, null);
            _context.CompletedTrades.Add(completedTrade);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetCompletedTradeById(completedTrade.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(completedTrade.Id, result.Id);
            Assert.Equal(seller.Username, result.SellerUsername);
            Assert.Equal(buyer.Username, result.BuyerUsername);
        }

        [Fact]
        public async Task GetCompletedTradeById_ShouldThrowWhenNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _tradeService.GetCompletedTradeById(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetCompletedTradeById_ShouldHandleMissingUsers()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var buyerId = Guid.NewGuid();
            var cardId = Guid.NewGuid();

            var completedTrade = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, sellerId, cardId, buyerId, 1000, null);
            _context.CompletedTrades.Add(completedTrade);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetCompletedTradeById(completedTrade.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetTradeHistory_ShouldReturnAllTrades()
        {
            // Arrange
            var seller = _context.Users.First();
            var buyer = _context.Users.Skip(1).First();
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);

            var trade1 = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, seller.Id, sellerCard.Id, buyer.Id, 1000, null);
            var trade2 = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, buyer.Id, sellerCard.Id, seller.Id, 2000, null);

            _context.CompletedTrades.AddRange(trade1, trade2);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetTradeHistory(null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Trades.Count() >= 2);
            Assert.Equal(10, result.Limit);
            Assert.Equal(0, result.Offset);
        }

        [Fact]
        public async Task GetTradeHistory_ShouldFilterByUserId()
        {
            // Arrange
            var seller = _context.Users.First();
            var buyer = _context.Users.Skip(1).First();
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);

            var trade1 = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, seller.Id, sellerCard.Id, buyer.Id, 1000, null);
            var trade2 = new CarDexBackend.Domain.Entities.CompletedTrade(
                Guid.NewGuid(), TradeEnum.FOR_PRICE, buyer.Id, sellerCard.Id, seller.Id, 2000, null);

            _context.CompletedTrades.AddRange(trade1, trade2);
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetTradeHistory(seller.Id, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Trades, t => 
                Assert.True(t.SellerUserId == seller.Id || t.BuyerUserId == seller.Id));
        }

        [Fact]
        public async Task GetTradeHistory_ShouldHandlePagination()
        {
            // Arrange
            var seller = _context.Users.First();
            var buyer = _context.Users.Skip(1).First();
            var sellerCard = _context.Cards.First(c => c.UserId == seller.Id);

            for (int i = 0; i < 5; i++)
            {
                var trade = new CarDexBackend.Domain.Entities.CompletedTrade(
                    Guid.NewGuid(), TradeEnum.FOR_PRICE, seller.Id, sellerCard.Id, buyer.Id, 1000 + i, null);
                _context.CompletedTrades.Add(trade);
            }
            _context.SaveChanges();

            // Act
            var result = await _tradeService.GetTradeHistory(null, limit: 2, offset: 1);

            // Assert
            Assert.Equal(2, result.Limit);
            Assert.Equal(1, result.Offset);
            Assert.True(result.Trades.Count() <= 2);
        }
    }
}
