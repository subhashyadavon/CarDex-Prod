using Microsoft.AspNetCore.Mvc;
using CarDexBackend.Shared.Dtos.Responses;
using CarDexBackend.Services;

namespace CarDexBackend.Controllers
{
    /// <summary>
    /// Handles requests related to card collections in the game.
    /// </summary>
    /// <remarks>
    /// Provides endpoints to browse all collections and retrieve detailed information
    /// about a specific collection, including its cards and theme.
    /// </remarks>
    [ApiController]
    [Route("collections")]
    public class CollectionsController : ControllerBase
    {
        private readonly ICollectionService _collectionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionsController"/> class.
        /// </summary>
        /// <param name="collectionService">Service responsible for retrieving collection data.</param>
        public CollectionsController(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }
        
        /// <summary>
        /// Retrieves all available collections in the game.
        /// </summary>
        /// <remarks>
        /// Each collection contains themed sets of cards that can be browsed or purchased.
        /// </remarks>
        /// <returns>A list of collections with their basic information.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(CollectionListResponse), 200)]
        public async Task<IActionResult> GetAllCollections()
        {
            var result = await _collectionService.GetAllCollections();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves detailed information about a specific collection.
        /// </summary>
        /// <param name="collectionId">Unique identifier of the collection to retrieve.</param>
        /// <returns>
        /// A detailed view of the collection, including its theme, description, and cards.
        /// Returns 404 Not Found if the collection does not exist.
        /// </returns>
        [HttpGet("{collectionId:guid}")]
        [ProducesResponseType(typeof(CollectionDetailedResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        public async Task<IActionResult> GetCollectionById(Guid collectionId)
        {
            try
            {
                var collection = await _collectionService.GetCollectionById(collectionId);
                return Ok(collection);
            }
            catch (KeyNotFoundException ex)
            {
                // Collection ID not found in database
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
        }
    }
}
