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

        [HttpGet("components")]
        public async Task<IActionResult> GetComponentList()
        {
            var result = await _storekeeperService.GetComponentListAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("components/{itemId:int}")]
        public async Task<IActionResult> GetComponentById([FromRoute] int itemId)
        {
            var result = await _storekeeperService.GetComponentByIdAsync(itemId);
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

        [HttpGet("component-categories")]
        public async Task<IActionResult> GetComponentCategories()
        {
            var result = await _storekeeperService.GetComponentCategoriesAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        //[HttpPost("create-inbound")]
        //public async Task<IActionResult> CreateInboundRequest([FromBody] CreateIobReq request)
        //{
        //    var result = await _storekeeperService.CreateInOutBoundRequestAsync(request, TransferType.Outbound, int.Parse(HttpContext.Session.GetString("UserId")!));
        //    if (result.Success)
        //        return Ok(result);
        //    return BadRequest(result);
        //}

        //[HttpPost("create-outbound")]
        //[HttpPost("create-transfer")]
        //[HttpPost("create-warehouse")]
        //[HttpPost("create-item-def")]
        //[HttpPost("create-category")]
        //[HttpGet("warehouses")]
        //[HttpGet("create-bin")]
        //[HttpGet("bins")]
        //[HttpGet("item-defs")]
        //[HttpGet("items")]
        //[HttpGet("categories")]

        [HttpPost("upload-image")]
        [RequestFormLimits(MultipartBodyLengthLimit = 1024 * 1024 * 10)]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            var result = await _storekeeperService.UploadImageAsync(image);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}