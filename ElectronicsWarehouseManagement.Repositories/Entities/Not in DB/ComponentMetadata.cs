using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public class ComponentMetadata
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
    }
}
