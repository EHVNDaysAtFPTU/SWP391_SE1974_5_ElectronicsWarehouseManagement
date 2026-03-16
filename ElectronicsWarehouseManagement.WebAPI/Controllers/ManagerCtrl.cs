using Azure.Core;
using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;


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
        [HttpGet("get-component/{componentId:int}")]
        public async Task<IActionResult> GetComponent([FromRoute]int componentId, [FromQuery] bool fullInfo)
        {
            var result = await _managerService.GetComponentAsync(componentId, fullInfo);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        [HttpGet("get-components")]
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
        public async Task<IActionResult> GetTransfer([FromRoute] int transferId, [FromQuery] bool fullInfo)
        {
            var result = await _managerService.GetTransferAsync(transferId,fullInfo);

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

        [HttpPost("transfer-requests/{transferId:int}/decisions")]
        public async Task<IActionResult> PostTransferReq([FromRoute] int transferId, [FromBody] TransferDecisionRequest request)
        {
            string? userIdString = GetCurrentUserId();
            int? approverId = null;

            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int parsedId))
            {
                approverId = parsedId;
            }

            if (approverId == null)
            {
                return BadRequest("Invalid or missing UserId in session.");
            }

            var result = await _managerService.PostTransferDecisionAsync(transferId, request.Decision, approverId.Value);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }


        [HttpGet("get-bin/{binId:int}")]
        public async Task<IActionResult> GetBin([FromRoute] int binId, [FromQuery] bool fullInfo)
        {
            var result = await _managerService.GetBin(binId, fullInfo);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("get-warehouse-bins/{warehouseId:int}")]
        public async Task<IActionResult> GetBinListByWareHouseId([FromRoute] int warehouseId, [FromQuery] PagingRequest request, [FromQuery]bool fullInfo)
        {
            var result = await _managerService.GetBinListByWareHouseId(request,warehouseId, fullInfo);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        [HttpGet("get-bins")]
        public async Task<IActionResult> GetBinList([FromQuery]PagingRequest request)
        {
            var result = await _managerService.GetBinList(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpGet("get-warehouse/{warehouseId:int}")]
        public async Task<IActionResult> GetWarehouseList(int warehouseId, [FromQuery] bool fullInfo)
        {
            var result = await _managerService.GetWareHouseAsync(warehouseId,fullInfo);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpGet("get-warehouses")]
        public async Task<IActionResult> GetWarehouseList([FromQuery] PagingRequest request)
        {
            var result = await _managerService.GetWareHouseListAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        [HttpGet("get-statistics/summary")]
        public async Task<IActionResult> GetStatisticsSummary()
        {
            var result = await _managerService.GetSummaryAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        [HttpGet("get-statistics/charts")]
        public async Task<IActionResult> GetStatisticsChart()
        {
            var result = await _managerService.GetChartDataAsync(7);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);

        }
        [HttpGet("export/transfer{transferId:int}")]
        public async Task<IActionResult> ExportTransferReq([FromRoute] int transferId)
        {
            var file = await _managerService.ExportTransferPdfAsync(transferId);

            return File(
                file,
                "application/pdf",
                $"transfer_{transferId}.pdf"
            );
        }


        private String? GetCurrentUserId()
        {
            string? result = HttpContext.Session.GetString("UserId");
            if(result == null)
            {
                return null;
            }
            return result;

        }


        private String? GetCurrentUsername()
        {
            return HttpContext.Session.GetString("User");
        }

       

        

    }
}
