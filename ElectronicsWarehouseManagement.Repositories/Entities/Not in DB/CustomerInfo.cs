using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public class CustomerInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("contact")]
        public string Contact { get; set; } = "";

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        //TODO: more fields if needed
    }
}
