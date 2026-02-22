using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class WarehouseResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("desc")]
        public string Description { get; set; }

        [JsonPropertyName("physical_location")]
        public string PhysicalLocation { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bins")]
        public List<BinResp>? Bins { get; set; }

        public WarehouseResp(Warehouse warehouse, bool fullInfo)
        {
            ID = warehouse.WarehouseId;
            Name = warehouse.WarehouseName;
            Description = warehouse.Description;
            PhysicalLocation = warehouse.PhysicalLocation;
            ImageUrl = warehouse.ImageUrl;
            if (fullInfo)
                Bins = warehouse.Bins.Select(b => new BinResp(b, true)).ToList();
        }
    }
}
