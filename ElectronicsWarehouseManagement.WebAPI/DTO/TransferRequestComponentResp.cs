using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class TransferRequestComponentResp
    {
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
        public ComponentResp? Component { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("request")]
        public TransferRequestResp? Request { get; set; }

        public TransferRequestComponentResp(TransferRequestComponent trc, bool fullInfo)
        {
            ID = trc.TransferRequestComponentId;
            RequestId = trc.RequestId;
            ComponentId = trc.ComponentId;
            Quantity = trc.Quantity;
            UnitPrice = trc.UnitPrice;
            if (fullInfo)
            {
                Component = new ComponentResp(trc.Component, false);
                Request = new TransferRequestResp(trc.Request, false);
            }
        }
    }
}
