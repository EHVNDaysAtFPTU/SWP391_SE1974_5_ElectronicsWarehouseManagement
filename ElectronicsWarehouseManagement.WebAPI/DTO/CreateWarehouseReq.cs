using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CreateWarehouseReq : IVerifiableRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("desc")]
        public string Description { get; set; } = "";

        [JsonPropertyName("physical_location")]
        public string PhysicalLocation { get; set; } = "";

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = "";

        public bool Verify(out string failedReason)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                failedReason = "Warehouse name cannot be empty.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(PhysicalLocation))
            {
                failedReason = "Physical location cannot be empty.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
