using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class UpdateTransferRequestReq : IVerifiableRequest
    {
        [JsonPropertyName("components")]
        public List<TransferRequestComponentReq>? Components { get; set; }

        [JsonPropertyName("desc")]
        public string? Description { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_from_id")]
        public int? WarehouseFromId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("warehouse_to_id")]
        public int? WarehouseToId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("customer_id")]
        public int? CustomerId { get; set; }

        public bool Verify(out string failedReason)
        {
            if (Components is not null)
            {
                foreach (var component in Components)
                {
                    if (!component.Verify(out failedReason))
                        return false;
                }
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
