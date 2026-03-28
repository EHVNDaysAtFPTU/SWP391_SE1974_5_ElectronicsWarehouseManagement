namespace ElectronicsWarehouseManagement.DTO
{
    public class SystemConfigReq
    {
        public bool MaintenanceMode { get; set; }
        public string? MaintenanceMessage { get; set; }
        public DateTime? ScheduledEnd { get; set; }
    }
}
