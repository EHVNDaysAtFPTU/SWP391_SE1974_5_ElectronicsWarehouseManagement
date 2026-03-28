using ElectronicsWarehouseManagement.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ElectronicsWarehouseManagement.WebAPI.Filters
{
    public class MaintenanceFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!MaintenanceSchedule.IsMaintenance)
            {
                await next();
                return;
            }
            string path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";
            bool isAllowedPath = false;
            if (!string.IsNullOrWhiteSpace(path))
            {
                isAllowedPath = path.StartsWith("/api/auth/logout") || path.EndsWith("maintenance.html");
#if DEBUG
                isAllowedPath |= path.Contains("swagger");
#endif
            }
            if (isAllowedPath)
            {
                await next();
                return;
            }
            if (!path.StartsWith("/api"))
            {
                context.Result = new RedirectResult("/maintenance.html");
                return;
            }
            TimeSpan timeBeforeShutdown = MaintenanceSchedule.DurationBeforeShutdown - (DateTime.UtcNow - MaintenanceSchedule.StartTime);
            context.Result = new ObjectResult(new ApiResult(ApiResultCode.Maintenance, string.Format(MaintenanceSchedule.MaintenanceMessage, timeBeforeShutdown.ToString(@"hh\:mm\:ss")))) { StatusCode = StatusCodes.Status503ServiceUnavailable };
        }
    }
}