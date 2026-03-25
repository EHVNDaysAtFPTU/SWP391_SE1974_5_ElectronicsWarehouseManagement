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

   

                bool isAdmin = context.HttpContext.User?.IsInRole("1") ?? false;

                if (!isAdmin)
                {
                    context.Result = new ObjectResult(new
                    {
                        message = string.IsNullOrEmpty(cfg.MaintenanceMessage)
                            ? "System is under maintenance"
                            : cfg.MaintenanceMessage,
                        scheduledEnd = cfg.ScheduledEnd 
                    })
                    {
                        StatusCode = 503
                    };
                    return;
                }
            }
            await next();
        }
    }
}