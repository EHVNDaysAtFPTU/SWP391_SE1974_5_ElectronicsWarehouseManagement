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

        [HttpGet("item-defs")]
        public async Task<IActionResult> GetItemDefList()
        {
            var result = await _storekeeperService.GetItemDefListAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("item-defs/{itemId:int}")]
        public async Task<IActionResult> GetItemDefById([FromRoute] int itemId)
        {
            var result = await _storekeeperService.GetItemDefByIdAsync(itemId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("create-item-def")]
        public async Task<IActionResult> CreateItemDef([FromBody] CreateItemDefReq request)
        {
            var result = await _storekeeperService.CreateItemDefAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("item-categories")]
        public async Task<IActionResult> GetItemCategories()
        {
            var result = await _storekeeperService.GetItemCategoriesAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("create-item-category")]
        public async Task<IActionResult> CreateItemCategory([FromBody] string categoryName)
        {
            var result = await _storekeeperService.CreateItemCategoryAsync(categoryName);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetItemList()
        {
            var result = await _storekeeperService.GetItemListAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("items/{itemId:int}")]
        public async Task<IActionResult> GetItemById([FromRoute] int itemId)
        {
            var result = await _storekeeperService.GetItemByIdAsync(itemId);
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
            var result = await _storekeeperService.GetWarehouseByIdAsync(warehouseId);
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
        public async Task<IActionResult> GetBinList()
        {
            var result = await _storekeeperService.GetBinListAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bins/{binId:int}")]
        public async Task<IActionResult> GetBinById([FromRoute] int binId)
        {
            var result = await _storekeeperService.GetBinByIdAsync(binId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("create-inbound")]
        public async Task<IActionResult> CreateInboundRequest([FromBody] CreateTransferReq request)
        {
            return await CreateTransferRequest(request, TransferType.Inbound);
        }

        [HttpPost("create-outbound")]
        public async Task<IActionResult> CreateOutboundRequest([FromBody] CreateTransferReq request)
        {
            return await CreateTransferRequest(request, TransferType.Outbound);
        }

        [HttpPost("create-transfer")]
        public async Task<IActionResult> CreateInternalTransferRequest([FromBody] CreateTransferReq request)
        {
            return await CreateTransferRequest(request, TransferType.InternalTransfer);
        }

        async Task<IActionResult> CreateTransferRequest(CreateTransferReq request, TransferType type)
        {
            var result = await _storekeeperService.CreateTransferRequestAsync(request, type, int.Parse(HttpContext.Session.GetString("UserId")!));
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}