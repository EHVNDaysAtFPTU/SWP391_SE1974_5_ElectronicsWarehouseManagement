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
            {
                return BadRequest(new ApiResult(ApiResultCode.AlreadyLoggedIn, "Already logged in."));
            }
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
            var sessionUser = HttpContext.Session.GetString("User");
            return Ok(new ApiResult<object>(new { User = User.Identity?.Name, SessionUser = sessionUser }));
        }

        // TODO: change password, reset password, etc.
    }
}
