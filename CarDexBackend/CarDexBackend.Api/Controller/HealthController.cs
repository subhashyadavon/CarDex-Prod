using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarDexBackend.Api.Controllers
{
    /// <summary>
    /// Provides a simple health check for the API.
    /// </summary>
    /// <remarks>
    /// Use this endpoint to verify that the API process is running and reachable.
    /// This endpoint does not require authentication.
    /// </remarks>
    [ApiController]
    [Route("health")]
    [AllowAnonymous] // so it is accessible without JWT
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Basic health check, returns 200 OK if the API is up.
        /// </summary>
        /// <returns>200 OK with a minimal status payload.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(new { status = "Healthy" });
        }
    }
}