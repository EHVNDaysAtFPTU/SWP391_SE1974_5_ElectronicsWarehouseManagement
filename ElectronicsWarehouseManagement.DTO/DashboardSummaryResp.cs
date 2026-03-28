using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class DashboardSummaryResp
    {
        [JsonPropertyName("total_components")]
        public int TotalComponents { get; set; }

        [JsonPropertyName("total_warehouses")]
        public int TotalWarehouses { get; set; }
        [JsonPropertyName("current_stock")]
        public double CurrentStock { get; set; }
        [JsonPropertyName("low_stock_items")]
        public int LowStockItems { get; set; }
        [JsonPropertyName("out_of_stock_items")]
        public double OutOfStockItems { get; set; }
        [JsonPropertyName("inbound_today")]
        public double InboundToday { get; set; }
        [JsonPropertyName("outbound_today")]
        public double OutboundToday { get; set; }
    }
}
