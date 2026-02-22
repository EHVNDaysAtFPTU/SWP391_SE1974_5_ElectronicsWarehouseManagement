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
        [JsonPropertyName("items")]
        public List<ItemResp>? Items { get; set; }

        public BinResp(Bin bin, bool fullInfo) 
        {
            ID = bin.BinId;
            LocationInWarehouse = bin.LocationInWarehouse;
            Status = bin.Status;
            WarehouseId = bin.WarehouseId;
            if (fullInfo)
            {
                Warehouse = new WarehouseResp(bin.Warehouse, true);
                Items = bin.Items.Select(i => new ItemResp(i, true)).ToList();
            }
        }
    }
}
