using Azure.Core;
using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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
        [HttpGet("get-item/{itemId:int}")]
        public async Task<IActionResult> GetItem([FromRoute]int itemId)
        {
            var result = await _managerService.GetItemAsync(itemId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        [HttpGet("get-items")]
        public async Task<IActionResult> GetItemList([FromQuery] PagingRequest request)
        {
            var result = await _managerService.GetItemListAsync(request);

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
        //[HttpPost("{id:int}/approve")]
        //public async Task<IActionResult> PostApprove(int id)
        //{

        //}

    }
}
