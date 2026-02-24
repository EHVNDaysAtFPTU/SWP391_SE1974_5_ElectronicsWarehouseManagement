using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ItemReq : IVerifiableRequest
    {
        [JsonPropertyName("quantity")]
        public float Quantity { get; set; }

        [JsonPropertyName("def_id")]
        public int ItemDefId { get; set; }

        public bool Verify(out string failedReason)
        {
            if (Quantity <= 0)
            {
                failedReason = "Quantity must be greater than zero.";
                return false;
            }
            if (ItemDefId <= 0)
            {
                failedReason = "Invalid item definition ID.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
