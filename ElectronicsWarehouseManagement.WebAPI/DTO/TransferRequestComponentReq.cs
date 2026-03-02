using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class TransferRequestComponentReq : IVerifiableRequest
    {
        [JsonPropertyName("quantity")]
        public float Quantity { get; set; }

        [JsonPropertyName("component_id")]
        public int ComponentId { get; set; }

        [JsonPropertyName("unit_price")]
        public double UnitPrice { get; set; }

        public bool Verify(out string failedReason)
        {
            if (Quantity <= 0)
            {
                failedReason = "Quantity must be greater than zero.";
                return false;
            }
            if (ComponentId <= 0)
            {
                failedReason = "Invalid item definition ID.";
                return false;
            }
            if (UnitPrice <= 0)
            {
                failedReason = "Unit price must be greater than zero.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
