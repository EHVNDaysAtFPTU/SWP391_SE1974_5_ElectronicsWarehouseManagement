using ElectronicsWarehouseManagement.WebAPI.DTO;
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

        [HttpPost("create-account")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccReq createAccReq)
        {
            ApiResult result = await _adminService.CreateAccountAsync(createAccReq);
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

        [HttpPost("set-role")]
        public async Task<IActionResult> SetRole([FromBody] SetRoleReq setRoleReq)
        {
            var result = await _adminService.SetRoleAsync(setRoleReq);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpDelete("delete-account/{userId:int}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] int userId)
        {
            var result = await _adminService.DeleteAccountAsync(userId, int.Parse(HttpContext.Session.GetString("UserId")!));
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUser([FromRoute] int userId)
        {
            var result = await _adminService.GetUserAsync(userId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            var result = await _adminService.SearchUsersAsync(query);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("set-status")]
        public async Task<IActionResult> SetStatus([FromBody] SetStatusReq setStatusReq)
        {
            var result = await _adminService.SetStatusAsync(setStatusReq);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
