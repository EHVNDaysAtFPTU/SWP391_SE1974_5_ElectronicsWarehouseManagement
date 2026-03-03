using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsWarehouseManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/me")]
    public class UserCtrl : ControllerBase
    {
        readonly IUserService _userService;

        private readonly ILogger<UserCtrl> _logger;

        public UserCtrl(IUserService authService, ILogger<UserCtrl> logger)
        {
            _userService = authService;
            _logger = logger;
        }

        [Authorize]
        [HttpPatch("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordReq request)
        {
            string username = HttpContext.Session.GetString("User") ?? "";
            ApiResult result = await _userService.ChangePasswordAsync(username, request);
            if (result.Success)
            {
                HttpContext.Session.Remove("User");
                HttpContext.Session.Remove("UserId");
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize]
        [HttpPatch("info")]
        public async Task<IActionResult> UpdateInfo([FromBody] UpdateInfoReq request)
        {
            string username = HttpContext.Session.GetString("User") ?? "";
            ApiResult<UserResp> result = await _userService.UpdateInfoAsync(username, request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [Authorize]
        [HttpGet("info")]
        public async Task<IActionResult> MyInfo()
        {
            string username = HttpContext.Session.GetString("User") ?? "";
            ApiResult<UserResp> result = await _userService.GetMyInfoAsync(username);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
