using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IStorekeeperService
    {
        Task<ApiResult<string>> UploadImageAsync(IFormFile image);
        Task<ApiResult<List<BinResp>>> GetBinsByWarehouseAsync(int warehouseId);
        Task<ApiResult<List<ComponentResp>>> GetComponentsAsync();
        Task<ApiResult<ComponentResp>> GetComponentAsync(int componentId);
        Task<ApiResult<ComponentResp>> CreateComponentAsync(CreateComponentReq request);

        Task<ApiResult<List<ComponentCategoryResp>>> GetComponentCategoriesAsync();
        Task<ApiResult<ComponentCategoryResp>> GetComponentCategoryAsync(int categoryId);
        Task<ApiResult<ComponentCategoryResp>> CreateComponentCategoryAsync(string categoryName);

        Task<ApiResult<List<ComponentBinResp>>> GetComponentsInBinsAsync();
        Task<ApiResult<ComponentBinResp>> GetComponentInBinAsync(int componentId);

        Task<ApiResult<List<BinResp>>> GetBinsAsync();
        Task<ApiResult<BinResp>> GetBinAsync(int binId);
        Task<ApiResult<BinResp>> CreateBinAsync(CreateBinReq request);

        Task<ApiResult<List<WarehouseResp>>> GetWarehouseListAsync();
        Task<ApiResult<WarehouseResp>> GetWarehouseAsync(int warehouseId);
        Task<ApiResult<WarehouseResp>> CreateWarehouseAsync(CreateWarehouseReq request);

        Task<ApiResult<List<TransferRequestResp>>> GetTransferRequestListAsync();
        Task<ApiResult<TransferRequestResp>> GetTransferRequestAsync(int requestId);
        Task<ApiResult<TransferRequestResp>> CreateTransferRequestAsync(CreateTransferRequestReq request, TransferType type, int creatorId);
        Task<ApiResult<TransferRequestResp>> ConfirmTransferRequestAsync(ConfirmTransferRequestReq request, int approverId);
    }

    public class StorekeeperService : IStorekeeperService
    {
        readonly EWMDbCtx _dbCtx;

        public StorekeeperService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }
        public async Task<ApiResult<List<BinResp>>> GetBinsByWarehouseAsync(int warehouseId)
        {
            if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == warehouseId))
                return new ApiResult<List<BinResp>>(ApiResultCode.NotFound,
                    $"Warehouse with ID '{warehouseId}' does not exist.");

            var bins = await _dbCtx.Bins
                .AsNoTracking()
                .Where(b => b.WarehouseId == warehouseId)
                .Select(b => new BinResp(b, false))
                .ToListAsync();

            return new ApiResult<List<BinResp>>(bins);
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


        public async Task<ApiResult<List<ComponentResp>>> GetComponentsAsync()
        {
            var components = await _dbCtx.Components.AsNoTracking().Select(c => new ComponentResp(c, false)).ToListAsync();
            return new ApiResult<List<ComponentResp>>(components);
        }

        public async Task<ApiResult<ComponentResp>> GetComponentAsync(int componentId)
        {
            var component = await _dbCtx.Components.AsNoTracking().Where(c => c.ComponentId == componentId).Select(i => new ComponentResp(i, true)).FirstOrDefaultAsync();
            if (component is null)
                return new ApiResult<ComponentResp>(ApiResultCode.NotFound);
            return new ApiResult<ComponentResp>(component);
        }

        public async Task<ApiResult<ComponentResp>> CreateComponentAsync(CreateComponentReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult<ComponentResp>(ApiResultCode.InvalidRequest, failedReason);
            List<ComponentCategory> categories = [];
            foreach (var id in request.CategoryIds)
            {
                var category = await _dbCtx.ComponentCategories.FindAsync(id);
                if (category is null)
                    return new ApiResult<ComponentResp>(ApiResultCode.InvalidRequest, $"Component category with ID '{id}' does not exist.");
                categories.Add(category);
            }
            var component = new Component
            {
                Unit = request.Unit,
                UnitPrice = request.UnitPrice,
                Metadata = new ComponentMetadata()
                {
                    Name = request.Name,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    Manufacturer = request.Manufacturer,
                    ManufacturingDate = request.ManufacturingDate,
                    DatasheetUrl = request.DatasheetUrl
                },
                Categories = categories
            };
            _dbCtx.Components.Add(component);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult<ComponentResp>(new ComponentResp(component, true));
        }


        public async Task<ApiResult<List<ComponentCategoryResp>>> GetComponentCategoriesAsync()
        {
            var categories = await _dbCtx.ComponentCategories.AsNoTracking().Select(cc => new ComponentCategoryResp(cc)).ToListAsync();
            return new ApiResult<List<ComponentCategoryResp>>(categories);
        }

        public async Task<ApiResult<ComponentCategoryResp>> GetComponentCategoryAsync(int categoryId)
        {
            var category = await _dbCtx.ComponentCategories.AsNoTracking().Where(cc => cc.CategoryId == categoryId).Select(c => new ComponentCategoryResp(c)).FirstOrDefaultAsync();
            if (category is null)
                return new ApiResult<ComponentCategoryResp>(ApiResultCode.NotFound);
            return new ApiResult<ComponentCategoryResp>(category);
        }

        public async Task<ApiResult<ComponentCategoryResp>> CreateComponentCategoryAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return new ApiResult<ComponentCategoryResp>(ApiResultCode.InvalidRequest, "Name is required.");
            if (await _dbCtx.ComponentCategories.AnyAsync(cc => cc.CategoryName == categoryName))
                return new ApiResult<ComponentCategoryResp>(ApiResultCode.InvalidRequest, $"Component category with name '{categoryName}' already exists.");
            var category = new ComponentCategory { CategoryName = categoryName };
            _dbCtx.ComponentCategories.Add(category);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult<ComponentCategoryResp>(new ComponentCategoryResp(category));
        }


        public async Task<ApiResult<List<ComponentBinResp>>> GetComponentsInBinsAsync() => new ApiResult<List<ComponentBinResp>>((List<ComponentBinResp>?)await _dbCtx.ComponentBins.AsNoTracking().Select(cb => new ComponentBinResp(cb, false)).ToListAsync());

        public async Task<ApiResult<ComponentBinResp>> GetComponentInBinAsync(int componentId)
        {
            var component = await _dbCtx.ComponentBins.AsNoTracking().Where(i => i.ComponentId == componentId).Select(i => new ComponentBinResp(i, true)).FirstOrDefaultAsync();
            if (component is null)
                return new ApiResult<ComponentBinResp>(ApiResultCode.NotFound);
            return new ApiResult<ComponentBinResp>(component);
        }


        public async Task<ApiResult<List<BinResp>>> GetBinsAsync()
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

        public async Task<ApiResult<BinResp>> CreateBinAsync(CreateBinReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult<BinResp>(ApiResultCode.InvalidRequest, failedReason);
            if (!await _dbCtx.Warehouses.AnyAsync(w => w.WarehouseId == request.WarehouseID))
                return new ApiResult<BinResp>(ApiResultCode.InvalidRequest, $"Warehouse with ID '{request.WarehouseID}' does not exist.");
            var bin = new Bin
            {
                WarehouseId = request.WarehouseID,
                LocationInWarehouse = request.LocationInWarehouse,
                Status = BinStatus.Empty
            };
            _dbCtx.Bins.Add(bin);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult<BinResp>(new BinResp(bin, true));
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

        public async Task<ApiResult<WarehouseResp>> CreateWarehouseAsync(CreateWarehouseReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult<WarehouseResp>(ApiResultCode.InvalidRequest, failedReason);
            var warehouse = new Warehouse
            {
                WarehouseName = request.Name,
                Description = request.Description,
                PhysicalLocation = request.PhysicalLocation,
                ImageUrl = request.ImageUrl
            };
            _dbCtx.Warehouses.Add(warehouse);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult<WarehouseResp>(new WarehouseResp(warehouse, true));
        }


        public async Task<ApiResult<List<TransferRequestResp>>> GetTransferRequestListAsync()
        {
            var transferReqs = await _dbCtx.TransferRequests
                 .Include(tr => tr.BinFrom)
        .ThenInclude(b => b.Warehouse)
                .Include(tr => tr.Creator)
                .Include(tr => tr.BinTo)
                    .ThenInclude(b => b.Warehouse)
                .Include(tr => tr.TransferRequestComponents)
                    .ThenInclude(c => c.Component)
                .AsNoTracking()
                .Select(tr => new TransferRequestResp(tr, true))
                .ToListAsync();

            return new ApiResult<List<TransferRequestResp>>(transferReqs);
        }

        public async Task<ApiResult<TransferRequestResp>> GetTransferRequestAsync(int requestId)
        {
            var transfer = await _dbCtx.TransferRequests
                 .Include(tr => tr.BinFrom)
        .ThenInclude(b => b.Warehouse)
                .Include(tr => tr.Creator)
                .Include(tr => tr.BinTo)
                    .ThenInclude(b => b.Warehouse)
                .Include(tr => tr.TransferRequestComponents)
                    .ThenInclude(c => c.Component)
                .AsNoTracking()
                .FirstOrDefaultAsync(tr => tr.RequestId == requestId);

            if (transfer is null)
                return new ApiResult<TransferRequestResp>(ApiResultCode.NotFound);

            return new ApiResult<TransferRequestResp>(new TransferRequestResp(transfer, true));
        }

        public async Task<ApiResult<TransferRequestResp>> CreateTransferRequestAsync(CreateTransferRequestReq request, TransferType type, int creatorId)
        {
            request.Type = type;
            if (!request.Verify(out string failedReason))
                return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, failedReason);
            switch (type)
            {
                case TransferType.Inbound:
                    if (!await _dbCtx.Bins.AnyAsync(w => w.BinId == request.BinToId))
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinToId}' does not exist.");
                    break;
                case TransferType.Outbound:
                    if (!await _dbCtx.Bins.AnyAsync(w => w.BinId == request.BinFromId))
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' does not exist.");
                    break;
                case TransferType.InternalTransfer:
                    if (!await _dbCtx.Bins.AnyAsync(w => w.BinId == request.BinFromId))
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' does not exist.");
                    if (!await _dbCtx.Bins.AnyAsync(w => w.BinId == request.BinToId))
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinToId}' does not exist.");
                    break;
            }
            List<TransferRequestComponent> tComponents = [];
            foreach (var componentReq in request.Components)
            {
                var component = await _dbCtx.Components.FindAsync(componentReq.ComponentId);
                if (component is null)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Component with ID '{componentReq.ComponentId}' does not exist.");
                tComponents.Add(new TransferRequestComponent
                {
                    ComponentId = componentReq.ComponentId,
                    Quantity = componentReq.Quantity,
                    UnitPrice = componentReq.UnitPrice
                });
            }
            var transferRequest = new TransferRequest
            {
                Description = request.Description,
                Type = type,
                Status = TransferStatus.Pending,
                CreationTime = DateTime.UtcNow,
                CreatorId = creatorId,
                BinFromId = request.BinFromId,
                BinToId = request.BinToId,
                TransferRequestComponents = tComponents,
            };
            _dbCtx.TransferRequests.Add(transferRequest);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult<TransferRequestResp>(new TransferRequestResp(transferRequest, true));
        }

        public async Task<ApiResult<TransferRequestResp>> ConfirmTransferRequestAsync(
      ConfirmTransferRequestReq request,
      int approverId)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult<TransferRequestResp>(
                    ApiResultCode.InvalidRequest, failedReason);

            var transferReq = await _dbCtx.TransferRequests
                .Include(tr => tr.TransferRequestComponents)
                .FirstOrDefaultAsync(tr => tr.RequestId == request.RequestId);

            if (transferReq is null)
                return new ApiResult<TransferRequestResp>(
                    ApiResultCode.NotFound,
                    $"Transfer request with ID '{request.RequestId}' does not exist.");

            if (transferReq.Status != TransferStatus.ApprovedAndWaitForConfirm)
                return new ApiResult<TransferRequestResp>(
                    ApiResultCode.InvalidRequest,
                    $"Transfer request with ID '{request.RequestId}' cannot be confirmed.");

            var originalComponents = transferReq.TransferRequestComponents.ToList();

            // ===== VERIFY QUANTITY =====
            var confirmDict = new Dictionary<int, double>();

            foreach (var binReq in request.Bins)
            {
                foreach (var comp in binReq.Components)
                {
                    if (!originalComponents.Any(c => c.ComponentId == comp.ComponentId))
                        return new ApiResult<TransferRequestResp>(
                            ApiResultCode.InvalidRequest,
                            $"Component ID {comp.ComponentId} not in original request.");

                    if (!confirmDict.ContainsKey(comp.ComponentId))
                        confirmDict[comp.ComponentId] = comp.Quantity;
                    else
                        confirmDict[comp.ComponentId] += comp.Quantity;
                }
            }

            foreach (var original in originalComponents)
            {
                if (!confirmDict.ContainsKey(original.ComponentId) ||
                    confirmDict[original.ComponentId] != original.Quantity)
                    return new ApiResult<TransferRequestResp>(
                        ApiResultCode.InvalidRequest,
                        $"Confirmed quantity mismatch for component {original.ComponentId}");
            }

            // ===== UPDATE STATUS =====
            transferReq.Status = TransferStatus.Confirmed;
            transferReq.ExecutionTime = DateTime.UtcNow;

            // ===== UPDATE BIN & AVERAGE PRICE =====
            foreach (var binReq in request.Bins)
            {
                var bin = await _dbCtx.Bins
                    .Include(b => b.ComponentBins)
                    .FirstAsync(b => b.BinId == binReq.BinId);

                foreach (var compReq in binReq.Components)
                {
                    var componentBin = bin.ComponentBins
                        .FirstOrDefault(cb => cb.ComponentId == compReq.ComponentId);

                    var component = await _dbCtx.Components
                        .FirstAsync(c => c.ComponentId == compReq.ComponentId);

                    double oldQty = componentBin?.Quantity ?? 0;
                    double importQty = compReq.Quantity;
                    double newQty = oldQty + importQty;

                    double oldPrice = component.UnitPrice;
                    double importPrice = originalComponents
                        .First(c => c.ComponentId == compReq.ComponentId)
                        .UnitPrice;

                    if (componentBin == null)
                    {
                        componentBin = new ComponentBin
                        {
                            BinId = bin.BinId,
                            ComponentId = compReq.ComponentId,
                            Quantity = importQty
                        };
                        _dbCtx.ComponentBins.Add(componentBin);
                    }
                    else
                    {
                        componentBin.Quantity = newQty;
                    }

                    if (newQty > 0)
                    {
                        component.UnitPrice =
                            ((oldPrice * oldQty) + (importPrice * importQty))
                            / newQty;
                    }
                }
            }

            await _dbCtx.SaveChangesAsync();

            return new ApiResult<TransferRequestResp>(
                new TransferRequestResp(transferReq, true));
        }
    }
}
