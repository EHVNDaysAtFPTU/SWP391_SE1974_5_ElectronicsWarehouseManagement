namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class SystemConfigDto
    {
        public bool MaintenanceMode { get; set; } = false;

        // Bắt buộc khi MaintenanceMode = true
        public DateTime? ScheduledEnd { get; set; }

        public string MaintenanceMessage { get; set; } = string.Empty;
    }
}
