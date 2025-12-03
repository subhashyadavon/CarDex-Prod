namespace CarDexBackend.Shared.Dtos.Responses
{
    /// <summary>
    /// Represents a detailed view of a vehicle
    /// </summary>
    public class VehicleDetailedResponse
    {
        public string Year { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Stat1 { get; set; }
        public int Stat2 { get; set; }
        public int Stat3 { get; set; }
        public int Value { get; set; }

    }
}
