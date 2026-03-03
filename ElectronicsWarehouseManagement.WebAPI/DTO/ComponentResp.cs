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

        [JsonPropertyName("stock_quantity")]
        public double StockQuantity { get; set; }

        [JsonPropertyName("stock")]
        public double Stock { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("categories")]
        public List<ComponentCategoryResp>? Categories { get; set; }

        public ComponentResp(Component component, bool fullInfo)
        {
            ID = component.ComponentId;
            Metadata = component.Metadata;
            Unit = component.Unit;
            UnitPrice = component.UnitPrice;
            var total = component.ComponentBins?.Sum(cb => cb.Quantity) ?? 0;
            StockQuantity = total;
            Stock = total;
            if (fullInfo)
            {
                Categories = component.Categories.Select(cc => new ComponentCategoryResp(cc)).ToList();
            }
        }
    }
}
