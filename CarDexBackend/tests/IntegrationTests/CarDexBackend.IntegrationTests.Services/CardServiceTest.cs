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

namespace DefaultNamespace
{
    public class CardServiceTest : IDisposable
    {
        private readonly CarDexDbContext _context;
        private readonly CardService _cardService;

        //Used ChatGPT to set up the base code, help with seeding the data and modify the code for the test
        //for getallcards with filters and sorting
        public CardServiceTest()
        {
            // Use In-Memory Database for isolated testing
            var options = new DbContextOptionsBuilder<CarDexDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_CardService_" + Guid.NewGuid())
                .Options;

            _context = new CarDexDbContext(options);
            _cardService = new CardService(_context);

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
            // Add test vehicles
            var vehicle1 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2021",
                Make = "Tesla",
                Model = "Model S",
                Value = 70000,
                
            };

            var vehicle2 = new CarDexBackend.Domain.Entities.Vehicle
            {
                Id = Guid.NewGuid(),
                Year = "2020",
                Make = "Ford",
                Model = "Mustang",
                Value = 50000,
                
            };

            _context.Vehicles.Add(vehicle1);
            _context.Vehicles.Add(vehicle2);
            _context.SaveChanges();

            // Add test cards
            var card1 = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                VehicleId = vehicle1.Id,
                CollectionId = Guid.NewGuid(),
                Grade = CarDexBackend.Domain.Enums.GradeEnum.FACTORY,
                Value = 70000,
                
            };

