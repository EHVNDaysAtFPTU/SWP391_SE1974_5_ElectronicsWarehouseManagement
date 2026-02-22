using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CreateBinReq : IVerifiableRequest
    {
        [JsonPropertyName("warehouse_id")]
        public int WarehouseID { get; set; }

        [JsonPropertyName("location_in_warehouse")]
        public string LocationInWarehouse { get; set; } = "";

        public bool Verify(out string failedReason)
        {
            if (WarehouseID < 1)
            {
                failedReason = "WarehouseID is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(LocationInWarehouse))
            {
                failedReason = "Location in warehouse is required.";
                return false;
            }
            failedReason = "";
            return true;
        }
    }
}
