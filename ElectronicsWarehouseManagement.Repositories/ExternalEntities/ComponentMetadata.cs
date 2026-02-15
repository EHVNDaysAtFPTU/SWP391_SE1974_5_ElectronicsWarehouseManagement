using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.Repositories.ExternalEntities
{
    public class ComponentMetadata
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

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
