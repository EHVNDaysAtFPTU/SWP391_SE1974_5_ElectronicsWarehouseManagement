using System.Text.Json.Serialization;
using ElectronicsWarehouseManagement.Repositories.Entities;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class BinDTO
    {
        [JsonPropertyName("bin_id")]
        public int BinId { get; set; }

        [JsonPropertyName("location_in_warehouse")]
        public string LocationInWarehouse { get; set; } = null!;

        [JsonPropertyName("status")]
        public int StatusInt { get; set; }

        [JsonPropertyName("warehouse_id")]
        public int WarehouseId { get; set; }

        [JsonPropertyName("warehouse")]
        public WarehouseDTO? Warehouse { get; set; }

      
        public BinDTO(Bin entity)
        {
            BinId = entity.BinId;
            LocationInWarehouse = entity.LocationInWarehouse;
            StatusInt = entity.StatusInt;
            WarehouseId = entity.WarehouseId;

            if (entity.Warehouse != null)
                Warehouse = new WarehouseDTO(entity.Warehouse);
        }

        public BinDTO() { }
    }
}