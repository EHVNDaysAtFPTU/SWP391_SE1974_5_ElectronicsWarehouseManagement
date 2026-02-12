using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class LoginResp(string href)
    {
        [JsonPropertyName("href")]
        public string Href { get; set; } = href;
    }
}
