using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElectronicsWarehouseManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LoginCtrl : ControllerBase
    {
        readonly IAuthService _authService;
        private readonly ILogger<LoginCtrl> _logger;

        public LoginCtrl(IAuthService authService, ILogger<LoginCtrl> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReq request)
        {
            if (HttpContext.Session.GetString("User") != null)
                return BadRequest(new ApiResult(ApiResultCode.AlreadyLoggedIn));
            var result = await _authService.LoginAsync(request);
            if (result.resp.Success && result.user != null)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, result.user.Username),
                    new(ClaimTypes.Email, result.user.Email)
                };
                foreach (var role in result.user.Roles.OrderBy(r => r.RoleId))
                {
                    claims.Add(new Claim(ClaimTypes.Role, $"{role.RoleId}"));
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, role.RoleName));
                }
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                HttpContext.Session.SetString("User", result.user.Username);
                HttpContext.Session.SetString("UserId", result.user.UserId.ToString());
                return Ok(result.resp);
            }
            if (result.user is null && result.resp.ResultCode == ApiResultCode.NotFound)
                return NotFound(result.resp);
            return BadRequest(result.resp);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _authService.LogoutAsync(id);
            if (result.Success)
            {
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordReq request)
        {
            var result = await _authService.ForgotPasswordAsync(request, $"{Request.Scheme}://{Request.Host}");
            if (result.Success)
                return Ok(result);
            if (result.ResultCode == ApiResultCode.NotFound)
                return NotFound(result);
            return BadRequest(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordReq request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (result.Success)
                return Ok(result);
            if (result.ResultCode == ApiResultCode.NotFound)
                return NotFound(result);
            return BadRequest(result);
        }
    }
}