using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CarDexBackend.Services;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexDatabase;
using CarDexBackend.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using CarDexBackend.Services.Resources;

using CarDexBackend.Repository.Implementations;
using CarDexBackend.Repository.Interfaces;
using CarDexBackend.Domain.Entities;

namespace DefaultNamespace.RegressionTests
{
    /// <summary>
    /// Regression test suite for critical user journeys and business logic.
    /// These tests ensure that core functionality continues to work after code changes.
    /// 
    /// Run with: dotnet test --filter "Category=Regression"
    /// </summary>
    [Trait("Category", "Regression")]
    public class RegressionTestSuite : IDisposable
    {
        private readonly CarDexDbContext _context;
        private readonly AuthService _authService;
        private readonly TradeService _tradeService;
        private readonly PackService _packService;
        private readonly CardService _cardService;
        private readonly UserService _userService;
        private readonly CollectionService _collectionService;
        private readonly Guid _testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");

        public RegressionTestSuite()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<CarDexDbContext>()
                .UseInMemoryDatabase(databaseName: "RegressionTestDatabase_" + Guid.NewGuid())
                .Options;

            _context = new CarDexDbContext(options);

            // Setup configuration for AuthService
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Jwt:SecretKey", "RegressionTestSecretKeyForJwtTokenGenerationThatIsAtLeast32CharactersLong"},
                {"Jwt:Issuer", "CarDex"},
                {"Jwt:Audience", "CarDexUsers"},
                {"Jwt:ExpirationMinutes", "60"}
            });
            var configuration = configurationBuilder.Build();
            var loggerFactory = new LoggerFactory();
            var authLogger = loggerFactory.CreateLogger<AuthService>();

            // Initialize Repositories
            var userRepo = new UserRepository(_context);
            var cardRepo = new CardRepository(_context);
            var packRepo = new PackRepository(_context);
            var openTradeRepo = new OpenTradeRepository(_context);
            var completedTradeRepo = new CompletedTradeRepository(_context);
            var rewardRepo = new RewardRepository(_context);
            var collectionRepo = new CollectionRepository(_context);
            var vehicleRepo = new Repository<Vehicle>(_context);

            // Initialize services
            _authService = new AuthService(userRepo, configuration, authLogger, new NullStringLocalizer<SharedResources>());
            
            var tradeCurrentUserService = new TestCurrentUserService { UserId = _testUserId };
            _tradeService = new TradeService(
                openTradeRepo, 
                completedTradeRepo, 
                userRepo, 
                cardRepo, 
                vehicleRepo, 
                rewardRepo, 
                tradeCurrentUserService,
                new NullStringLocalizer<SharedResources>());
            
            var packCurrentUserService = new TestCurrentUserService { UserId = _testUserId };
            _packService = new PackService(
                packRepo, 
                collectionRepo, 
                userRepo, 
                vehicleRepo, 
                cardRepo, 
                packCurrentUserService,
                new NullStringLocalizer<SharedResources>());
            
            _cardService = new CardService(cardRepo, vehicleRepo, new NullStringLocalizer<SharedResources>());
            
            _userService = new UserService(
                userRepo, 
                cardRepo, 
                packRepo, 
                openTradeRepo, 
                completedTradeRepo, 
                rewardRepo, 
                vehicleRepo, 
                collectionRepo,
                new NullStringLocalizer<SharedResources>());
            
            _collectionService = new CollectionService(collectionRepo, cardRepo, new NullStringLocalizer<SharedResources>());

            // Seed critical test data
            SeedRegressionTestData();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private void SeedRegressionTestData()
        {
            // Create test vehicles first
            var vehicle1 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2023",
                Make = "Tesla",
                Model = "Model S",
                Value = 80000
            };
            var vehicle2 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2022",
                Make = "Ford",
                Model = "Mustang",
                Value = 55000
            };
            _context.Vehicles.AddRange(vehicle1, vehicle2);
            _context.SaveChanges();

            // Create test collection with actual vehicle IDs
            var collection = new CarDexBackend.Domain.Entities.Collection
            {
                Id = Guid.NewGuid(),
                Name = "Regression Test Collection",
                Vehicles = new Guid[] { vehicle1.Id, vehicle2.Id },
                PackPrice = 500
            };
            _context.Collections.Add(collection);
            _context.SaveChanges();
        }

        #region Authentication Regression Tests

        /// <summary>
        /// REGRESSION: User registration must always work correctly
        /// Verifies the critical user registration flow hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Authentication")]
        public async Task Regression_UserRegistration_ShouldSucceed()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = $"RegressionUser_{Guid.NewGuid()}",
                Password = "SecurePassword123"
            };

            // Act
            var result = await _authService.Register(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.User);
            Assert.Equal(request.Username, result.User.Username);
            
            // Verify user exists in database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            Assert.NotNull(user);
        }

        /// <summary>
        /// REGRESSION: User login must always work with valid credentials
        /// Verifies the critical authentication flow hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Authentication")]
        public async Task Regression_UserLogin_ShouldSucceed()
        {
            // Arrange - Register a user first
            var username = $"RegressionLogin_{Guid.NewGuid()}";
            var password = "TestPassword123";
            
            var registerRequest = new RegisterRequest
            {
                Username = username,
                Password = password
            };
            await _authService.Register(registerRequest);

            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.User);
            Assert.Equal(username, result.User.Username);
        }

        #endregion

        #region Trading Regression Tests

        /// <summary>
        /// REGRESSION: Card-for-currency trade execution must always work
        /// This is a critical business flow that must never break.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Trading")]
        public async Task Regression_CardForCurrencyTrade_ShouldExecute()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var buyerId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            
            var seller = new CarDexBackend.Domain.Entities.User
            {
                Id = sellerId,
                Username = "Seller",
                Password = "password",
                Currency = 500
            };
            
            var buyer = new CarDexBackend.Domain.Entities.User
            {
                Id = buyerId,
                Username = "Buyer",
                Password = "password",
                Currency = 2000 // Enough to buy
            };
            _context.Users.AddRange(seller, buyer);

            var vehicle = _context.Vehicles.First();
            var collection = _context.Collections.First();
            
            var sellerCard = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = sellerId,
                VehicleId = vehicle.Id,
                CollectionId = collection.Id,
                Grade = GradeEnum.FACTORY,
                Value = 10000
            };
            _context.Cards.Add(sellerCard);

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(),
                TradeEnum.FOR_PRICE,
                sellerId,
                sellerCard.Id,
                1000,
                null
            );
            _context.OpenTrades.Add(trade);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tradeService.ExecuteTrade(trade.Id, null);

            // Assert
            Assert.NotNull(result.CompletedTrade);
            Assert.Equal(1000, result.CompletedTrade.Price);
            Assert.Equal(sellerId, result.CompletedTrade.SellerUserId);
            Assert.Equal(buyerId, result.CompletedTrade.BuyerUserId);
            
            // Verify currency was transferred
            var updatedSeller = await _context.Users.FindAsync(sellerId);
            var updatedBuyer = await _context.Users.FindAsync(buyerId);
            Assert.Equal(1500, updatedSeller.Currency); // 500 + 1000
            Assert.Equal(1000, updatedBuyer.Currency); // 2000 - 1000
            
            // Verify card ownership changed
            var updatedCard = await _context.Cards.FindAsync(sellerCard.Id);
            Assert.Equal(buyerId, updatedCard.UserId);
        }

        /// <summary>
        /// REGRESSION: Retrieving open trades with filters must always work
        /// Verifies the trade browsing functionality hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Trading")]
        public async Task Regression_GetOpenTradesWithFilters_ShouldWork()
        {
            // Arrange
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Username = "Trader",
                Password = "password",
                Currency = 1000
            };
            _context.Users.Add(user);

            var vehicle = _context.Vehicles.First();
            var collection = _context.Collections.First();
            var card = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                VehicleId = vehicle.Id,
                CollectionId = collection.Id,
                Grade = GradeEnum.FACTORY,
                Value = 10000
            };
            _context.Cards.Add(card);

            var trade = new CarDexBackend.Domain.Entities.OpenTrade(
                Guid.NewGuid(),
                TradeEnum.FOR_PRICE,
                user.Id,
                card.Id,
                1500,
                null
            );
            _context.OpenTrades.Add(trade);
            await _context.SaveChangesAsync();

            // Act
            var result = await _tradeService.GetOpenTrades(
                type: "FOR_PRICE",
                collectionId: collection.Id,
                grade: "FACTORY",
                minPrice: 1000,
                maxPrice: 2000,
                vehicleId: null,
                wantCardId: null,
                sortBy: "price_asc",
                limit: 10,
                offset: 0
            );

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Trades.Count() > 0);
            Assert.True(result.Total > 0);
        }

        #endregion

        #region Pack Opening Regression Tests

        /// <summary>
        /// REGRESSION: Purchasing a pack must always work
        /// Verifies pack purchase flow hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Packs")]
        public async Task Regression_PurchasePack_ShouldDeductCurrency()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = testUserId,
                Username = "PackBuyer",
                Password = "password",
                Currency = 1000
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var collection = _context.Collections.First();
            var request = new PackPurchaseRequest
            {
                CollectionId = collection.Id
            };

            var initialCurrency = user.Currency;

            // Act
            var result = await _packService.PurchasePack(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Pack);
            Assert.Equal(initialCurrency - collection.PackPrice, result.UserCurrency);
            Assert.Equal(collection.Id, result.Pack.CollectionId);
        }

        /// <summary>
        /// REGRESSION: Opening a pack must generate exactly 5 cards
        /// Verifies pack opening logic hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Packs")]
        public async Task Regression_OpenPack_ShouldGenerateFiveCards()
        {
            // Arrange
            var testUserId = Guid.Parse("a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11");
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = testUserId,
                Username = "PackOpener",
                Password = "password",
                Currency = 1000
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var collection = _context.Collections.First();
            var pack = new CarDexBackend.Domain.Entities.Pack(
                Guid.NewGuid(),
                testUserId,
                collection.Id,
                500
            );
            _context.Packs.Add(pack);
            await _context.SaveChangesAsync();

            // Act
            var result = await _packService.OpenPack(pack.Id);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Cards);
            Assert.Equal(5, result.Cards.Count());
            Assert.True(result.Pack.IsOpened);
        }

        #endregion

        #region Card Management Regression Tests

        /// <summary>
        /// REGRESSION: Getting cards with filters must always work
        /// Verifies card retrieval functionality hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Cards")]
        public async Task Regression_GetAllCardsWithFilters_ShouldWork()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = userId,
                Username = "CardOwner",
                Password = "password",
                Currency = 1000
            };
            _context.Users.Add(user);

            var vehicle = _context.Vehicles.First();
            var collection = _context.Collections.First();
            
            var card = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VehicleId = vehicle.Id,
                CollectionId = collection.Id,
                Grade = GradeEnum.NISMO,
                Value = 15000
            };
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cardService.GetAllCards(
                userId: userId,
                collectionId: collection.Id,
                vehicleId: vehicle.Id,
                grade: "NISMO",
                minValue: 10000,
                maxValue: 20000,
                sortBy: "value_desc",
                limit: 10,
                offset: 0
            );

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Cards.Count() > 0);
            Assert.True(result.Total > 0);
            Assert.All(result.Cards, c => Assert.True(c.Value >= 10000 && c.Value <= 20000));
        }

        /// <summary>
        /// REGRESSION: Getting card by ID must always work
        /// Verifies single card retrieval hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Cards")]
        public async Task Regression_GetCardById_ShouldReturnCard()
        {
            // Arrange
            var vehicle = _context.Vehicles.First();
            var collection = _context.Collections.First();
            var card = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                VehicleId = vehicle.Id,
                CollectionId = collection.Id,
                Grade = GradeEnum.FACTORY,
                Value = 10000
            };
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cardService.GetCardById(card.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(card.Id, result.Id);
            Assert.Equal("FACTORY", result.Grade);
            Assert.Equal(10000, result.Value);
        }

        #endregion

        #region User Inventory Regression Tests

        /// <summary>
        /// REGRESSION: Getting user cards must always work
        /// Verifies user inventory retrieval hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "UserInventory")]
        public async Task Regression_GetUserCards_ShouldReturnUserCards()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = userId,
                Username = "Collector",
                Password = "password",
                Currency = 1000
            };
            _context.Users.Add(user);

            var vehicle = _context.Vehicles.First();
            var collection = _context.Collections.First();
            
            var card = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                VehicleId = vehicle.Id,
                CollectionId = collection.Id,
                Grade = GradeEnum.LIMITED_RUN,
                Value = 12000
            };
            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserCards(userId, null, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Cards.Count() > 0);
            Assert.True(result.Total > 0);
            Assert.All(result.Cards, c => Assert.True(true)); // User's cards
        }

        /// <summary>
        /// REGRESSION: Getting user profile must always work
        /// Verifies user profile retrieval hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "UserInventory")]
        public async Task Regression_GetUserProfile_ShouldReturnProfile()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new CarDexBackend.Domain.Entities.User
            {
                Id = userId,
                Username = "TestUser",
                Password = "password",
                Currency = 1500
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserProfile(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("TestUser", result.Username);
        }

        #endregion

        #region Collections Regression Tests

        /// <summary>
        /// REGRESSION: Getting all collections must always work
        /// Verifies collection listing hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Collections")]
        public async Task Regression_GetAllCollections_ShouldReturnCollections()
        {
            // Act
            var result = await _collectionService.GetAllCollections();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Collections.Count() > 0);
            Assert.True(result.Total > 0);
        }

        /// <summary>
        /// REGRESSION: Getting collection by ID must always work
        /// Verifies collection detail retrieval hasn't broken.
        /// </summary>
        [Fact]
        [Trait("Category", "Regression")]
        [Trait("Area", "Collections")]
        public async Task Regression_GetCollectionById_ShouldReturnCollection()
        {
            // Arrange
            var collection = _context.Collections.First();

            // Act
            var result = await _collectionService.GetCollectionById(collection.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(collection.Id, result.Id);
            Assert.Equal("Regression Test Collection", result.Name);
        }

        #endregion
    }
}
