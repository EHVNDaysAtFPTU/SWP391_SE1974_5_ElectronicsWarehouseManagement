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
        [JsonPropertyName("bin_from_id")]
        public int? BinFromId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bin_to_id")]
        public int? BinToId { get; set; }

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
                    if (BinFromId is null || BinToId is null)
                    {
                        failedReason = "Missing source or destination warehouse for internal transfer.";
                        return false;
                    }
                    if (BinFromId == BinToId)
                    {
                        failedReason = "Source and destination warehouses cannot be the same.";
                        return false;
                    }
                    break;
                case TransferType.Inbound:
                    if (BinToId is null)
                    {
                        failedReason = "Destination warehouse must be specified.";
                        return false;
                    }
                    break;
                case TransferType.Outbound:
                    if (BinFromId is null)
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
