using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using CarDexBackend.Shared.Validator;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace CarDexBackend.UnitTests.Api
{
    public class ValidatorExtensionsTests
    {
        [Fact]
        public void UseTokenValidator_ShouldReturnApplicationBuilder()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var applicationBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

            // Act
            var result = applicationBuilder.UseTokenValidator();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applicationBuilder, result);
        }

        [Fact]
        public void UseUserRateLimiter_ShouldReturnApplicationBuilder()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var applicationBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

            // Act
            var result = applicationBuilder.UseUserRateLimiter();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applicationBuilder, result);
        }

        [Fact]
        public void UseTokenValidator_ShouldChainCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var applicationBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

            // Act
            var result = applicationBuilder
                .UseTokenValidator()
                .UseTokenValidator(); // Should chain

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applicationBuilder, result);
        }

        [Fact]
        public void UseUserRateLimiter_ShouldChainCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var applicationBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

            // Act
            var result = applicationBuilder
                .UseUserRateLimiter()
                .UseUserRateLimiter(); // Should chain

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applicationBuilder, result);
        }

        [Fact]
        public void UseTokenValidator_ShouldAllowChainingWithOtherMiddleware()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var applicationBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

            // Act
            var result = applicationBuilder
                .UseTokenValidator()
                .Use(next => async context => await next(context));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(applicationBuilder, result);
        }
    }
}
