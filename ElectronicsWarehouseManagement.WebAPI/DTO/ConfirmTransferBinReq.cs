using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ConfirmTransferBinReq : IVerifiableRequest
    {
        [JsonPropertyName("bin_id")]
        public int BinId { get; set; }

        [JsonPropertyName("quantity")]
        public float Quantity { get; set; }

        public bool Verify(out string failedReason)
        {
            if (BinId <= 0)
            {
                failedReason = "Invalid bin ID.";
                return false;
            }
            if (Quantity <= 0)
            {
                failedReason = "Quantity must be greater than zero.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
