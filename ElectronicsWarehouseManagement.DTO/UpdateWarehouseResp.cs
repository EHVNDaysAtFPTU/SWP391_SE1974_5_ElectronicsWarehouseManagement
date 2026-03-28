using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class UpdateWarehouseResp
    {

        [JsonPropertyName("warehouse_id")]
        public int WarehouseId { get; set; }
        [JsonPropertyName("warehouse_name")]
        public string WarehouseName { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("physical_location")]
        public string PhysicalLocation { get; set; }
        public UpdateWarehouseResp(Warehouse warehouse)
        {
            WarehouseId = warehouse.WarehouseId;
            WarehouseName = warehouse.WarehouseName;
            Description = warehouse.Description;
            PhysicalLocation = warehouse.PhysicalLocation;
        }
    }
}
