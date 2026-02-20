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
        public int Type { get; set; }

        [JsonPropertyName("creation_date")]
        public DateOnly CreationDate { get; set; }

        [JsonPropertyName("execution_date")]
        public DateOnly? ExecutionDate { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("creator_id")]
        public int CreatorId { get; set; }

        [JsonPropertyName("approver_id")]
        public int ApproverId { get; set; }

        [JsonPropertyName("warehouse_from_id")]
        public int WarehouseFromId { get; set; }

        [JsonPropertyName("warehouse_to_id")]
        public int WarehouseToId { get; set; }

        [JsonPropertyName("approver")]
        public virtual User Approver { get; set; }

        [JsonPropertyName("creator")]
        public virtual User Creator { get; set; }

        [JsonPropertyName("items")]
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();

        [JsonPropertyName("warehouse_from")]
        public virtual Warehouse WarehouseFrom { get; set; }

        [JsonPropertyName("warehouse_to")]
        public virtual Warehouse WarehouseTo { get; set; }

        public TransferReqDTO(int transferId, string description, int type, DateOnly creationDate,
                              DateOnly? executionDate, int status, int creatorId, int approverId,
                              int warehouseFromId, int warehouseToId,
                              User approver = null, User creator = null,
                              ICollection<Item> items = null,
                              Warehouse warehouseFrom = null, Warehouse warehouseTo = null)
        {
            TransferId = transferId;
            Description = description;
            Type = type;
            CreationDate = creationDate;
            ExecutionDate = executionDate;
            Status = status;
            CreatorId = creatorId;
            ApproverId = approverId;
            WarehouseFromId = warehouseFromId;
            WarehouseToId = warehouseToId;
            Approver = approver;
            Creator = creator;
            Items = items ?? new List<Item>();
            WarehouseFrom = warehouseFrom;
            WarehouseTo = warehouseTo;
        }
        public TransferReqDTO(TransferReq entity)
        {
            TransferId = entity.TransferId;
            Description = entity.Description;
            Type = entity.Type;
            CreationDate = entity.CreationDate;
            ExecutionDate = entity.ExecutionDate;
            Status = entity.Status;
            CreatorId = entity.CreatorId;
            ApproverId = entity.ApproverId;
            WarehouseFromId = entity.WarehouseFromId;
            WarehouseToId = entity.WarehouseToId;

            Approver = entity.Approver;
            Creator = entity.Creator;
            Items = entity.Items;
            WarehouseFrom = entity.WarehouseFrom;
            WarehouseTo = entity.WarehouseTo;
        }
    }
}
