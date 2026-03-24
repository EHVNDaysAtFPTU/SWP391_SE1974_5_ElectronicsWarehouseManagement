using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ElectronicsWarehouseManagement.WebAPI.Services;

namespace ElectronicsWarehouseManagement.WebAPI.Filters
{
    public class MaintenanceFilter : IAsyncActionFilter
    {
        private readonly IAdminService _adminService;

        public MaintenanceFilter(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var result = await _adminService.GetSystemConfigAsync();

            if (!result.Success)
            {
                await next();
                return;
            }

            var cfg = result.Data;
            var now = DateTime.Now;

            bool isMaintenance = false;

            // bật tay
            if (cfg.MaintenanceMode)
                isMaintenance = true;

            // theo giờ
            if (!isMaintenance &&
                cfg.MaintenanceStartTime.HasValue &&
                cfg.MaintenanceEndTime.HasValue)
            {
                if (now >= cfg.MaintenanceStartTime && now <= cfg.MaintenanceEndTime)
                    isMaintenance = true;
            }

            if (isMaintenance)
            {
                var path = context.HttpContext.Request.Path.Value?.ToLower();

                // ✅ whitelist API
                if (path != null && (
                    path.Contains("/api/admin/config") ||
                    path.StartsWith("/api/auth")
                ))
                {
                    await next();
                    return;
                }
                if (context.HttpContext.User.IsInRole("1"))
                {
                    await next();
                    return;
                }

                // ❌ block user thường
                context.Result = new ObjectResult(new
                {
                    message = string.IsNullOrEmpty(cfg.MaintenanceMessage)
                        ? "System is under maintenance"
                        : cfg.MaintenanceMessage
                })
                {
                    StatusCode = 503
                };

                return;
            }

            await next();
        }
    }
}