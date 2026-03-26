using ElectronicsWarehouseManagement.WebAPI.DTO;

namespace ElectronicsWarehouseManagement.WebAPI.Helpers
{
        public static class SystemConfigHelper
        {
            public static void CheckAndDisableMaintenance()
            {
                if (SystemRuntimeConfig.ScheduledEnd.HasValue &&
                    DateTime.UtcNow > SystemRuntimeConfig.ScheduledEnd.Value)
                {
                    SystemRuntimeConfig.MaintenanceMode = false;
                    SystemRuntimeConfig.ScheduledEnd = null;
                }
            }
        }
    }
