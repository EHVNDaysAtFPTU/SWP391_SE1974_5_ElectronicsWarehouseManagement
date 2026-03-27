using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Helpers;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsWarehouseManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "1")]
    public class AdminCtrl : ControllerBase
    {
        readonly IAdminService _adminService;

        private readonly ILogger<AdminCtrl> _logger;

        public AdminCtrl(IAdminService adminService, ILogger<AdminCtrl> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpPost("users/create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserReq createUserReq)
        {
            ApiResult result = await _adminService.CreateUserAsync(createUserReq);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _adminService.GetRolesAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _adminService.GetUsersAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("users/count")]
        public async Task<IActionResult> GetUserCount()
        {
            var result = await _adminService.GetUserCountAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPatch("users/{userId:int}/role")]
        public async Task<IActionResult> SetRole([FromRoute] int userId, [FromBody] SetRoleReq setRoleReq)
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _adminService.SetRoleAsync(userId, setRoleReq, id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("users/{userId:int}/delete")]
        public async Task<IActionResult> DeleteUser([FromRoute] int userId)
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _adminService.DeleteUserAsync(userId, id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetUser([FromRoute] int userId)
        {
            var result = await _adminService.GetUserAsync(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("users/search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            var result = await _adminService.SearchUsersAsync(query);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPatch("users/{userId:int}/status")]
        public async Task<IActionResult> SetStatus([FromRoute] int userId, [FromBody] SetStatusReq setStatusReq)
        {
            var result = await _adminService.SetStatusAsync(userId, setStatusReq);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            SystemConfigHelper.CheckAndDisableMaintenance();
            return Ok(new ApiResult<object>(new
            {
                maintenanceMode = SystemRuntimeConfig.MaintenanceMode,
                maintenanceMessage = SystemRuntimeConfig.MaintenanceMessage,
                scheduledEnd = SystemRuntimeConfig.ScheduledEnd?.ToLocalTime().ToString("yyyy-MM-ddTHH:mm")
            }));
        }

        [HttpPost("config/save")]
        public IActionResult SaveConfig([FromBody] SystemConfigReq req)
        {
            // Validation: when enabling maintenance, scheduled end must be provided and in the future.
            if (req.MaintenanceMode)
            {
                if (!req.ScheduledEnd.HasValue)
                    return BadRequest(new ApiResult(ApiResultCode.InvalidRequest, "Scheduled end must be provided when enabling maintenance mode."));

                var scheduledLocal = DateTime.SpecifyKind(req.ScheduledEnd.Value, DateTimeKind.Local);
                var scheduledUtc = scheduledLocal.ToUniversalTime();

                if (scheduledUtc <= DateTime.UtcNow)
                    return BadRequest(new ApiResult(ApiResultCode.InvalidRequest, "Scheduled end must be in the future."));

                SystemRuntimeConfig.MaintenanceMode = true;
                SystemRuntimeConfig.ScheduledEnd = scheduledUtc;
            }
            else
            {
                SystemRuntimeConfig.MaintenanceMode = false;
                SystemRuntimeConfig.ScheduledEnd = null;
            }

            SystemRuntimeConfig.MaintenanceMessage = req.MaintenanceMessage;

            return Ok(new ApiResult());
        }

        [HttpPost("config/reset")]
        public IActionResult ResetConfig()
        {
            SystemRuntimeConfig.MaintenanceMode = false;
            SystemRuntimeConfig.MaintenanceMessage = "System is under maintenance";
            SystemRuntimeConfig.ScheduledEnd = null;

            return Ok(new ApiResult());
        }

    }
}
