using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ElectronicsWarehouseManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/me")]
    public class UserCtrl : ControllerBase
    {
    readonly IUserService _userService;
    readonly IAuthService _authService;

    private readonly ILogger<UserCtrl> _logger;

    public UserCtrl(IUserService authService, IAuthService authService2, ILogger<UserCtrl> logger)
    {
        _userService = authService;
        _authService = authService2;
        _logger = logger;
    }

        [Authorize]
        [HttpPatch("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordReq request)
        {
            string username = HttpContext.Session.GetString("User") ?? "";
            string? idStr = HttpContext.Session.GetString("UserId");
            ApiResult result = await _userService.ChangePasswordAsync(username, request);
            if (result.Success)
            {
                if (!string.IsNullOrWhiteSpace(idStr) && int.TryParse(idStr, out int uid))
                {
                    try
                    {
                        await _authService.LogoutAsync(uid);
                    }
                    catch { /* ignore errors */ }
                }

                // Clear session and sign out cookie
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
