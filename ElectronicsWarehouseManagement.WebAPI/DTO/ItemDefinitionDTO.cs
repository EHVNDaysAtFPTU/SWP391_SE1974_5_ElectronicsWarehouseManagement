using ElectronicsWarehouseManagement.Repositories.Entities;

using System.Text.Json.Serialization;

public class ItemDefinitionDTO
{

    [Obsolete("Use Metadata instead.")]
    [JsonPropertyName("metadata_json")]
    public string MetadataJson { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; }

    [JsonPropertyName("unit_price")]
    public float UnitPrice { get; set; }

    [JsonPropertyName("items")]
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();

    [JsonPropertyName("categories")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public ItemDefinitionDTO(ItemDefinitionDTO itemDef)
    {
        Unit = itemDef.Unit;
        UnitPrice = itemDef.UnitPrice;
        Items = itemDef.Items;
        Categories = itemDef.Categories;
    }
}
