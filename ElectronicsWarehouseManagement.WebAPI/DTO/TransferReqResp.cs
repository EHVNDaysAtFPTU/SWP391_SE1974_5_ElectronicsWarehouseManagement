using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class TransferReqResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

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

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("approver_id")]
        public int? ApproverId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_from_id")]
        public int? WarehouseFromId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_to_id")]
        public int? WarehouseToId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("approver")]
        public User? Approver { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("creator")]
        public User? Creator { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_from")]
        public Warehouse? WarehouseFrom { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_to")]
        public Warehouse? WarehouseTo { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("items")]
        public List<ItemResp>? Items { get; set; }

        public TransferReqResp(TransferReq transferReq, bool fullInfo)
        {
            ID = transferReq.TransferId;
            Description = transferReq.Description;
            Type = (int)transferReq.Type;
            CreationDate = transferReq.CreationDate;
            ExecutionDate = transferReq.ExecutionDate;
            Status = (int)transferReq.Status;
            CreatorId = transferReq.CreatorId;
            ApproverId = transferReq.ApproverId;
            WarehouseFromId = transferReq.WarehouseFromId;
            WarehouseToId = transferReq.WarehouseToId;
            if (fullInfo)
            {
                Approver = transferReq.Approver;
                Creator = transferReq.Creator;
                WarehouseFrom = transferReq.WarehouseFrom;
                WarehouseTo = transferReq.WarehouseTo;
                Items = transferReq.Items.Select(i => new ItemResp(i, true)).ToList();
            }
        }
    }
}
