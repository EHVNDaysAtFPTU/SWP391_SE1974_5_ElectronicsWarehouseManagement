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
            if (result.Success)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, request.UsernameOrEmail),
                    // You can add more claims here as needed
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                HttpContext.Session.SetString("User", request.UsernameOrEmail);
                return Ok(result);
            }
            return Unauthorized(result);
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
        [HttpGet("me")]
        public IActionResult Me()
        {
            //TODO: return user info
            return Ok(new ApiResult<object>());
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
                return Ok(result);
            }
            return BadRequest(result);
        }

        // TODO: reset password, etc.
    }
}
