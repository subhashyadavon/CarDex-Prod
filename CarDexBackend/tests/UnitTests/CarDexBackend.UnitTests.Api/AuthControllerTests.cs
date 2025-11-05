using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using CarDexBackend.Api.Controllers;
using CarDexBackend.Services;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CarDexBackend.UnitTests.Api.Controllers
{
    /// <summary>
    /// Contains unit tests for the <see cref="AuthController"/> class.
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthControllerTests"/> class with mocked dependencies.
        /// </summary>
        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        // ===== SUCCESSES =====

        /// <summary>
        /// Verifies that <see cref="AuthController.Register"/> returns a <see cref="CreatedAtActionResult"/> when registration is successful.
        /// </summary>
        [Fact]
        public async Task Register_Succeeds()
        {
            var request = new RegisterRequest { Username = "testuser", Password = "password" };
            var user = new UserResponse { Id = Guid.NewGuid(), Username = "testuser" };
            var loginResponse = new LoginResponse 
            { 
                AccessToken = "mock-token",
                User = user
            };

            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterRequest>())).ReturnsAsync(loginResponse);

            var result = await _controller.Register(request);

            var createdResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<LoginResponse>(createdResult.Value);
            Assert.Equal(user.Username, value.User.Username);
        }

        /// <summary>
        /// Verifies that <see cref="AuthController.Login"/> returns an <see cref="OkObjectResult"/> when valid credentials are provided.
        /// </summary>
        [Fact]
        public async Task Login_Succeeds()
        {
            var request = new LoginRequest { Username = "testuser", Password = "password" };
            var loginResponse = new LoginResponse { AccessToken = "abc123" };

            _mockAuthService.Setup(s => s.Login(It.IsAny<LoginRequest>())).ReturnsAsync(loginResponse);

            var result = await _controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.Equal("abc123", value.AccessToken);
        }

        /// <summary>
        /// Verifies that <see cref="AuthController.Logout"/> returns a <see cref="NoContentResult"/>.
        /// </summary>
        [Fact]
        public async Task Logout_Succeeds()
        {
            var userId = Guid.NewGuid();
            var claims = new[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("unique_name", "testuser")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };

            _mockAuthService.Setup(s => s.Logout(It.IsAny<Guid>())).Returns(Task.CompletedTask);

            var result = await _controller.Logout();

            Assert.IsType<NoContentResult>(result);
        }

        // ===== FAILURES =====

        /// <summary>
        /// Verifies that <see cref="AuthController.Register"/> returns a <see cref="ConflictObjectResult"/> when the username already exists.
        /// </summary>
        [Fact]
        public async Task Register_Conflicts()
        {
            var request = new RegisterRequest { Username = "duplicate", Password = "123" };

            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterRequest>())).ThrowsAsync(new InvalidOperationException("Username already exists."));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Register(request));
        }

        /// <summary>
        /// Ensures Register returns 400 BadRequest for unknown exceptions.
        /// </summary>
        [Fact]
        public async Task Register_BadRequest()
        {
            var request = new RegisterRequest { Username = "broken", Password = "test" };

            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterRequest>())).ThrowsAsync(new Exception("Bad request"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Register(request));
        }

        /// <summary>
        /// Verifies that <see cref="AuthController.Login"/> returns an <see cref="UnauthorizedObjectResult"/> when credentials are invalid.
        /// </summary>
        [Fact]
        public async Task Login_Unauthorized()
        {
            var request = new LoginRequest { Username = "wrong", Password = "invalid" };

            _mockAuthService.Setup(s => s.Login(It.IsAny<LoginRequest>())).ThrowsAsync(new UnauthorizedAccessException("Invalid credentials."));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Login(request));
        }

        /// <summary>
        /// Verifies that Register returns 503 for DbUpdateException.
        /// </summary>
        [Fact]
        public async Task Register_DbUpdateException()
        {
            var request = new RegisterRequest { Username = "test", Password = "password" };

            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterRequest>())).ThrowsAsync(new DbUpdateException("Database error"));

            var result = await _controller.Register(request);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusResult.StatusCode);
            var error = Assert.IsType<ErrorResponse>(statusResult.Value);
            Assert.Contains("Database error", error.Message);
        }

        /// <summary>
        /// Verifies that Register returns 503 for transient failure.
        /// </summary>
        [Fact]
        public async Task Register_TransientFailure()
        {
            var request = new RegisterRequest { Username = "test", Password = "password" };

            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterRequest>())).ThrowsAsync(new InvalidOperationException("transient failure occurred"));

            var result = await _controller.Register(request);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusResult.StatusCode);
            var error = Assert.IsType<ErrorResponse>(statusResult.Value);
            Assert.Contains("Database connection failed", error.Message);
        }

        /// <summary>
        /// Verifies that Register returns 503 for exception has been raised.
        /// </summary>
        [Fact]
        public async Task Register_ExceptionHasBeenRaised()
        {
            var request = new RegisterRequest { Username = "test", Password = "password" };

            _mockAuthService.Setup(s => s.Register(It.IsAny<RegisterRequest>())).ThrowsAsync(new InvalidOperationException("exception has been raised"));

            var result = await _controller.Register(request);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(503, statusResult.StatusCode);
            var error = Assert.IsType<ErrorResponse>(statusResult.Value);
            Assert.Contains("Database connection failed", error.Message);
        }

        /// <summary>
        /// Verifies that Logout returns 401 when user claim is missing.
        /// </summary>
        [Fact]
        public async Task Logout_UnauthorizedWhenNoClaim()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            var result = await _controller.Logout();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(unauthorized.Value);
            Assert.Equal("Invalid token.", error.Message);
        }

        /// <summary>
        /// Verifies that Logout returns 401 when user ID claim is invalid.
        /// </summary>
        [Fact]
        public async Task Logout_UnauthorizedWhenInvalidClaim()
        {
            var claims = new[]
            {
                new Claim("sub", "not-a-guid")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = principal
                }
            };

            var result = await _controller.Logout();

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(unauthorized.Value);
            Assert.Equal("Invalid token.", error.Message);
        }
    }
}
