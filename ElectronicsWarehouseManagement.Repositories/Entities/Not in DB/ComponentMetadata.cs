using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public class ComponentMetadata
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("desc")]
        public string Description { get; set; } = "";

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = "";

        [JsonPropertyName("manufacter")]
        public string Manufacter { get; set; } = "";

        [JsonPropertyName("manufacting_date")]
        public DateTime ManufactingDate { get; set; }

        [JsonPropertyName("datasheet_url")]
        public string DatasheetUrl { get; set; } = "";
    }
}

