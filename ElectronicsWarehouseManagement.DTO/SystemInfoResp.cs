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
        public string VendorID { get; set; } = "";

        [JsonPropertyName("cpu_model")]
        public string CPUModel { get; set; } = "";

        [JsonPropertyName("p_cores")]
        public uint PhysicalCores { get; set; }

        [JsonPropertyName("l_cores")]
        public uint LogicalCores { get; set; }

        [JsonPropertyName("os")]
        public string OSName { get; set; } = "";

        [JsonPropertyName("os_ver")]
        public string OSVersion { get; set; } = "";

        [JsonPropertyName("machine_memory_usage")]
        public double MachineMemoryUsageMB { get; set; }

        [JsonPropertyName("machine_total_memory")]
        public ulong MachineTotalMemoryMB { get; set; }

        [JsonPropertyName("machine_memory_usage_paged")]
        public double MachineMemoryUsagePagedMB { get; set; }

        [JsonPropertyName("machine_total_memory_paged")]
        public ulong MachineMemoryTotalPagedMB { get; set; }

        [JsonPropertyName("machine_cpu_usage")]
        public ulong MachineCPUUsagePercent { get; set; }
    }
}
