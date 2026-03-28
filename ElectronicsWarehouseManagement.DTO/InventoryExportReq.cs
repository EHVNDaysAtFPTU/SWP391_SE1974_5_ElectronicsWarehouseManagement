using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class InventoryExportReq
    {
        [JsonPropertyName("component_ids")]
        public List<int> ComponentIds { get; set; } = new List<int>();

        [JsonPropertyName("export_title")]
        public string ExportTitle { get; set; } = "Inventory Report";

        [JsonPropertyName("include_quantity")]
        public bool IncludeQuantity { get; set; } = true;

        [JsonPropertyName("include_price")]
        public bool IncludePrice { get; set; } = true;
    }
}
