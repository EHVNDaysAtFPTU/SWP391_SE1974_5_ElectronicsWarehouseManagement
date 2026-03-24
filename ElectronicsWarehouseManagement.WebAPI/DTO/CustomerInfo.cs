using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CustomerInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("contact")]
        public string Contact { get; set; } = "";

        [JsonPropertyName("address")]
        public string? Address { get; set; }
    }
}
