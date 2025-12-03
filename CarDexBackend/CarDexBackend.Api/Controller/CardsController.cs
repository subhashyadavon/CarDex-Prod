using Microsoft.AspNetCore.Mvc;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Services;

namespace CarDexBackend.Controllers
{
    /// <summary>
    /// Handles all card-related operations for the game.
    /// </summary>
    /// <remarks>
    /// Provides endpoints for browsing cards, filtering by attributes, and retrieving detailed card information.
    /// </remarks>
    [ApiController]
    [Route("cards")]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CardsController"/> class.
        /// </summary>
        /// <param name="cardService">Service for accessing and managing card data.</param>
        public CardsController(ICardService cardService)
        {
            _cardService = cardService;
        }

        /// <summary>
        /// Retrieves all available cards, with optional filters and pagination.
        /// </summary>
        /// <param name="userId">Optional user ID to filter by owned cards.</param>
        /// <param name="collectionId">Optional collection ID to filter cards by collection.</param>
        /// <param name="vehicleId">Optional vehicle ID to find specific cards of a vehicle.</param>
        /// <param name="grade">Optional grade filter (FACTORY, LIMITED_RUN, NISMO).</param>
        /// <param name="minValue">Optional minimum card value filter.</param>
        /// <param name="maxValue">Optional maximum card value filter.</param>
        /// <param name="sortBy">Sorting order (value_asc, grade_desc, etc.).</param>
        /// <param name="limit">Maximum number of results per page (1–100).</param>
        /// <param name="offset">Number of results to skip.</param>
        /// <returns>
        /// A paginated list of cards matching the query.
        /// Returns 400 Bad Request if pagination parameters are invalid.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(CardListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCards(
            [FromQuery] Guid? userId,
            [FromQuery] Guid? collectionId,
            [FromQuery] Guid? vehicleId,
            [FromQuery] string? grade,
            [FromQuery] int? minValue,
            [FromQuery] int? maxValue,
            [FromQuery] string? sortBy = "date_desc",
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0)
        {
            var cards = await _cardService.GetAllCards(userId, collectionId, vehicleId, grade, minValue, maxValue, sortBy, limit, offset);
            return Ok(cards);
        }

        /// <summary>
        /// Retrieves detailed information about a specific card.
        /// </summary>
        /// <param name="cardId">Unique identifier of the card to retrieve.</param>
        /// <returns>
        /// Detailed card information if found.
        /// Returns 404 Not Found if the card does not exist.
        /// </returns>
        [HttpGet("{cardId:guid}")]
        [ProducesResponseType(typeof(CardDetailedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCardById(Guid cardId)
        {
            var card = await _cardService.GetCardById(cardId);
            return Ok(card);
        }


        /// <summary>
        /// Retrieves detailed information about a specific vehicle.
        /// </summary>
        [HttpGet("vehicles/{vehicleId:guid}")]
        [ProducesResponseType(typeof(VehicleDetailedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVehicleById(Guid vehicleId)
        {
            var vehicle = await _cardService.GetVehicleById(vehicleId);
            return Ok(vehicle);
        }

        /// <summary>
        /// Retrieves detailed information about all vehicles.
        /// </summary>
        [HttpGet("vehicles")]
        [ProducesResponseType(typeof(VehicleListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllVehicles()
        {
            var vehicles = await _cardService.GetAllVehicles();
            return Ok(vehicles);
        }
    }
}
