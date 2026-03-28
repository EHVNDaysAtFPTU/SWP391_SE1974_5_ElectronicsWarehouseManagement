using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class FinishedTransferRequestComponentResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("request_id")]
        public int RequestID { get; set; }

        [JsonPropertyName("component_id")]
        public int ComponentID { get; set; }

        [JsonPropertyName("bin_id")]
        public int BinID { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("bin")]
        public BinResp Bin { get; set; }

        public FinishedTransferRequestComponentResp(FinishedTransferRequestComponent tComponent)
        {
            ID = tComponent.FinishedTransferRequestComponentId;
            RequestID = tComponent.RequestId;
            ComponentID = tComponent.ComponentId;
            BinID = tComponent.BinId;
            Quantity = tComponent.Quantity;
            Type = (int)tComponent.Type;
            Bin = new BinResp(tComponent.Bin, false);
        }
    }
}

