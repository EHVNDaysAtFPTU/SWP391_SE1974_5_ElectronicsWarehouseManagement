using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CreateIobReq
    {
        [JsonPropertyName("item_ids")]
        public int[] ItemIds { get; set; } = [];

        [JsonPropertyName("desc")]
        public string Description { get; set; } = "";

        [JsonPropertyName("warehouse_id")]
        public int WarehouseId { get; set; }
    }
}
