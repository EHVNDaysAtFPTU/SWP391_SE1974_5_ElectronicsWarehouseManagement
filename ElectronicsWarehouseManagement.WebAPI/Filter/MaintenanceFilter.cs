using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ElectronicsWarehouseManagement.WebAPI.Filters
{
    public class MaintenanceFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            SystemConfigHelper.CheckAndDisableMaintenance();

            if (!SystemRuntimeConfig.MaintenanceMode)
            {
                await next();
                return;
            }

            var httpContext = context.HttpContext;
            var path = httpContext.Request.Path.Value?.ToLower();

            bool isAdmin = httpContext.User?.IsInRole("1") ?? false;

            bool isAllowedPath =
                path != null &&
                (
                    path.StartsWith("/api/auth/logout") ||
                    path.StartsWith("/view/shared") ||
                    path.StartsWith("/login")
#if DEBUG
                    || path.Contains("swagger")
#endif
                );

            if (!isAdmin && !isAllowedPath)
            {
                // If the request is an API call, return 503 with JSON so client-side API callers
                // can show a modal. If the request targets a view (html), redirect to the
                // static maintenance page so the user sees the full-page notice.
                if (path != null && path.StartsWith("/api"))
                {
                    context.Result = new ObjectResult(new
                    {
                        message = SystemRuntimeConfig.MaintenanceMessage ?? "System is under maintenance",
                        scheduledEnd = SystemRuntimeConfig.ScheduledEnd?.ToLocalTime().ToString("yyyy-MM-ddTHH:mm")
                    })
                    {
                        StatusCode = 503
                    };
                    return;
                }

                // Allow serving the maintenance page itself by redirecting to a shared view
                var scheduledLocal = SystemRuntimeConfig.ScheduledEnd?.ToLocalTime().ToString("yyyy-MM-ddTHH:mm");
                var msg = SystemRuntimeConfig.MaintenanceMessage ?? "System is under maintenance";
                var url = "/view/shared/maintenance.html";
                context.Result = new RedirectResult(url);
                return;
            }

            await next();
        }
    }
}