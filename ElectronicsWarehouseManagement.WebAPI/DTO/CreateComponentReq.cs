using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CreateComponentReq : IVerifiableRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("desc")]
        public string Description { get; set; } = "";

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = "";

        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; } = "";

        [JsonPropertyName("manufacturing_date")]
        public DateTime ManufacturingDate { get; set; }

        [JsonPropertyName("datasheet_url")]
        public string DatasheetUrl { get; set; } = "";

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "";

        [JsonPropertyName("unit_price")]
        public float UnitPrice { get; set; }

        [JsonPropertyName("category_ids")]
        public List<int> CategoryIds { get; set; } = [];

        public bool Verify(out string failedReason)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                failedReason = "Name is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Unit))
            {
                failedReason = "Unit is required.";
                return false;
            }
            if (UnitPrice <= 0)
            {
                failedReason = "Unit price must be greater than 0.";
                return false;
            }
            if (CategoryIds.Count == 0)
            {
                failedReason = "At least one category is required.";
                return false;
            }
            failedReason = "";
            return true;
        }
    }
}
