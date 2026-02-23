using System.Text.Json.Serialization;
using ElectronicsWarehouseManagement.Repositories.Entities;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class WarehouseDTO
    {
        [JsonPropertyName("warehouse_id")]
        public int WarehouseId { get; set; }

        [JsonPropertyName("warehouse_name")]
        public string WarehouseName { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("physical_location")]
        public string PhysicalLocation { get; set; } = null!;

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        public WarehouseDTO() { }

        public WarehouseDTO(Warehouse entity)
        {
            WarehouseId = entity.WarehouseId;
            WarehouseName = entity.WarehouseName;
            Description = entity.Description;
            PhysicalLocation = entity.PhysicalLocation;
            ImageUrl = entity.ImageUrl;
        }
    }
}