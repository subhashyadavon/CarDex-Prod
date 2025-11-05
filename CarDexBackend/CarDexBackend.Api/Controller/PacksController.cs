using Microsoft.AspNetCore.Mvc;
using CarDexBackend.Shared.Dtos.Requests;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Services;

namespace CarDexBackend.Controllers
{
    /// <summary>
    /// Handles all endpoints related to card packs.
    /// </summary>
    /// <remarks>
    /// Provides functionality for purchasing, viewing, and opening packs.
    /// In the mock implementation, all data is stored in-memory for parallel development.
    /// </remarks>
    [ApiController]
    [Route("packs")]
    public class PacksController : ControllerBase
    {
        private readonly IPackService _packService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PacksController"/> class.
        /// </summary>
        /// <param name="packService">Service responsible for managing pack operations.</param>
        public PacksController(IPackService packService)
        {
            _packService = packService;
        }

        /// <summary>
        /// Purchases a pack from a specific collection using the user's currency.
        /// </summary>
        /// <param name="request">The purchase request containing the collection ID and user details.</param>
        /// <returns>
        /// 201 Created with pack details on success.
        /// 400 Bad Request for invalid input.
        /// 401 Unauthorized if the user is not authenticated.
        /// 404 Not Found if the collection does not exist.
        /// 409 Conflict if the user has insufficient currency.
        /// </returns>
        [HttpPost("purchase")]
        [ProducesResponseType(typeof(PackPurchaseResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PurchasePack([FromBody] PackPurchaseRequest request)
        {
            var result = await _packService.PurchasePack(request);
            return CreatedAtAction(nameof(GetPackById), new { packId = result.Pack.Id }, result);
        }

        /// <summary>
        /// Retrieves detailed information about a specific owned pack.
        /// </summary>
        /// <param name="packId">Unique identifier of the pack to retrieve.</param>
        /// <returns>
        /// 200 OK with pack details.
        /// 401 Unauthorized if the user is not logged in.
        /// 403 Forbidden if the pack does not belong to the requesting user.
        /// 404 Not Found if the pack does not exist.
        /// </returns>
        [HttpGet("{packId:guid}")]
        [ProducesResponseType(typeof(PackDetailedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPackById(Guid packId)
        {
            var pack = await _packService.GetPackById(packId);
            return Ok(pack);
        }

        /// <summary>
        /// Opens an owned pack, consumes it, and generates cards for the user.
        /// </summary>
        /// <param name="packId">Unique identifier of the pack to open.</param>
        /// <returns>
        /// 200 OK with generated cards.
        /// 401 Unauthorized if the user is not logged in.
        /// 403 Forbidden if the pack does not belong to the requesting user.
        /// 404 Not Found if the pack does not exist.
        /// 409 Conflict if the pack has already been opened.
        /// </returns>
        [HttpPost("{packId:guid}/open")]
        [ProducesResponseType(typeof(PackOpenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> OpenPack(Guid packId)
        {
            var response = await _packService.OpenPack(packId);
            return Ok(response);
        }
    }
}