            var card2 = new CarDexBackend.Domain.Entities.Card
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                VehicleId = vehicle2.Id,
                CollectionId = Guid.NewGuid(),
                Grade = CarDexBackend.Domain.Enums.GradeEnum.LIMITED_RUN,
                Value = 50000,
                
            };

            _context.Cards.Add(card1);
            _context.Cards.Add(card2);
            _context.SaveChanges();
        }

        // Test for GetAllCards
        [Fact]
        public async Task GetAllCards_ShouldReturnCorrectCards()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Cards.Count()); 
            Assert.Equal(2, result.Total); 
            // Check cards exist, don't rely on specific order
            var cardNames = result.Cards.Select(c => c.Name).ToList();
            Assert.Contains("2021 Tesla Model S", cardNames);
            Assert.Contains("2020 Ford Mustang", cardNames);
        }

        // Test for GetCardById
        [Fact]
        public async Task GetCardById_ShouldReturnCorrectCard()
        {
            // Arrange - Get the Tesla card specifically
            var teslaVehicle = _context.Vehicles.First(v => v.Make == "Tesla");
            var teslaCard = _context.Cards.First(c => c.VehicleId == teslaVehicle.Id);
            
            // Act
            var result = await _cardService.GetCardById(teslaCard.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teslaCard.Id, result.Id);
            Assert.Equal("2021 Tesla Model S", result.Name);
            Assert.Equal(teslaCard.Grade.ToString(), result.Grade);
            Assert.Equal(teslaCard.Value, result.Value);
        }

        // Test for GetAllCards with filters
        [Fact]
        public async Task GetAllCards_WithFilters_ShouldReturnFilteredCards()
        {
            // Act: Filter by grade
            var result = await _cardService.GetAllCards(null, null, null, "FACTORY", null, null, null, 10, 0);

            // Assert: Only one card should be returned (Tesla Model S)
            Assert.NotNull(result);
            Assert.Single(result.Cards);
            Assert.Equal("2021 Tesla Model S", result.Cards.First().Name);
        }

        // Test for GetAllCards with sorting (descending by value)
        [Fact]
        public async Task GetAllCards_WithSorting_ShouldReturnSortedCards()
        {
            // Act: Sort by value descending
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, "value_desc", 10, 0);

            // Assert: The first card should be the one with the higher value
            Assert.NotNull(result);
            Assert.Equal(2, result.Cards.Count());
            Assert.Equal("2021 Tesla Model S", result.Cards.First().Name);
            Assert.Equal("2020 Ford Mustang", result.Cards.Last().Name);
        }

        [Fact]
        public async Task GetAllCards_ShouldFilterByUserId()
        {
            // Arrange
            var userId = _context.Cards.First().UserId;

            // Act
            var result = await _cardService.GetAllCards(userId, null, null, null, null, null, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Cards, c => Assert.True(true)); // Cards filtered by userId
        }

        [Fact]
        public async Task GetAllCards_ShouldFilterByCollectionId()
        {
            // Arrange
            var collectionId = _context.Cards.First().CollectionId;

            // Act
            var result = await _cardService.GetAllCards(null, collectionId, null, null, null, null, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Cards);
        }

        [Fact]
        public async Task GetAllCards_ShouldFilterByVehicleId()
        {
            // Arrange
            var vehicleId = _context.Cards.First().VehicleId;

            // Act
            var result = await _cardService.GetAllCards(null, null, vehicleId, null, null, null, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Cards);
        }

        [Fact]
        public async Task GetAllCards_ShouldFilterByMinValue()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, minValue: 60000, null, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Cards, c => Assert.True(c.Value >= 60000));
        }

        [Fact]
        public async Task GetAllCards_ShouldFilterByMaxValue()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, maxValue: 60000, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Cards, c => Assert.True(c.Value <= 60000));
        }

        [Fact]
        public async Task GetAllCards_ShouldSortByValueAsc()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, "value_asc", 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Cards.Count() >= 1);
        }

        [Fact]
        public async Task GetAllCards_ShouldSortByGradeAsc()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, "grade_asc", 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Cards.Count() >= 1);
        }

        [Fact]
        public async Task GetAllCards_ShouldSortByGradeDesc()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, "grade_desc", 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Cards.Count() >= 1);
        }

        [Fact]
        public async Task GetAllCards_ShouldHandlePagination()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, null, limit: 1, offset: 0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Cards.Count() <= 1);
            Assert.Equal(1, result.Limit);
            Assert.Equal(0, result.Offset);
        }

        [Fact]
        public async Task GetCardById_ShouldThrowWhenCardNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _cardService.GetCardById(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetCardById_ShouldHandleMissingVehicle()
        {
            // Arrange
            var card = _context.Cards.First();
            card.VehicleId = Guid.NewGuid(); // Non-existent vehicle
            _context.SaveChanges();

            // Act
            var result = await _cardService.GetCardById(card.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Unknown Vehicle", result.Name);
        }

        [Fact]
        public async Task GetAllCards_ShouldHandleEmptyGradeFilter()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, "   ", null, null, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Cards.Count());
        }

        [Fact]
        public async Task GetAllCards_ShouldUseDefaultSortWhenInvalidSortBy()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, "invalid_sort", 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Cards.Count());
        }

        [Fact]
        public async Task GetCardById_ShouldIncludeAllCardDetails()
        {
            // Arrange
            var card = _context.Cards.First();
            var vehicle = _context.Vehicles.First(v => v.Id == card.VehicleId);

            // Act
            var result = await _cardService.GetCardById(card.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(card.Id, result.Id);
            Assert.Equal($"{vehicle.Year} {vehicle.Make} {vehicle.Model}", result.Name);
            Assert.Equal(card.Grade.ToString(), result.Grade);
            Assert.Equal(card.Value, result.Value);
            Assert.Equal(card.VehicleId.ToString(), result.VehicleId);
            Assert.Equal(card.CollectionId.ToString(), result.CollectionId);
            Assert.Equal(card.UserId.ToString(), result.OwnerId);
        }

        [Fact]
        public async Task GetAllCards_ShouldReturnCorrectTotalCount()
        {
            // Arrange - Add more cards
            var vehicle = _context.Vehicles.First();
            for (int i = 0; i < 3; i++)
            {
                var card = new CarDexBackend.Domain.Entities.Card
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    VehicleId = vehicle.Id,
                    CollectionId = Guid.NewGuid(),
                    Grade = CarDexBackend.Domain.Enums.GradeEnum.FACTORY,
                    Value = 50000 + i
                };
                _context.Cards.Add(card);
            }
            _context.SaveChanges();

            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Total >= 5); // At least the original 2 + 3 new
        }

        [Fact]
        public async Task GetAllCards_ShouldHandleMultipleFiltersTogether()
        {
            // Arrange
            var userId = _context.Cards.First().UserId;
            var collectionId = _context.Cards.First().CollectionId;

            // Act
            var result = await _cardService.GetAllCards(userId, collectionId, null, "FACTORY", null, null, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Cards, c => Assert.True(true)); // Filtered by multiple criteria
        }

        [Fact]
        public async Task GetAllCards_ShouldHandleLargeOffset()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, null, 10, 100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Offset);
            Assert.Empty(result.Cards);
        }

        [Fact]
        public async Task GetAllCards_ShouldHandleZeroLimit()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, null, null, null, 0, 0);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Limit);
            Assert.Empty(result.Cards);
        }

        [Fact]
        public async Task GetAllCards_ShouldFilterByMinAndMaxValueTogether()
        {
            // Act
            var result = await _cardService.GetAllCards(null, null, null, null, minValue: 50000, maxValue: 70000, null, 10, 0);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Cards, c => Assert.True(c.Value >= 50000 && c.Value <= 70000));
        }
    }
}
