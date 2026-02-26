using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ConfirmTransferRequestComponentReq : IVerifiableRequest
    {
        [JsonPropertyName("quantity")]
        public float Quantity { get; set; }

        [JsonPropertyName("component_id")]
        public int ComponentId { get; set; }

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
            failedReason = string.Empty;
            return true;
        }
    }
}
