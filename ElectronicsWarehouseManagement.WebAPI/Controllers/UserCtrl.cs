using ElectronicsWarehouseManagement.DTO;
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

        private readonly ILogger<UserCtrl> _logger;

        public UserCtrl(IUserService userService, ILogger<UserCtrl> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [Authorize]
        [HttpPatch("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordReq request)
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            string username = HttpContext.Session.GetString("User") ?? "";
            ApiResult result = await _userService.ChangePasswordAsync(username, request);
            if (!result.Success)
                return BadRequest(result);
            AuthService.Logout(id);
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(result);
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
