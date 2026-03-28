using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class ConfirmTransferRequestReq : IVerifiableRequest
    {
        [JsonPropertyName("request_id")]
        public int RequestId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bins_to")]
        public List<ConfirmTransferBinReq>? BinsTo { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bins_from")]
        public List<ConfirmTransferBinReq>? BinsFrom { get; set; }

        public bool Verify(out string failedReason)
        {
            if (RequestId < 0)
            {
                failedReason = "Invalid transfer ID.";
                return false;
            }
            if (BinsFrom is null && BinsTo is null)
            {
                failedReason = "At least one kind of bins must be provided.";
                return false;
            }
            if (BinsFrom is not null && BinsFrom.Count == 0)
            {
                failedReason = "At least one bin must be provided.";
                return false;
            }
            if (BinsTo is not null && BinsTo.Count == 0)
            {
                failedReason = "At least one bin must be provided.";
                return false;
            }
            if (BinsFrom is not null && BinsTo is not null)
            {
                if (BinsFrom.Count != BinsTo.Count)
                {
                    failedReason = "The number of bins from and to must be the same.";
                    return false;
                }
            }
            foreach (var bin in (BinsFrom ?? []).Concat(BinsTo ?? []))
            {
                if (!bin.Verify(out failedReason))
                    return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
