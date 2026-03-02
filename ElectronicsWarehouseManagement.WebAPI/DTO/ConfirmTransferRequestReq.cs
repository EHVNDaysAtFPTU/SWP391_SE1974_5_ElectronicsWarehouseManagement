using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ConfirmTransferRequestReq : IVerifiableRequest
    {
        [JsonPropertyName("request_id")]
        public int RequestId { get; set; }

        [JsonPropertyName("bins")]
        public List<ConfirmTransferBinReq> Bins { get; set; } = [];

        public bool Verify(out string failedReason)
        {
            if (RequestId <= 0)
            {
                failedReason = "Invalid transfer ID.";
                return false;
            }
            if (Bins.Count == 0)
            {
                failedReason = "At least one bin must be provided.";
                return false;
            }
            foreach (var bin in Bins)
            {
                if (!bin.Verify(out failedReason))
                    return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
