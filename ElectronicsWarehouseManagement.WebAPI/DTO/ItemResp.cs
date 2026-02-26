using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ItemResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("quantity")]
        public float Quantity { get; set; } 

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("transfer_id")]
        //public int? TransferId { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("inbound_id")]
        //public int? InboundId { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("outbound_id")]
        //public int? OutboundId { get; set; }

        [JsonPropertyName("def_id")]
        public int? ItemDefId { get; set; }

        //TODO

        //[JsonPropertyName("transfers")]
        //public List<TransferReqResp> Transfers { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("definition")]
        public ItemDefResp? ItemDef { get; set; }

        [JsonPropertyName("bins")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<BinResp>? Bins { get; set; }

        public ItemResp(Item item, bool fullInfo)
        {
            ID = item.ItemId;
            Quantity = item.Quantity; 
            ItemDefId = item.ItemDefId;
            ItemDef = item.ItemDef is null ? null : new ItemDefResp(item.ItemDef);
            
            if (fullInfo)
            {
                ItemDef = item.ItemDef is null ? null : new ItemDefResp(item.ItemDef);
                Bins = item.Bins.Select(b => new BinResp(b, true)).ToList();
                // TODO
                //Transfers = ...
            }
            
        }
        public ItemResp(ItemResp item) { }
    }
}
