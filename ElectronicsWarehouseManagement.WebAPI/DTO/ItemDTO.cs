using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ItemDTO
    {
        [JsonPropertyName("item_id")]
        public int ItemId { get; set; }

        [JsonPropertyName("metadata")]
        public string Metadata { get; set; }

        [JsonPropertyName("item_quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("item_unit")]
        public string Unit { get; set; }

        [JsonPropertyName("import_date")]
        public DateOnly ImportDate { get; set; }

        [JsonPropertyName("unit_price")]
        public float UnitPrice { get; set; }

        [JsonPropertyName("transfer_id")]
        public int? TransferId { get; set; }

        [JsonPropertyName("iob_id")]
        public int? IobId { get; set; }

        [JsonPropertyName("iob")]
        public virtual InOutBoundReq Iob { get; set; }

        [JsonPropertyName("transfer")]
        public virtual TransferReq Transfer { get; set; }

        [JsonPropertyName("bins")]
        public virtual ICollection<Bin> Bins { get; set; } = new List<Bin>();

        [JsonPropertyName("categories")]
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
