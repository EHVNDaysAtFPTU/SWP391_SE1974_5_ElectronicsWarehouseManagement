namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public static class SystemRuntimeConfig
    {
        public static bool MaintenanceMode { get; set; } = false;

        public static string? MaintenanceMessage { get; set; } = "System is under maintenance";

        public static DateTime? ScheduledEnd { get; set; }
    }
}
