namespace ElectronicsWarehouseManagement.WebAPI
{
    public static class MaintenanceSchedule
    {
        public static bool IsMaintenance { get; private set; }

        public static string MaintenanceMessage { get; private set; } = "System will be down for maintenance in {0}. Please save your work and log out.";

        public static DateTime StartTime { get; private set; }

        public static TimeSpan DurationBeforeShutdown { get; private set; }

        public static void ScheduleMaintenance(string? message, TimeSpan? durationBeforeShutdown)
        {
            message ??= "System will be down for maintenance in {0}. Please save your work and log out.";
            durationBeforeShutdown ??= TimeSpan.FromMinutes(10);
            IsMaintenance = true;
            StartTime = DateTime.UtcNow;
            DurationBeforeShutdown = durationBeforeShutdown.Value;
            MaintenanceMessage = message;
            Task.Run(async () =>
            {
                await Task.Delay(DurationBeforeShutdown);
                Environment.Exit(0);
            });
        }
    }
}
