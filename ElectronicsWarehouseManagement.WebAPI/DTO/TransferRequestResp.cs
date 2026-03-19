using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class TransferRequestResp
    {
        public TransferRequestResp() { }

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
        [JsonPropertyName("components")]
        public List<TransferRequestComponentResp>? Components { get; set; }

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
            BinFromId = request.BinFromId;
            BinToId = request.BinToId;
            if (fullInfo)
            {
                Creator = new UserResp(request.Creator, false);
                if (request.ApproverId is not null)
                    Approver = new UserResp(request.Approver, false);
                if (request.BinFromId is not null)
                    BinFrom = new BinResp(request.BinFrom, false);
                if (request.BinToId is not null)
                    BinTo = new BinResp(request.BinTo, false);
                Components = request.TransferRequestComponents.Select(i => new TransferRequestComponentResp(i, false)).ToList();
            }
        }
    }
}
