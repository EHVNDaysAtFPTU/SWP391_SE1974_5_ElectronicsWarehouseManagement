using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ComponentBinResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; } 

        [JsonPropertyName("component_id")]
        public int ComponentId { get; set; }

        [JsonPropertyName("bin_id")]
        public int BinId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("component")]
        public ComponentResp? Component { get; set; }

        [JsonPropertyName("bin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BinResp? Bin { get; set; }

        public ComponentBinResp(ComponentBin item, bool fullInfo)
        {
            ID = item.ComponentBinId;
            Quantity = item.Quantity; 
            ComponentId = item.ComponentId;
            BinId = item.BinId;
            if (fullInfo)
            {
                Component = new ComponentResp(item.Component, true);
                Bin = new BinResp(item.Bin, true);
            }
        }
    }
}
