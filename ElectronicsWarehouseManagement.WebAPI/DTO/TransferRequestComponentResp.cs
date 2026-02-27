using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class TransferRequestComponentResp
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("request_id")]
        public int RequestId { get; set; }

        [JsonPropertyName("component_id")]
        public int ComponentId { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public double UnitPrice { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("component")]
        public Component? Component { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("request")]
        public TransferRequest? Request { get; set; }

        public TransferRequestComponentResp(TransferRequestComponent trc, bool fullInfo)
        {
            ID = trc.TransferRequestComponentId;
            RequestId = trc.RequestId;
            ComponentId = trc.ComponentId;
            Quantity = trc.Quantity;
            UnitPrice = trc.UnitPrice;

            // 🔥 thêm dòng này
            Name = trc.Component?.Metadata?.Name;

            if (fullInfo)
            {
                Component = null;   // 🚫 không trả nguyên entity nữa
                Request = null;
            }
        }
    }
}
