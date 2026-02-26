using Azure.Core;
using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace ElectronicsWarehouseManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/manager")]
    [Authorize(Roles ="2")]
    public class ManagerCtrl:ControllerBase
    {
        readonly IManagerService _managerService;
        private readonly ILogger<ManagerCtrl> _logger;
        public ManagerCtrl(IManagerService managerService, ILogger<ManagerCtrl> logger)
        {
            _managerService = managerService;
            _logger = logger;
        }
        [HttpGet("get-item/{componentId:int}")]
        public async Task<IActionResult> GetItem([FromRoute]int componentId, [FromQuery] bool fullInfo)
        {
            var result = await _managerService.GetComponentAsync(componentId, fullInfo);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        [HttpGet("get-items")]
        public async Task<IActionResult> GetItemList([FromQuery] PagingRequest request)
        {
            var result = await _managerService.GetComponentListAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("get-transfer/{transferId:int}")]
        public async Task<IActionResult> GetTransfer([FromRoute] int transferId)
        {
            var result = await _managerService.GetTransferAsync(transferId);

            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("get-transfers")]
        public async Task<IActionResult> GetTransferReqList([FromQuery] PagingRequest request)
        {
            var result = await _managerService.GetTransferReqListAsync(request); 
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        //[HttpPost("transfer-requests/{transferId:int}/decisions")]
        //public async Task<IActionResult> PostTransferReq([FromRoute] int transferId, [FromBody] TransferDecisionRequest request)
        //{
        //    //int? approverId = GetCurrentUserId();
        //    //_logger.LogInformation("CurrentUserId (Session): {userId}", approverId);
        //    //_logger.LogInformation("Authenticated: {auth}", User.Identity?.IsAuthenticated);
        //    //_logger.LogInformation("Role: {role}", User.FindFirst(ClaimTypes.Role)?.Value);
        //    //_logger.LogInformation("ModelState valid: {valid}", ModelState.IsValid);
        //    //_logger.LogInformation("TransferId: {id}", transferId);
        //    //_logger.LogInformation("Request null: {nullCheck}", request == null);
        //    //_logger.LogInformation("Decision: {decision}", request?.Decision);
        //    var result = await _managerService.PostTransferDecisionAsync(transferId, request.Decision);
        //    if (result.Success)
        //    {
        //        return Ok(result);
        //    }
        //    return BadRequest(result);
        //}



        //[HttpGet("me")]
        //public IActionResult Me()
        //{
        //    string? username = HttpContext.Session.GetString("User");
        //    string? userId = HttpContext.Session.GetString("UserId");

        //    if (string.IsNullOrEmpty(username))
        //    {
        //        return Unauthorized(new ApiResult(ApiResultCode.Unauthorized));
        //    }

        //    var userInfo = new
        //    {
        //        Username = username,
        //        UserId = userId
        //    };

        //    return Ok(new ApiResult<object>(userInfo));
        //}

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return null;

            if (int.TryParse(userIdClaim.Value, out int userId))
                return userId;

            return null;
        }


        private String? GetCurrentUsername()
        {
            return HttpContext.Session.GetString("User");
        }

       

        //[HttpPost("{id:int}/approve")]
        //public async Task<IActionResult> PostTransferApprove(int id)
        //{

        //}

    }
}
