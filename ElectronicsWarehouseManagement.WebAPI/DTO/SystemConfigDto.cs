namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class SystemConfigDto
    {
        public bool MaintenanceMode { get; set; }
        public string? MaintenanceMessage { get; set; }

        public DateTime? MaintenanceStartTime { get; set; }
        public DateTime? MaintenanceEndTime { get; set; }
    }
}
