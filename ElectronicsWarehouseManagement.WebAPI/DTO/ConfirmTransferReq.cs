using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ConfirmTransferReq : IVerifiableRequest
    {
        [JsonPropertyName("transfer_id")]
        public int TransferId { get; set; }

        [JsonPropertyName("bins")]
        public List<ConfirmTransferBinReq> TransferBins { get; set; } = [];

        public bool Verify(out string failedReason)
        {
            if (TransferId <= 0)
            {
                failedReason = "Invalid transfer ID.";
                return false;
            }
            if (TransferBins.Count == 0)
            {
                failedReason = "At least one bin must be provided.";
                return false;
            }
            foreach (var bin in TransferBins)
            {
                if (!bin.Verify(out failedReason))
                    return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
