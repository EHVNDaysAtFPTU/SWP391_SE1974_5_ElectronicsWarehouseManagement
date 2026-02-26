using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ComponentResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("metadata")]
        public ComponentMetadata? Metadata { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "";

        [JsonPropertyName("unit_price")]
        public double UnitPrice { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("categories")]
        public List<ComponentCategoryResp>? Categories { get; set; }

        public ComponentResp(Component component, bool fullInfo)
        {
            ID = component.ComponentId;
            Metadata = component.Metadata;
            Unit = component.Unit;
            UnitPrice = component.UnitPrice;
            if (fullInfo)
            {
                Categories = component.Categories.Select(cc => new ComponentCategoryResp(cc)).ToList();
            }
        }
    }
}
