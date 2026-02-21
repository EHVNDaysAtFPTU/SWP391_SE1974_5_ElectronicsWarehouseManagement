using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ItemDTO
    {
        [JsonPropertyName("id")]
        public int ItemId { get; set; }

        [JsonPropertyName("metadata")]
        public ComponentMetadata? Metadata { get; set; }

        [JsonPropertyName("quantity")]
        public float Quantity { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        [JsonPropertyName("import_date")]
        public DateOnly ImportDate { get; set; }

        [JsonPropertyName("unit_price")]
        public float UnitPrice { get; set; }

        [JsonPropertyName("transfer_id")]
        public int? TransferId { get; set; }

        [JsonPropertyName("iob_id")]
        public int? IobId { get; set; }

        public ItemDTO(Item item)
        {
            ItemId = item.ItemId;
            Quantity = item.Quantity;
            //Unit = item.Unit;
            ImportDate = item.ImportDate;
            //UnitPrice = item.UnitPrice;
            TransferId = item.TransferId;
            //IobId = item.IobId;
        }

        public ItemDTO(int itemId, ComponentMetadata? metadata, int quantity, string unit, DateOnly importDate, float unitPrice, int? transferId, int? iobId)
        {
            ItemId = itemId;
            Metadata = metadata;
            Quantity = quantity;
            Unit = unit;
            ImportDate = importDate;
            UnitPrice = unitPrice;
            TransferId = transferId;
            IobId = iobId;
        }
    }
}
