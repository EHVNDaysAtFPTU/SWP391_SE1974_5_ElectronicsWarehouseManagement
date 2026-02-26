using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
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
        [HttpGet("bins/by-warehouse/{warehouseId:int}")]
        public async Task<IActionResult> GetBinsByWarehouse(int warehouseId)
        {
            var result = await _storekeeperService.GetBinsByWarehouseAsync(warehouseId);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
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
            var result = await _storekeeperService.ConfirmTransferRequestAsync(request, int.Parse(HttpContext.Session.GetString("UserId")!));
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("transfers")]
        public async Task<IActionResult> GetTransferRequests()
        {
            var result = await _storekeeperService.GetTransferRequestListAsync();
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

        async Task<IActionResult> CreateTransferRequest(CreateTransferRequestReq request, TransferType type)
        {
            var result = await _storekeeperService.CreateTransferRequestAsync(request, type, int.Parse(HttpContext.Session.GetString("UserId")!));
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}