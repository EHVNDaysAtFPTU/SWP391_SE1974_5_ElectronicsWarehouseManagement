using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IStorekeeperService
    {
        Task<ApiResult<List<ItemDTO>>> GetComponentListAsync();
        Task<ApiResult<ItemDTO>> GetComponentByIdAsync(int itemId);
        Task<ApiResult<string>> UploadImageAsync(IFormFile image);
        Task<ApiResult> CreateComponentCategoryAsync(string categoryName);
        Task<ApiResult<List<CategoryResp>>> GetComponentCategoriesAsync();
        //Task<ApiResult> CreateInOutBoundRequestAsync(CreateIobReq request, TransferType outbound, int creatorId);
    }

    public class StorekeeperService : IStorekeeperService
    {
        readonly EWMDbCtx _dbCtx;

        public StorekeeperService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<ApiResult<List<ItemDTO>>> GetComponentListAsync()
        {
            var components = await _dbCtx.Items.AsNoTracking().Select(i => new ItemDTO(i)).ToListAsync();
            return new ApiResult<List<ItemDTO>>(components);
        }

        public async Task<ApiResult<ItemDTO>> GetComponentByIdAsync(int itemId)
        {
            var component = await _dbCtx.Items.AsNoTracking().Where(i => i.ItemId == itemId).Select(i => new ItemDTO(i)).FirstOrDefaultAsync();
            if (component is null)
                return new ApiResult<ItemDTO>(ApiResultCode.NotFound);
            return new ApiResult<ItemDTO>(component);
        }

        public async Task<ApiResult> CreateComponentCategoryAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return new ApiResult(ApiResultCode.InvalidRequest, "Name is required.");
            if (await _dbCtx.Categories.AnyAsync(cc => cc.CategoryName == categoryName))
                return new ApiResult(ApiResultCode.InvalidRequest, $"Component category with name '{categoryName}' already exists.");
            var category = new Category { CategoryName = categoryName };
            _dbCtx.Categories.Add(category);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<List<CategoryResp>>> GetComponentCategoriesAsync()
        {
            var categories = await _dbCtx.Categories.AsNoTracking().Select(c => new CategoryResp(c)).ToListAsync();
            return new ApiResult<List<CategoryResp>>(categories);
        }

        //public async Task<ApiResult> CreateInOutBoundRequestAsync(CreateIobReq request, TransferType type, int creatorId)
        //{
        //    if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == request.WarehouseId))
        //        return new ApiResult(ApiResultCode.InvalidRequest, $"Warehouse with ID '{request.WarehouseId}' does not exist.");
        //    List<Item> items = [];
        //    foreach (var id in request.ItemIds)
        //    {
        //        var item = await _dbCtx.Items.FindAsync(id);
        //        if (item is null)
        //            return new ApiResult(ApiResultCode.InvalidRequest, $"Item with ID '{id}' does not exist.");
        //        items.Add(item);
        //    }
        //    var iob = new TransferReq
        //    {
        //        Description = request.Description,
        //        Type = type,
        //        Status = TransferStatus.Pending,
        //        CreationDate = DateOnly.FromDateTime(DateTime.UtcNow),
        //        CreatorId = creatorId
        //    };
        //    if (type == TransferType.Inbound)
        //    {
        //        iob.WarehouseToId = request.WarehouseId;
        //        iob.ItemInbounds = items;
        //    }
        //    else if (type == TransferType.Outbound)
        //    {
        //        iob.WarehouseFromId = request.WarehouseId;
        //        iob.ItemOutbounds = items;
        //    }
        //    else
        //        return new ApiResult(ApiResultCode.InvalidRequest, $"Transfer type '{type}' is not supported for this operation.");
        //    _dbCtx.TransferReqs.Add(iob);
        //    await _dbCtx.SaveChangesAsync();
        //    return new ApiResult();
        //}

        //public async Task<ApiResult> CreateTransferRequestAsync(CreateTransferReq request, int creatorId)
        //{
        //    if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == request.WarehouseFromId))
        //        return new ApiResult(ApiResultCode.InvalidRequest, $"Warehouse with ID '{request.WarehouseFromId}' does not exist.");
        //    if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == request.WarehouseToId))
        //        return new ApiResult(ApiResultCode.InvalidRequest, $"Warehouse with ID '{request.WarehouseToId}' does not exist.");
        //    List<Item> items = [];
        //    foreach (var id in request.ItemIds)
        //    {
        //        var item = await _dbCtx.Items.FindAsync(id);
        //        if (item is null)
        //            return new ApiResult(ApiResultCode.InvalidRequest, $"Item with ID '{id}' does not exist.");
        //        items.Add(item);
        //    }
        //    var iob = new TransferReq
        //    {
        //        Description = request.Description,
        //        Type = TransferType.InternalTransfer,
        //        Status = TransferStatus.Pending,
        //        CreationDate = DateOnly.FromDateTime(DateTime.UtcNow),
        //        CreatorId = creatorId,
        //        WarehouseFromId = request.WarehouseFromId,
        //        WarehouseToId = request.WarehouseToId,
        //        ItemTransfers = items,
        //    };
        //    _dbCtx.TransferReqs.Add(iob);
        //    await _dbCtx.SaveChangesAsync();
        //    return new ApiResult();
        //}

        public async Task<ApiResult<string>> UploadImageAsync(IFormFile image)
        {
            if (image is null || image.Length == 0)
                return new ApiResult<string>(ApiResultCode.InvalidRequest);
            var ext = Path.GetExtension(image.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".bmp" && ext != ".gif" && ext != ".webp")
                return new ApiResult<string>(ApiResultCode.InvalidRequest, $"Image format '{ext}' is not supported.");
            string filePath = Path.Combine("uploads", "img");
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            filePath = Path.Combine(filePath, $"{Guid.NewGuid()}{ext}");
            using (var stream = new FileStream(filePath, FileMode.Create))
                await image.CopyToAsync(stream);
            return new ApiResult<string>(filePath.Replace('\\', '/'));
        }
    }
}
