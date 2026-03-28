using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class SystemInfoResp
    {
        [JsonPropertyName("memory_usage")]
        public double CurrentMemoryUsageMB { get; set; }

        [JsonPropertyName("memory_usage_paged")]
        public double CurrentMemoryUsagePagedMB { get; set; }

        [JsonPropertyName("cpu_usage")]
        public double CPUUsagePercent { get; set; }

        [JsonPropertyName("uptime")]
        public TimeSpan Uptime { get; set; }

        [JsonPropertyName("vendor")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VendorID { get; set; }

        [JsonPropertyName("cpu_model")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CPUModel { get; set; }

        [JsonPropertyName("p_cores")]
        public int PhysicalCores { get; set; }

        [JsonPropertyName("l_cores")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? LogicalCores { get; set; }

        [JsonPropertyName("os")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OSName { get; set; }

        [JsonPropertyName("os_ver")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OSVersion { get; set; }

        [JsonPropertyName("machine_memory_usage")]
        public double MachineMemoryUsageMB { get; set; }

        [JsonPropertyName("machine_total_memory")]
        public long MachineTotalMemoryMB { get; set; }

        [JsonPropertyName("machine_memory_usage_paged")]
        public double MachineMemoryUsagePagedMB { get; set; }
    }
}
