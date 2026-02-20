using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CreateTransferReq
    {
        [JsonPropertyName("item_ids")]
        public int[] ItemIds { get; set; } = [];

        [JsonPropertyName("desc")]
        public string Description { get; set; } = "";

        [JsonPropertyName("warehouse_from_id")]
        public int WarehouseFromId { get; set; }

        [JsonPropertyName("warehouse_to_id")]
        public int WarehouseToId { get; set; }
    }
}
