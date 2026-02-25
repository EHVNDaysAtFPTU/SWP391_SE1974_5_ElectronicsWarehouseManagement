using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IStorekeeperService
    {
        Task<ApiResult<string>> UploadImageAsync(IFormFile image);
        Task<ApiResult<List<ItemDefResp>>> GetItemDefListAsync();
        Task<ApiResult<ItemDefResp>> GetItemDefAsync(int itemId);
        Task<ApiResult> CreateItemCategoryAsync(string categoryName);
        Task<ApiResult<List<CategoryResp>>> GetItemCategoriesAsync();
        Task<ApiResult> CreateItemDefAsync(CreateItemDefReq request);
        Task<ApiResult<List<ItemResp>>> GetItemListAsync();
        Task<ApiResult<ItemResp>> GetItemAsync(int itemId);
        Task<ApiResult> CreateBinAsync(CreateBinReq request);
        Task<ApiResult<List<BinResp>>> GetBinListAsync();
        Task<ApiResult<BinResp>> GetBinAsync(int binId);
        Task<ApiResult> CreateWarehouseAsync(CreateWarehouseReq request);
        Task<ApiResult<List<WarehouseResp>>> GetWarehouseListAsync();
        Task<ApiResult<WarehouseResp>> GetWarehouseAsync(int warehouseId);
        Task<ApiResult> CreateTransferRequestAsync(CreateTransferReq request, TransferType type, int creatorId);
        Task<ApiResult> ConfirmTransferRequestAsync(ConfirmTransferReq request, int approverId);
        Task<ApiResult<List<TransferReqResp>>> GetTransferRequestListAsync();
        Task<ApiResult<TransferReqResp>> GetTransferRequestAsync(int transferReqId);
    }

    public class StorekeeperService : IStorekeeperService
    {
        readonly EWMDbCtx _dbCtx;

        public StorekeeperService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }

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

        public async Task<ApiResult<List<ItemDefResp>>> GetItemDefListAsync()
        {
            var components = await _dbCtx.ItemDefinitions.AsNoTracking().Select(i => new ItemDefResp(i)).ToListAsync();
            return new ApiResult<List<ItemDefResp>>(components);
        }

        public async Task<ApiResult<ItemDefResp>> GetItemDefAsync(int itemId)
        {
            var component = await _dbCtx.ItemDefinitions.AsNoTracking().Where(i => i.ItemDefId == itemId).Select(i => new ItemDefResp(i)).FirstOrDefaultAsync();
            if (component is null)
                return new ApiResult<ItemDefResp>(ApiResultCode.NotFound);
            return new ApiResult<ItemDefResp>(component);
        }

        public async Task<ApiResult> CreateItemCategoryAsync(string categoryName)
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

        public async Task<ApiResult<List<CategoryResp>>> GetItemCategoriesAsync()
        {
            var categories = await _dbCtx.Categories.AsNoTracking().Select(c => new CategoryResp(c)).ToListAsync();
            return new ApiResult<List<CategoryResp>>(categories);
        }

        public async Task<ApiResult> CreateItemDefAsync(CreateItemDefReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            List<Category> categories = [];
            foreach (var id in request.CategoryIds)
            {
                var category = await _dbCtx.Categories.FindAsync(id);
                if (category is null)
                    return new ApiResult(ApiResultCode.InvalidRequest, $"Component category with ID '{id}' does not exist.");
                categories.Add(category);
            }
            var itemDef = new ItemDefinition
            {
                Unit = request.Unit,
                UnitPrice = request.UnitPrice,
                Metadata = new ComponentMetadata()
                {
                    Name = request.Name,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    Manufacter = request.Manufacter,
                    ManufactingDate = request.ManufactingDate,
                    DatasheetUrl = request.DatasheetUrl
                },
                Categories = categories
            };
            _dbCtx.ItemDefinitions.Add(itemDef);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<List<ItemResp>>> GetItemListAsync()
        {
            var items = await _dbCtx.Items.AsNoTracking().Select(i => new ItemResp(i, false)).ToListAsync();
            return new ApiResult<List<ItemResp>>(items);
        }

        public async Task<ApiResult<ItemResp>> GetItemAsync(int itemId)
        {
            var item = await _dbCtx.Items.AsNoTracking().Where(i => i.ItemId == itemId).Select(i => new ItemResp(i, true)).FirstOrDefaultAsync();
            if (item is null)
                return new ApiResult<ItemResp>(ApiResultCode.NotFound);
            return new ApiResult<ItemResp>(item);
        }

        public async Task<ApiResult> CreateWarehouseAsync(CreateWarehouseReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            var warehouse = new Warehouse
            {
                WarehouseName = request.Name,
                Description = request.Description,
                PhysicalLocation = request.PhysicalLocation,
                ImageUrl = request.ImageUrl
            };
            _dbCtx.Warehouses.Add(warehouse);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<List<WarehouseResp>>> GetWarehouseListAsync()
        {
            var warehouses = await _dbCtx.Warehouses.AsNoTracking().Select(w => new WarehouseResp(w, false)).ToListAsync();
            return new ApiResult<List<WarehouseResp>>(warehouses);
        }

        public async Task<ApiResult<WarehouseResp>> GetWarehouseAsync(int warehouseId)
        {
            var warehouse = await _dbCtx.Warehouses.AsNoTracking().Where(w => w.WarehouseId == warehouseId).Select(w => new WarehouseResp(w, true)).FirstOrDefaultAsync();
            if (warehouse is null)
                return new ApiResult<WarehouseResp>(ApiResultCode.NotFound);
            return new ApiResult<WarehouseResp>(warehouse);
        }

        public async Task<ApiResult> CreateBinAsync(CreateBinReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == request.WarehouseID))
                return new ApiResult(ApiResultCode.InvalidRequest, $"Warehouse with ID '{request.WarehouseID}' does not exist.");
            var bin = new Bin
            {
                WarehouseId = request.WarehouseID,
                LocationInWarehouse = request.LocationInWarehouse,
                Status = BinStatus.Empty
            };
            _dbCtx.Bins.Add(bin);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<List<BinResp>>> GetBinListAsync()
        {
            var bins = await _dbCtx.Bins.AsNoTracking().Select(b => new BinResp(b, false)).ToListAsync();
            return new ApiResult<List<BinResp>>(bins);
        }

        public async Task<ApiResult<BinResp>> GetBinAsync(int binId)
        {
            var bin = await _dbCtx.Bins.AsNoTracking().Where(b => b.BinId == binId).Select(b => new BinResp(b, true)).FirstOrDefaultAsync();
            if (bin is null)
                return new ApiResult<BinResp>(ApiResultCode.NotFound);
            return new ApiResult<BinResp>(bin);
        }

        public async Task<ApiResult> CreateTransferRequestAsync(CreateTransferReq request, TransferType type, int creatorId)
        {
            request.Type = type;
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            switch (type)
            {
                case TransferType.Inbound:
                    if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == request.WarehouseToId))
                        return new ApiResult(ApiResultCode.InvalidRequest, $"Warehouse with ID '{request.WarehouseToId}' does not exist.");
                    break;
                case TransferType.Outbound:
                    if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == request.WarehouseFromId))
                        return new ApiResult(ApiResultCode.InvalidRequest, $"Warehouse with ID '{request.WarehouseFromId}' does not exist.");
                    break;
                case TransferType.InternalTransfer:
                    if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == request.WarehouseFromId))
                        return new ApiResult(ApiResultCode.InvalidRequest, $"Warehouse with ID '{request.WarehouseFromId}' does not exist.");
                    if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == request.WarehouseToId))
                        return new ApiResult(ApiResultCode.InvalidRequest, $"Warehouse with ID '{request.WarehouseToId}' does not exist.");
                    break;
            }
            List<Item> items = [];
            foreach (var item in request.Items)
            {
                var itemDef = await _dbCtx.ItemDefinitions.FindAsync(item.ItemDefId);
                if (itemDef is null)
                    return new ApiResult(ApiResultCode.InvalidRequest, $"Item with ID '{item.ItemDefId}' does not exist.");
                items.Add(new Item
                {
                    ItemDefId = item.ItemDefId,
                    Quantity = item.Quantity,
                });
            }
            var transferReq = new TransferReq
            {
                Description = request.Description,
                Type = type,
                Status = TransferStatus.Pending,
                CreationDate = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatorId = creatorId,
                WarehouseFromId = request.WarehouseFromId,
                WarehouseToId = request.WarehouseToId,
                Items = items,
            };
            _dbCtx.TransferReqs.Add(transferReq);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult> ConfirmTransferRequestAsync(ConfirmTransferReq request, int approverId)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            var transferReq = await _dbCtx.TransferReqs.Include(tr => tr.Items).FirstOrDefaultAsync(tr => tr.TransferId == request.TransferId);
            if (transferReq is null)
                return new ApiResult(ApiResultCode.NotFound, $"Transfer request with ID '{request.TransferId}' does not exist.");
            if (transferReq.Status != TransferStatus.ApprovedAndWaitForConfirm)
                return new ApiResult(ApiResultCode.InvalidRequest, $"Transfer request with ID '{request.TransferId}' cannot be confirmed.");
            transferReq.Status = TransferStatus.Confirmed;
            transferReq.ApproverId = approverId;
            transferReq.ExecutionDate = DateOnly.FromDateTime(DateTime.UtcNow);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<List<TransferReqResp>>> GetTransferRequestListAsync()
        {
            var transferReqs = _dbCtx.TransferReqs.AsNoTracking().Select(tr => new TransferReqResp(tr, false)).ToList();
            return new ApiResult<List<TransferReqResp>>(transferReqs);
        }

        public async Task<ApiResult<TransferReqResp>> GetTransferRequestAsync(int transferReqId)
        {
            return new ApiResult<TransferReqResp>(await _dbCtx.TransferReqs.AsNoTracking().Where(tr => tr.TransferId == transferReqId).Select(tr => new TransferReqResp(tr, true)).FirstOrDefaultAsync());
        }
    }
}
