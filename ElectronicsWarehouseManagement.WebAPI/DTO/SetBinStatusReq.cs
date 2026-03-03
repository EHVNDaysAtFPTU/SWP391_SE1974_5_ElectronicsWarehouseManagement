using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class SetBinStatusReq : IVerifiableRequest
    {
        [JsonPropertyName("bin_id")]
        public int BinId { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        public bool Verify(out string failedReason)
        {
            if (BinId <= 0)
            {
                failedReason = "BinId must be greater than 0.";
                return false;
            }
            if (!Enum.IsDefined(typeof(BinStatus), Status))
            {
                failedReason = "Invalid status value.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
