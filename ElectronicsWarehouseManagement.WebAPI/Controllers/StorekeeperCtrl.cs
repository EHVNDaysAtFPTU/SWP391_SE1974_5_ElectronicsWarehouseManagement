using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.DTO;
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

        [HttpGet("components")]
        public async Task<IActionResult> GetComponents()
        {
            var result = await _storekeeperService.GetComponentsAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("components/{componentId:int}")]
        public async Task<IActionResult> GetComponent([FromRoute] int componentId)
        {
            var result = await _storekeeperService.GetComponentAsync(componentId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("components/create")]
        public async Task<IActionResult> CreateComponent([FromBody] CreateComponentReq request)
        {
            var result = await _storekeeperService.CreateComponentAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("components/categories")]
        public async Task<IActionResult> GetComponentCategories()
        {
            var result = await _storekeeperService.GetComponentCategoriesAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("components/categories/create")]
        public async Task<IActionResult> CreateComponentCategory([FromBody] string categoryName)
        {
            var result = await _storekeeperService.CreateComponentCategoryAsync(categoryName);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("warehouses")]
        public async Task<IActionResult> GetWarehouseList()
        {
            var result = await _storekeeperService.GetWarehousesAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("warehouses/count")]
        public async Task<IActionResult> GetWarehouseCount()
        {
            var result = await _storekeeperService.GetWarehouseCountAsync();
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

        [HttpGet("warehouses/{warehouseId:int}/components")]
        public async Task<IActionResult> GetWarehouseComponents([FromRoute] int warehouseId)
        {
            var result = await _storekeeperService.GetComponentsInWarehouseAsync(warehouseId);
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

        [HttpGet("bins/count")]
        public async Task<IActionResult> GetBinCount()
        {
            var result = await _storekeeperService.GetBinCountAsync();
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

        [HttpPatch("bins/{binId:int}/status")]
        public async Task<IActionResult> UpdateBinStatus([FromRoute] int binId, [FromBody] int status)
        {
            var result = await _storekeeperService.UpdateBinStatusAsync(binId, (BinStatus)status);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bins/components")]
        public async Task<IActionResult> GetComponentInBins()
        {
            var result = await _storekeeperService.GetComponentsInBinsAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bins/components/count")]
        public async Task<IActionResult> GetComponentInBinCount()
        {
            var result = await _storekeeperService.GetComponentInBinCountAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("bins/components/{componentId:int}")]
        public async Task<IActionResult> GetComponentInBin([FromRoute] int componentId)
        {
            var result = await _storekeeperService.GetComponentInBinAsync(componentId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("transfers/inbound/create")]
        public async Task<IActionResult> CreateInboundRequest([FromBody] CreateTransferRequestReq request)
        {
            return await CreateTransferRequest(request, TransferType.Inbound);
        }

        [HttpPost("transfers/outbound/create")]
        public async Task<IActionResult> CreateOutboundRequest([FromBody] CreateTransferRequestReq request)
        {
            return await CreateTransferRequest(request, TransferType.Outbound);
        }

        [HttpPost("transfers/transfer/create")]
        public async Task<IActionResult> CreateInternalTransferRequest([FromBody] CreateTransferRequestReq request)
        {
            return await CreateTransferRequest(request, TransferType.InternalTransfer);
        }

        [HttpPost("transfers/confirm")]
        public async Task<IActionResult> ConfirmTransferRequest([FromBody] ConfirmTransferRequestReq request)
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _storekeeperService.ConfirmTransferRequestAsync(request, id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("transfers")]
        public async Task<IActionResult> GetTransferRequests()
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _storekeeperService.GetTransferRequestsAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("transfers/count")]
        public async Task<IActionResult> GetTransferRequestCount()
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _storekeeperService.GetTransferRequestCountAsync(id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("transfers/{transferId:int}")]
        public async Task<IActionResult> GetTransferRequest([FromRoute] int transferId)
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _storekeeperService.GetTransferRequestAsync(transferId, id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPatch("transfers/{transferId:int}")]
        public async Task<IActionResult> UpdateTransferRequest([FromRoute] int transferId, UpdateTransferRequestReq request)
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _storekeeperService.UpdateTransferRequestAsync(transferId, request, id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("transfers/{transferId:int}/cancel")]
        public async Task<IActionResult> CancelTransferRequest([FromRoute] int transferId)
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _storekeeperService.CancelTransferRequestAsync(transferId, id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var result = await _storekeeperService.GetCustomersAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("customers/count")]
        public async Task<IActionResult> GetCustomerCount()
        {
            var result = await _storekeeperService.GetCustomerCountAsync();
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("customers/{customerId:int}")]
        public async Task<IActionResult> UpdateCustomer([FromRoute] int customerId)
        {
            var result = await _storekeeperService.GetCustomerAsync(customerId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        async Task<IActionResult> CreateTransferRequest(CreateTransferRequestReq request, TransferType type)
        {
            string? idStr = HttpContext.Session.GetString("UserId");
            if (idStr is null || !int.TryParse(idStr, out int id))
                return Redirect("/");
            var result = await _storekeeperService.CreateTransferRequestAsync(request, type, id);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}