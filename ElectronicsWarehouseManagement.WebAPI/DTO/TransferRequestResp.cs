using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class TransferRequestResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("creation_date")]
        public DateTime CreationDate { get; set; }

        [JsonPropertyName("execution_date")]
        public DateTime? ExecutionDate { get; set; }

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
        [JsonPropertyName("customer_id")]
        public int? CustomerId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("approver")]
        public UserResp? Approver { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("creator")]
        public UserResp? Creator { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_from")]
        public WarehouseResp? WarehouseFrom { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_to")]
        public WarehouseResp? WarehouseTo { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("components")]
        public List<TransferRequestComponentResp>? Components { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("customer")]
        public CustomerResp? Customer { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("finished_components")]
        public List<FinishedTransferRequestComponentResp>? FinishedComponents { get; set; }

        public TransferRequestResp(TransferRequest request, bool fullInfo)
        {
            ID = request.RequestId;
            Description = request.Description;
            Type = (int)request.Type;
            CreationDate = request.CreationTime;
            ExecutionDate = request.ExecutionTime;
            Status = (int)request.Status;
            CreatorId = request.CreatorId;
            ApproverId = request.ApproverId;
            WarehouseFromId = request.WarehouseFromId;
            WarehouseToId = request.WarehouseToId;
            CustomerId = request.CustomerId;
            if (fullInfo)
            {
                Creator = new UserResp(request.Creator, false);
                if (request.ApproverId is not null)
                    Approver = new UserResp(request.Approver, false);
                if (request.WarehouseFromId is not null)
                    WarehouseFrom = new WarehouseResp(request.WarehouseFrom, false);
                if (request.WarehouseToId is not null)
                    WarehouseTo = new WarehouseResp(request.WarehouseTo, false);
                if (request.CustomerId is not null)
                    Customer = new CustomerResp(request.Customer, false);
                Components = request.TransferRequestComponents.Select(i => new TransferRequestComponentResp(i, false)).ToList();
                if (request.Status == TransferStatus.Finished || request.Status == TransferStatus.MissingComponents)
                    FinishedComponents = request.FinishedTransferRequestComponents.Select(i => new FinishedTransferRequestComponentResp(i)).ToList();
            }
        }
    }
}
