using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class UpdateBinReq
    {
        [JsonPropertyName("bin_id")]
        public int BinId { get; set; }
        [JsonPropertyName("location_in_warehouse")]

        public string LocationInWarehouse { get; set; }

        [JsonPropertyName("status_int")]
        public int StatusInt { get; set; }
    }
}
