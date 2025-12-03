namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents a paginated list of vehicle summaries returned by the API.
    /// </summary>
    public class VehicleListResponse
    {
        /// <summary>
        /// A list of all vehicles
        /// </summary>
        public IEnumerable<VehicleDetailedResponse> Vehicles { get; set; } = new List<VehicleDetailedResponse>();
    }
}
