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
            return NotFound(result.resp);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("User");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new ApiResult());
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordReq request)
        {
            string username = HttpContext.Session.GetString("User") ?? "";
            ApiResult result = await _authService.ChangePasswordAsync(username, request);
            if (result.Success)
            {
                HttpContext.Session.Remove("User");
                HttpContext.Session.Remove("UserId");
                return Ok(result);
            }
            return BadRequest(result);
        }

        [Authorize]
        [HttpPost("change-login")]
        public async Task<IActionResult> ChangeLogin([FromBody] ChangeLoginReq request)
        {
            string username = HttpContext.Session.GetString("User") ?? "";
            ApiResult result = await _authService.ChangeLoginAsync(username, request);
            if (result.Success)
            {
                HttpContext.Session.Remove("User");
                return Ok(result);
            }
            return BadRequest(result);
        }

        //[Authorize]
        //[HttpGet("me")]
        //public IActionResult Me()
        //{
        //    //TODO: return user info
        //    return Ok(new ApiResult<object>());
        //}
    }
}