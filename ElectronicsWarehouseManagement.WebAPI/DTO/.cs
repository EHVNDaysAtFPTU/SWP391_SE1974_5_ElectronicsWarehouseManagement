//using ElectronicsWarehouseManagement.Repositories.Entities;
//using System.Text.Json.Serialization;
//using Microsoft.EntityFrameworkCore;

//namespace ElectronicsWarehouseManagement.WebAPI.DTO
//{
//    public class ItemDTO
//    {
//        [JsonPropertyName("item_id")]
//        public int ItemId { get; set; }

//        [JsonPropertyName("quantity")]
//        public float Quantity { get; set; }

//        [JsonPropertyName("import_date")]
//        public DateOnly ImportDate { get; set; }

//        [JsonPropertyName("transfer_id")]
//        public int? TransferId { get; set; }

//        [JsonPropertyName("inbound_id")]
//        public int? InboundId { get; set; }

//        [JsonPropertyName("outbound_id")]
//        public int? OutboundId { get; set; }

//        [JsonPropertyName("item_def_id")]
//        public int ItemDefId { get; set; }

//        [JsonPropertyName("item_def")]
//        public ItemDefinition? ItemDef { get; set; }

//        [JsonPropertyName("bins")]
//        public List<BinDTO>? Bins { get; set; }


//        public ItemDTO(Item entity)
//        {
//            ItemId = entity.ItemId;
//            Quantity = entity.Quantity;
//            ImportDate = entity.ImportDate;
//            TransferId = entity.TransferId;
//            InboundId = entity.InboundId;
//            OutboundId = entity.OutboundId;
//            ItemDefId = entity.ItemDefId;

//            ItemDef = entity.ItemDef;

//            Bins = entity.Bins?
//                .Select(b => new BinDTO(b))
//                .ToList();
//        }



//        public ItemDTO(int itemId, float quantity, DateOnly importDate,
//                       int? transferId, int? inboundId, int? outboundId,
//                       int itemDefId,
//                       ItemDefinition? itemDef = null,
//                       List<BinDTO>? bins = null)
//        {
//            ItemId = itemId;
//            Quantity = quantity;
//            ImportDate = importDate;
//            TransferId = transferId;
//            InboundId = inboundId;
//            OutboundId = outboundId;
//            ItemDefId = itemDefId;
//            ItemDef = itemDef;
//            Bins = bins;
//        }
//    }
//}