using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ElectronicsWarehouseManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/manager")]
    [Authorize(Roles = "2")]
    public class ManagerCtrl:ControllerBase
    {
        readonly IManagerService _managerService;
        private readonly ILogger<ManagerCtrl> _logger;
        public ManagerCtrl(IManagerService managerService, ILogger<ManagerCtrl> logger)
        {
            _managerService = managerService;
            _logger = logger;
        }
        [HttpGet("itemlist")]
        public async Task<IActionResult> GetItemList()
        {
            var result = await _managerService.GetItemList(); if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("request")]
        public async Task<IActionResult> GetTransferReqList()
        {
            var result = await _managerService.GetTransferReqList(); 
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

    }
}
