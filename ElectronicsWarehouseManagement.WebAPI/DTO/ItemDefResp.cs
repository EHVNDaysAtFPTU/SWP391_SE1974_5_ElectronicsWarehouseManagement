using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ItemDefResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("metadata")]
        public ComponentMetadata? Metadata { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; } = "";

        [JsonPropertyName("unit_price")]
        public float UnitPrice { get; set; }

        public ItemDefResp(ItemDefinition itemDef)
        {
            ID = itemDef.ItemDefId;
            Metadata = itemDef.Metadata;
            Unit = itemDef.Unit;
            UnitPrice = itemDef.UnitPrice;
        }

        public ItemDefResp(int iD, ComponentMetadata? metadata, string unit, float unitPrice)
        {
            ID = iD;
            Metadata = metadata;
            Unit = unit;
            UnitPrice = unitPrice;
        }
        public ItemDefResp() { }

    }
}