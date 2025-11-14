namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents a single card owned by a user with full vehicle details embedded.
    /// </summary>
    /// <remarks>
    /// This DTO extends <see cref="UserCardResponse"/> by including the complete
    /// vehicle information (make, model, year, stats, image) needed to display
    /// the card in the UI without requiring additional API calls.
    /// Returned by <c>GET /users/{userId}/cards/with-vehicles</c>.
    /// </remarks>
    public class UserCardWithVehicleResponse : UserCardResponse
    {
        /// <summary>
        /// Year of the vehicle (e.g., "1999", "2023").
        /// </summary>
        public string Year { get; set; } = string.Empty;

        /// <summary>
        /// Manufacturer or brand of the vehicle (e.g., "Nissan", "Toyota").
        /// </summary>
        public string Make { get; set; } = string.Empty;

        /// <summary>
        /// Model name of the vehicle (e.g., "Skyline GT-R", "Supra").
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// First performance statistic (e.g., horsepower).
        /// </summary>
        public int Stat1 { get; set; }

        /// <summary>
        /// Second performance statistic (e.g., top speed).
        /// </summary>
        public int Stat2 { get; set; }

        /// <summary>
        /// Third performance statistic (e.g., weight, torque).
        /// </summary>
        public int Stat3 { get; set; }

        /// <summary>
        /// Image URL or base64-encoded image of the vehicle.
        /// </summary>
        public string VehicleImage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a paginated list of cards with vehicle details owned by a user.
    /// </summary>
    /// <remarks>
    /// This DTO includes pagination metadata and is used in  
    /// responses from <c>GET /users/{userId}/cards/with-vehicles</c>.
    /// Each card includes complete vehicle information for immediate display.
    /// </remarks>
    public class UserCardWithVehicleListResponse
    {
        /// <summary>
        /// The list of cards with embedded vehicle details owned by the user.
        /// </summary>
        public IEnumerable<UserCardWithVehicleResponse> Cards { get; set; } = new List<UserCardWithVehicleResponse>();

        /// <summary>
        /// The total number of cards that match the query.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// The maximum number of cards returned in this page.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// The number of cards skipped before the current page.
        /// </summary>
        public int Offset { get; set; }
    }
}
