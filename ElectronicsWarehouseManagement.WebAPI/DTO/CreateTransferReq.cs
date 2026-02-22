using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CreateTransferReq : IVerifiableRequest
    {
        [JsonPropertyName("item_ids")]
        public int[] ItemIds { get; set; } = [];

        [JsonPropertyName("desc")]
        public string Description { get; set; } = "";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_from_id")]
        public int? WarehouseFromId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_to_id")]
        public int? WarehouseToId { get; set; }

        [JsonIgnore]
        internal TransferType Type { get; set; }

        public bool Verify(out string failedReason)
        {
            if (ItemIds.Length == 0)
            {
                failedReason = "Item list cannot be empty.";
                return false;
            }
            switch (Type)
            {
                case TransferType.InternalTransfer:
                    if (WarehouseFromId == null || WarehouseToId == null)
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
                    if (WarehouseToId == null)
                    {
                        failedReason = "Destination warehouse must be specified.";
                        return false;
                    }
                    break;
                case TransferType.Outbound:
                    if (WarehouseFromId == null)
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
