using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ConfirmTransferBinReq : IVerifiableRequest
    {
        [JsonPropertyName("bin_id")]
        public int BinId { get; set; }

        [JsonPropertyName("components")]
        public List<ConfirmTransferRequestComponentReq> Components { get; set; } = [];

        public bool Verify(out string failedReason)
        {
            if (BinId <= 0)
            {
                failedReason = "Invalid bin ID.";
                return false;
            }
            if (Components.Count <= 0)
            {
                failedReason = "No components provided in transfer confirmation request.";
                return false;
            }
            foreach (var component in Components) 
            {
                if (!component.Verify(out failedReason))
                    return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
