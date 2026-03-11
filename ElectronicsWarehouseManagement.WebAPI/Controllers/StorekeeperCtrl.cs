using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicsWarehouseManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/storekeeper")]
    [Authorize(Roles = "3")]
    public class StorekeeperCtrl : ControllerBase
    {
        readonly IStorekeeperService _storekeeperService;

        private readonly ILogger<StorekeeperCtrl> _logger;

        public StorekeeperCtrl(IStorekeeperService storekeeperService, ILogger<StorekeeperCtrl> logger)
        {
            _storekeeperService = storekeeperService;
            _logger = logger;
        }

        [HttpPost("upload-image")]
        [RequestFormLimits(MultipartBodyLengthLimit = 1024 * 1024 * 10)]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            var result = await _storekeeperService.UploadImageAsync(image);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("components")]
        public async Task<IActionResult> GetComponents()
        {
            var result = await _storekeeperService.GetComponentsAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("components/{itemId:int}")]
        public async Task<IActionResult> GetComponent([FromRoute] int itemId)
        {
            var result = await _storekeeperService.GetComponentAsync(itemId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("create-component")]
        public async Task<IActionResult> CreateComponent([FromBody] CreateComponentReq request)
        {
            var result = await _storekeeperService.CreateComponentAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("component-categories")]
        public async Task<IActionResult> GetComponentCategories()
        {
            var result = await _storekeeperService.GetComponentCategoriesAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("create-component-category")]
        public async Task<IActionResult> CreateComponentCategory([FromBody] string categoryName)
        {
            var result = await _storekeeperService.CreateComponentCategoryAsync(categoryName);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("component-bins")]
        public async Task<IActionResult> GetComponentInBins()
        {
            var result = await _storekeeperService.GetComponentsInBinsAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("component-bins/{itemId:int}")]
        public async Task<IActionResult> GetComponentInBin([FromRoute] int itemId)
        {
            var result = await _storekeeperService.GetComponentInBinAsync(itemId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("create-warehouse")]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseReq request)
        {
            var result = await _storekeeperService.CreateWarehouseAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("warehouses")]
        public async Task<IActionResult> GetWarehouseList()
        {
            var result = await _storekeeperService.GetWarehouseListAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("warehouses/{warehouseId:int}")]
        public async Task<IActionResult> GetWarehouseById([FromRoute] int warehouseId)
        {
            var result = await _storekeeperService.GetWarehouseAsync(warehouseId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("create-bin")]
        public async Task<IActionResult> CreateBin([FromBody] CreateBinReq request)
        {
            var result = await _storekeeperService.CreateBinAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bins")]
        public async Task<IActionResult> GetBins()
        {
            var result = await _storekeeperService.GetBinsAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bins/{binId:int}")]
        public async Task<IActionResult> GetBin([FromRoute] int binId)
        {
            var result = await _storekeeperService.GetBinAsync(binId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("create-inbound")]
        public async Task<IActionResult> CreateInboundRequest([FromBody] CreateTransferRequestReq request)
        {
            return await CreateTransferRequest(request, TransferType.Inbound);
        }

        [HttpPost("create-outbound")]
        public async Task<IActionResult> CreateOutboundRequest([FromBody] CreateTransferRequestReq request)
        {
            return await CreateTransferRequest(request, TransferType.Outbound);
        }

        [HttpPost("create-transfer")]
        public async Task<IActionResult> CreateInternalTransferRequest([FromBody] CreateTransferRequestReq request)
        {
            return await CreateTransferRequest(request, TransferType.InternalTransfer);
        }
        
        [HttpPost("confirm-transfer")]
        public async Task<IActionResult> ConfirmTransferRequest([FromBody] ConfirmTransferRequestReq request)
        {
            var uid = GetUserId();
            if (!uid.HasValue)
                return BadRequest(new { code = 0, msg = "User not authenticated or session expired.", success = false });
            var result = await _storekeeperService.ConfirmTransferRequestAsync(request, uid.Value);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("transfers")]
        public async Task<IActionResult> GetTransferRequests()
        {
            var result = await _storekeeperService.GetTransferRequestsAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("transfers/{transferId:int}")]
        public async Task<IActionResult> GetTransferRequest([FromRoute] int transferId)
        {
            var result = await _storekeeperService.GetTransferRequestAsync(transferId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPatch("bin/{binId:int}/status")]
        public async Task<IActionResult> UpdateBinStatus([FromRoute] int binId, [FromBody] int status)
        {
            var result = await _storekeeperService.UpdateBinStatusAsync(binId, (BinStatus)status);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        async Task<IActionResult> CreateTransferRequest(CreateTransferRequestReq request, TransferType type)
        {
            var uid = GetUserId();
            if (!uid.HasValue)
                return BadRequest(new { code = 0, msg = "User not authenticated or session expired.", success = false });
            var result = await _storekeeperService.CreateTransferRequestAsync(request, type, uid.Value);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        int? GetUserId()
        {
            // Try session first
            try
            {
                var s = HttpContext.Session.GetString("UserId");
                if (!string.IsNullOrEmpty(s) && int.TryParse(s, out var id)) return id;
            }
            catch { }

            // Fallback to claims
            try
            {
                var claim = HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? HttpContext.User?.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out var cid)) return cid;
            }
            catch { }

            return null;
        }
    }
}