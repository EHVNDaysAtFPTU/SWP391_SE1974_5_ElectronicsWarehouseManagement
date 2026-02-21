using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class TransferReqDTO
    {
        [JsonPropertyName("transfer_id")]
        public int TransferId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public int TypeInt { get; set; }

        [JsonPropertyName("creation_date")]
        public DateOnly CreationDate { get; set; }

        [JsonPropertyName("execution_date")]
        public DateOnly? ExecutionDate { get; set; }

        [JsonPropertyName("status")]
        public int StatusInt { get; set; }

        [JsonPropertyName("creator_id")]
        public int CreatorId { get; set; }

        [JsonPropertyName("approver_id")]
        public int? ApproverId { get; set; }

        [JsonPropertyName("warehouse_from_id")]
        public int? WarehouseFromId { get; set; }

        [JsonPropertyName("warehouse_to_id")]
        public int? WarehouseToId { get; set; }


        [JsonPropertyName("approver")]
        public User Approver { get; set; }

        [JsonPropertyName("creator")]
        public User Creator { get; set; }

        [JsonPropertyName("item_inbounds")]
        public ICollection<Item> ItemInbounds { get; set; }

        [JsonPropertyName("item_outbounds")]
        public ICollection<Item> ItemOutbounds { get; set; }

        [JsonPropertyName("item_transfers")]
        public ICollection<Item> ItemTransfers { get; set; }

        [JsonPropertyName("warehouse_from")]
        public Warehouse WarehouseFrom { get; set; }

        [JsonPropertyName("warehouse_to")]
        public Warehouse WarehouseTo { get; set; }

        public TransferReqDTO(TransferReq entity)
        {
            TransferId = entity.TransferId;
            Description = entity.Description;
            TypeInt = entity.TypeInt;
            CreationDate = entity.CreationDate;
            ExecutionDate = entity.ExecutionDate;
            StatusInt = entity.StatusInt;
            CreatorId = entity.CreatorId;
            ApproverId = entity.ApproverId;
            WarehouseFromId = entity.WarehouseFromId;
            WarehouseToId = entity.WarehouseToId;

            Approver = entity.Approver;
            Creator = entity.Creator;
            ItemInbounds = entity.ItemInbounds;
            ItemOutbounds = entity.ItemOutbounds;
            ItemTransfers = entity.ItemTransfers;
            WarehouseFrom = entity.WarehouseFrom;
            WarehouseTo = entity.WarehouseTo;
        }
    }
}