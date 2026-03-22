using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class BinResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("location_in_warehouse")]
        public string LocationInWarehouse { get; set; }

        [JsonPropertyName("status")]
        public BinStatus Status { get; set; }

        [JsonPropertyName("warehouse_id")]
        public int WarehouseId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse")]
        public WarehouseResp? Warehouse { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("components")]
        public List<ComponentBinResp>? Components { get; set; }

        public BinResp(Bin bin, bool fullInfo) 
        {
            if (bin is null)
                return; // defensive: avoid NullReferenceException if caller passed null

            ID = bin.BinId;
            LocationInWarehouse = bin.LocationInWarehouse;
            Status = bin.Status;
            WarehouseId = bin.WarehouseId;
            if (fullInfo)
            {
                Warehouse = bin.Warehouse is not null ? new WarehouseResp(bin.Warehouse, false) : null;
                Components = bin.ComponentBins?.Where(cb => cb != null).Select(i => new ComponentBinResp(i, false)).ToList();
            }
        }
    }
}
