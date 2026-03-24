using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CreateTransferRequestReq : IVerifiableRequest
    {
        [JsonPropertyName("components")]
        public List<TransferRequestComponentReq> Components { get; set; } = [];

        [JsonPropertyName("desc")]
        public string Description { get; set; } = "";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_from_id")]
        public int? WarehouseFromId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_to_id")]
        public int? WarehouseToId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("customer_id")]
        public int? CustomerId { get; set; }

        [JsonIgnore]
        internal TransferType Type { get; set; }

        public bool Verify(out string failedReason)
        {
            if (Components.Count == 0)
            {
                failedReason = "Component list cannot be empty.";
                return false;
            }
            foreach (var component in Components)
            {
                if (!component.Verify(out failedReason))
                    return false;
            }
            switch (Type)
            {
                case TransferType.InternalTransfer:
                    if (WarehouseFromId is null || WarehouseToId is null)
                    {
                        failedReason = "Missing source or destination warehouse for internal transfer.";
                        return false;
                    }
                    if (WarehouseFromId == WarehouseToId)
                    {
                        failedReason = "Source and destination warehouses cannot be the same.";
                        return false;
                    }
                    break;
                case TransferType.Inbound:
                    if (WarehouseToId is null)
                    {
                        failedReason = "Destination warehouse must be specified.";
                        return false;
                    }
                    break;
                case TransferType.Outbound:
                    if (WarehouseFromId is null)
                    {
                        failedReason = "Source warehouse must be specified.";
                        return false;
                    }
                    break;
                default:
                    failedReason = "Invalid transfer type.";
                    return false;
            }
            failedReason = "";
            return true;
        }
    }
}
