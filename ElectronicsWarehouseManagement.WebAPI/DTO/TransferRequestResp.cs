using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;
using System.Linq;

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
        [JsonPropertyName("bin_from_id")]
        public int? BinFromId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bin_to_id")]
        public int? BinToId { get; set; }

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
        [JsonPropertyName("bin_from")]
        public BinResp? BinFrom { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bin_to")]
        public BinResp? BinTo { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("customer")]
        public CustomerResp? Customer { get; set; }

        [JsonPropertyName("supplier_customer_name")]
        public string? SupplierCustomerName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("components")]
        public List<TransferRequestComponentResp>? Components { get; set; }

        public TransferRequestResp(TransferRequest request, bool fullInfo)
        {
            ID = request.RequestId;
            Description = request.Description;
            // Use underlying int fields from the entity model
            Type = request.TypeInt;
            CreationDate = request.CreationTime;
            ExecutionDate = request.ExecutionTime;
            Status = request.StatusInt;
            CreatorId = request.CreatorId;
            ApproverId = request.ApproverId;
            BinFromId = request.BinFromId;
            BinToId = request.BinToId;
            CustomerId = request.CustomerId;

            if (fullInfo)
            {
                Creator = request.Creator is not null ? new UserResp(request.Creator, false) : null;
                Approver = request.Approver is not null ? new UserResp(request.Approver, false) : null;
                BinFrom = request.BinFrom is not null ? new BinResp(request.BinFrom, false) : null;
                BinTo = request.BinTo is not null ? new BinResp(request.BinTo, false) : null;
                Customer = request.Customer is not null ? new CustomerResp(request.Customer, false) : null;
                SupplierCustomerName = request.Customer?.CustomerName;
                Components = request.TransferRequestComponents?.Select(i => new TransferRequestComponentResp(i, true)).ToList();
            }
        }
    }
}
