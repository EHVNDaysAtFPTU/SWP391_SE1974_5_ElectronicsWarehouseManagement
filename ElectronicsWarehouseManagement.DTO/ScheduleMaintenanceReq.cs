using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class ScheduleMaintenanceReq : IVerifiableRequest
    {
        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; }

        [JsonPropertyName("minutes_before_shutdown")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? MinutesBeforeShutdown { get; set; }

        public bool Verify(out string failedReason)
        {
            if (MinutesBeforeShutdown is not null && MinutesBeforeShutdown <= 0)
            {
                failedReason = "Minutes before shutdown must be greater than 0.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
