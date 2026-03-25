using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class UpdateBinResp
    {
        [JsonPropertyName("bin_id")]
        public int BinId { get; set; }
        [JsonPropertyName("location_in_warehouse")]

        public string LocationInWarehouse { get; set; }

        [JsonPropertyName("status_int")]
        public int StatusInt { get; set; }
    }
}
