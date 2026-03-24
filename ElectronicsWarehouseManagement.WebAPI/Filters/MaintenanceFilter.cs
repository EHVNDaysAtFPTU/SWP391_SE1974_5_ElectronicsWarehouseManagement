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

            bool isMaintenance = cfg.MaintenanceMode;

            if (isMaintenance)
            {
                var path = context.HttpContext.Request.Path.Value?.ToLower();

                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith("/api/auth") ||
                        path.StartsWith("/api/admin/config") ||
                        path.StartsWith("/login.html"))
                    {
                        await next();
                        return;
                    }
                }

                if (context.HttpContext.User.IsInRole("1"))
                {
                    await next();
                    return;
                }

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