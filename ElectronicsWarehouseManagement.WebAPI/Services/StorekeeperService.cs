using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IStorekeeperService
    {
        Task<ApiResult<string>> UploadImageAsync(IFormFile image);
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

        Task<ApiResult<BinResp>> UpdateBinStatusAsync(int binId, BinStatus status);

        Task<ApiResult<List<TransferRequestResp>>> GetTransferRequestsAsync();
        Task<ApiResult<TransferRequestResp>> GetTransferRequestAsync(int requestId);
        Task<ApiResult<TransferRequestResp>> CreateTransferRequestAsync(CreateTransferRequestReq request, TransferType type, int creatorId);
        Task<ApiResult<TransferRequestResp>> ConfirmTransferRequestAsync(ConfirmTransferRequestReq request, int approverId);
    }

    public class StorekeeperService : IStorekeeperService
    {
        readonly EWMDbCtx _dbCtx;
        readonly ILogger<StorekeeperService> _logger;

        public StorekeeperService(EWMDbCtx dbCtx, ILogger<StorekeeperService> logger)
        {
            _dbCtx = dbCtx;
            _logger = logger;
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

            // Return full info so client can get total quantity/stock information (ComponentResp.Quantity)
            var components = await _dbCtx.Components.Include(c => c.ComponentBins).AsNoTracking().Select(c => new ComponentResp(c, false)).ToListAsync();
            return new ApiResult<List<ComponentResp>>(components);
        }

        public async Task<ApiResult<ComponentResp>> GetComponentAsync(int componentId)
        {
            var component = await _dbCtx.Components.AsNoTracking().Include(c => c.Categories).Where(c => c.ComponentId == componentId).Select(i => new ComponentResp(i, true)).FirstOrDefaultAsync();
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
            //await _dbCtx.Entry(component).Collection(c => c.Categories).LoadAsync();
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
            var bin = await _dbCtx.Bins.AsNoTracking().Include(b => b.Warehouse).Include(b => b.ComponentBins).Where(b => b.BinId == binId).Select(b => new BinResp(b, true)).FirstOrDefaultAsync();
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

            await _dbCtx.Entry(bin).Reference(b => b.Warehouse).LoadAsync();
            await _dbCtx.Entry(bin).Collection(b => b.ComponentBins).LoadAsync();

            return new ApiResult<BinResp>(new BinResp(bin, true));
        }


        public async Task<ApiResult<List<WarehouseResp>>> GetWarehouseListAsync()
        {
            var warehouses = await _dbCtx.Warehouses.AsNoTracking().Select(w => new WarehouseResp(w, false)).ToListAsync();
            return new ApiResult<List<WarehouseResp>>(warehouses);
        }

        public async Task<ApiResult<WarehouseResp>> GetWarehouseAsync(int warehouseId)
        {
            var warehouse = await _dbCtx.Warehouses.AsNoTracking().Include(w => w.Bins).Where(w => w.WarehouseId == warehouseId).Select(w => new WarehouseResp(w, true)).FirstOrDefaultAsync();
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
            warehouse.Bins.Add(new Bin
            {
                LocationInWarehouse = "Default Bin",
                Status = BinStatus.Empty
            });
            _dbCtx.Warehouses.Add(warehouse);
            await _dbCtx.SaveChangesAsync();

            await _dbCtx.Entry(warehouse).Collection(w => w.Bins).LoadAsync();

            return new ApiResult<WarehouseResp>(new WarehouseResp(warehouse, true));
        }


        public async Task<ApiResult<BinResp>> UpdateBinStatusAsync(int binId, BinStatus status)
        {
            if (binId <= 0)
                return new ApiResult<BinResp>(ApiResultCode.InvalidRequest, "Invalid bin ID.");
            if (!Enum.IsDefined(status))
                return new ApiResult<BinResp>(ApiResultCode.InvalidRequest, "Invalid bin status.");
            Bin? bin = await _dbCtx.Bins.FindAsync(binId);
            if (bin is null)
                return new ApiResult<BinResp>(ApiResultCode.NotFound, $"Bin with ID '{binId}' does not exist.");
            if (bin.ComponentBins.Count > 0 && bin.Status == BinStatus.Empty)
                return new ApiResult<BinResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{binId}' cannot be set to empty because it contains components.");
            bin.Status = status;
            await _dbCtx.SaveChangesAsync();
            //await _dbCtx.Entry(bin).Reference(b => b.Warehouse).LoadAsync();
            //await _dbCtx.Entry(bin).Collection(b => b.ComponentBins).LoadAsync();
            return new ApiResult<BinResp>(new BinResp(bin, true));
        }


        public async Task<ApiResult<List<TransferRequestResp>>> GetTransferRequestsAsync()
        {
            var transferReqs = await _dbCtx.TransferRequests.AsNoTracking().Select(tr => new TransferRequestResp(tr, false)).ToListAsync();
            return new ApiResult<List<TransferRequestResp>>(transferReqs);
        }

        public async Task<ApiResult<TransferRequestResp>> GetTransferRequestAsync(int requestId)
        {
            return new ApiResult<TransferRequestResp>(await _dbCtx.TransferRequests.AsNoTracking()
                .Include(tr => tr.Creator).Include(tr => tr.Approver)
                .Include(tr => tr.BinFrom).Include(tr => tr.BinTo)
                .Include(tr => tr.TransferRequestComponents)
                .Where(tr => tr.RequestId == requestId).Select(tr => new TransferRequestResp(tr, true)).FirstOrDefaultAsync());
        }

        public async Task<ApiResult<TransferRequestResp>> CreateTransferRequestAsync(CreateTransferRequestReq request, TransferType type, int creatorId)
        {
            request.Type = type;
            if (!request.Verify(out string failedReason))
                return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, failedReason);
            Bin? binFrom = null;
            Bin? binTo = null;
            if (request.BinFromId.HasValue)
                binFrom = await _dbCtx.Bins.FindAsync(request.BinFromId.Value);
            if (request.BinToId.HasValue)
                binTo = await _dbCtx.Bins.FindAsync(request.BinToId.Value);
            switch (type)
            {
                case TransferType.Inbound:
                    if (binTo is null)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinToId}' does not exist.");
                    if (binTo.Status == BinStatus.Full)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinToId}' is full.");
                    break;
                case TransferType.Outbound:
                    if (binFrom is null)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' does not exist.");
                    if (binFrom.Status == BinStatus.Full)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' is full.");
                    break;
                case TransferType.InternalTransfer:
                    if (binFrom is null)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' does not exist.");
                    if (binTo is null)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinToId}' does not exist.");
                    if (binFrom.Status == BinStatus.Full)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' is full.");
                    if (binTo.Status == BinStatus.Full)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinToId}' is full.");
                    break;
            }
            List<TransferRequestComponent> tComponents = [];
            foreach (var componentReq in request.Components)
            {
                var component = await _dbCtx.Components.FindAsync(componentReq.ComponentId);
                if (component is null)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Component with ID '{componentReq.ComponentId}' does not exist.");
                // For outbound requests unit price may be omitted by client. Use current component.UnitPrice as fallback.
                var unitPrice = componentReq.UnitPrice;
                if (type == TransferType.Outbound && (unitPrice <= 0))
                    unitPrice = component.UnitPrice;

                tComponents.Add(new TransferRequestComponent
                {
                    ComponentId = componentReq.ComponentId,
                    Quantity = componentReq.Quantity,
                    UnitPrice = unitPrice
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

            await _dbCtx.Entry(transferRequest).Reference(tr => tr.Creator).LoadAsync();
            await _dbCtx.Entry(transferRequest).Reference(tr => tr.BinFrom).LoadAsync();
            await _dbCtx.Entry(transferRequest).Reference(tr => tr.BinTo).LoadAsync();
            await _dbCtx.Entry(transferRequest).Collection(tr => tr.TransferRequestComponents).LoadAsync();

            return new ApiResult<TransferRequestResp>(new TransferRequestResp(transferRequest, true));
        }

        public async Task<ApiResult<TransferRequestResp>> ConfirmTransferRequestAsync(
     ConfirmTransferRequestReq request, int approverId)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, failedReason);

            var transferRequest = await _dbCtx.TransferRequests
                .Include(tr => tr.TransferRequestComponents)
                .FirstOrDefaultAsync(tr => tr.RequestId == request.RequestId);

            if (transferRequest == null)
                return new ApiResult<TransferRequestResp>(ApiResultCode.NotFound);

            if (transferRequest.Status != TransferStatus.ApprovedAndWaitForConfirm)
                return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest);

            foreach (var binReq in request.Bins)
            {
                // Resolve which bin this binReq refers to depending on transfer type
                int resolvedBinId = binReq.BinId > 0 ? binReq.BinId : (int)(transferRequest.BinFromId ?? transferRequest.BinToId ?? 0);
                if (resolvedBinId <= 0)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, "Bin id is missing in confirmation payload and cannot be resolved.");

                var bin = await _dbCtx.Bins.Include(b => b.ComponentBins).FirstOrDefaultAsync(b => b.BinId == resolvedBinId);
                if (bin == null)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{resolvedBinId}' does not exist.");

                foreach (var compReq in binReq.Components)
                {
                    var componentBin = bin.ComponentBins.FirstOrDefault(cb => cb.ComponentId == compReq.ComponentId);

                    if (transferRequest.Type == TransferType.Inbound)
                    {
                        // Add incoming quantity to destination bin
                        var component = await _dbCtx.Components.FirstAsync(c => c.ComponentId == compReq.ComponentId);
                        var existingTotalQuantity = await _dbCtx.ComponentBins.Where(cb => cb.ComponentId == compReq.ComponentId).SumAsync(cb => cb.Quantity);
                        var existingTotalPrice = existingTotalQuantity * component.UnitPrice;

                        if (componentBin == null)
                        {
                            componentBin = new ComponentBin { BinId = bin.BinId, ComponentId = compReq.ComponentId, Quantity = compReq.Quantity };
                            _dbCtx.ComponentBins.Add(componentBin);
                            bin.ComponentBins.Add(componentBin);
                        }
                        else
                        {
                            componentBin.Quantity += compReq.Quantity;
                        }

                        // Recalculate unit price using original transfer request component price (if any)
                        var trComp = transferRequest.TransferRequestComponents.FirstOrDefault(t => t.ComponentId == compReq.ComponentId);
                        if (trComp != null && trComp.UnitPrice > 0)
                        {
                            var newTotalQuantity = existingTotalQuantity + compReq.Quantity;
                            if (newTotalQuantity > 0)
                                component.UnitPrice = (existingTotalPrice + (compReq.Quantity * trComp.UnitPrice)) / newTotalQuantity;
                        }
                    }
                    else if (transferRequest.Type == TransferType.Outbound)
                    {
                        // Determine source bin: prefer bin id provided in confirmation payload, fallback to transfer's BinFromId
                        int? sourceBinId = binReq.BinId > 0 ? binReq.BinId : transferRequest.BinFromId;
                        if (!sourceBinId.HasValue)
                            return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, "Source bin not specified.");

                        var binFrom = await _dbCtx.Bins.Include(b => b.ComponentBins).FirstOrDefaultAsync(b => b.BinId == sourceBinId.Value);
                        if (binFrom == null)
                            return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{sourceBinId.Value}' does not exist.");

                        double remaining = compReq.Quantity;
                        _logger?.LogInformation("Confirm outbound: request={requestId} comp={compId} requested={req} sourceBin={binId}", transferRequest.RequestId, compReq.ComponentId, compReq.Quantity, binFrom.BinId);

                        // try consume from source bin first
                        var cbFrom = binFrom.ComponentBins.FirstOrDefault(cb => cb.ComponentId == compReq.ComponentId);
                        if (cbFrom != null)
                        {
                            if (cbFrom.Quantity >= remaining)
                            {
                                cbFrom.Quantity -= remaining;
                                remaining = 0;
                                if (cbFrom.Quantity == 0)
                                {
                                    _dbCtx.ComponentBins.Remove(cbFrom);
                                    binFrom.ComponentBins.Remove(cbFrom);
                                }
                            }
                            else
                            {
                                remaining -= cbFrom.Quantity;
                                _dbCtx.ComponentBins.Remove(cbFrom);
                                binFrom.ComponentBins.Remove(cbFrom);
                            }
                        }

                        if (remaining > 0)
                        {
                            var otherBinsDebug = await _dbCtx.ComponentBins.Where(cb => cb.ComponentId == compReq.ComponentId && cb.BinId != binFrom.BinId).Select(cb => new { cb.BinId, cb.Quantity }).ToListAsync();
                            _logger?.LogInformation("Confirm outbound: still need {remaining} after source bin; other bins: {bins}", remaining, string.Join(',', otherBinsDebug.Select(x => $"{x.BinId}:{x.Quantity}")));
                        }

                        // if still need, consume from other bins that contain this component
                        if (remaining > 0)
                        {
                            var otherBins = await _dbCtx.ComponentBins.Where(cb => cb.ComponentId == compReq.ComponentId && cb.BinId != binFrom.BinId).OrderByDescending(cb => cb.Quantity).ToListAsync();
                            foreach (var ob in otherBins)
                            {
                                if (remaining <= 0) break;
                                if (ob.Quantity > remaining)
                                {
                                    ob.Quantity -= remaining;
                                    remaining = 0;
                                }
                                else
                                {
                                    remaining -= ob.Quantity;
                                    _dbCtx.ComponentBins.Remove(ob);
                                }
                            }
                        }

                        if (remaining > 0)
                        {
                            return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, "Not enough stock.");
                        }

                        // update status of bins affected (source and any others)
                        var affectedBinIds = new List<int> { binFrom.BinId };
                        // collect other bins that may have been modified
                        var modifiedOtherBinIds = await _dbCtx.ComponentBins.Where(cb => cb.ComponentId == compReq.ComponentId && cb.BinId != binFrom.BinId).Select(cb => cb.BinId).Distinct().ToListAsync();
                        affectedBinIds.AddRange(modifiedOtherBinIds);
                        foreach (var bid in affectedBinIds.Distinct())
                        {
                            var b = await _dbCtx.Bins.Include(x => x.ComponentBins).FirstOrDefaultAsync(x => x.BinId == bid);
                            if (b != null)
                                b.Status = b.ComponentBins.Any(cb => cb.Quantity > 0) ? BinStatus.Available : BinStatus.Empty;
                        }

                        // if the confirmation payload used the same bin object loaded earlier, sync it
                        if (bin.BinId == binFrom.BinId)
                            bin = binFrom;
                    }
                    else if (transferRequest.Type == TransferType.InternalTransfer)
                    {
                        // For internal transfer, bin in payload is treated as source; destination comes from transferRequest.BinToId
                        var availSrc = componentBin?.Quantity ?? 0;
                        _logger?.LogInformation("Confirm internal transfer: srcBin={binId}, componentId={compId}, requested={req}, available={avail}", bin.BinId, compReq.ComponentId, compReq.Quantity, availSrc);
                        if (componentBin == null || componentBin.Quantity < compReq.Quantity)
                            return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Not enough stock for component '{compReq.ComponentId}' in bin '{bin.BinId}'. Available: {availSrc}, Requested: {compReq.Quantity}");

                        componentBin.Quantity -= compReq.Quantity;

                        var binToId = transferRequest.BinToId;
                        if (!binToId.HasValue)
                            return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, "Destination bin not set for internal transfer.");

                        var binTo = await _dbCtx.Bins.Include(b => b.ComponentBins).FirstAsync(b => b.BinId == binToId.Value);
                        var toComponent = binTo.ComponentBins.FirstOrDefault(cb => cb.ComponentId == compReq.ComponentId);
                        if (toComponent == null)
                        {
                            toComponent = new ComponentBin { BinId = binTo.BinId, ComponentId = compReq.ComponentId, Quantity = compReq.Quantity };
                            _dbCtx.ComponentBins.Add(toComponent);
                            binTo.ComponentBins.Add(toComponent);
                        }
                        else
                        {
                            toComponent.Quantity += compReq.Quantity;
                        }

                        if (componentBin.Quantity == 0)
                        {
                            _dbCtx.ComponentBins.Remove(componentBin);
                            bin.ComponentBins.Remove(componentBin);
                        }

                        binTo.Status = binTo.ComponentBins.Any(cb => cb.Quantity > 0) ? BinStatus.Available : BinStatus.Empty;
                    }
                }

                // Update current bin status
                bin.Status = bin.ComponentBins.Any(cb => cb.Quantity > 0) ? BinStatus.Available : BinStatus.Empty;
            }

            transferRequest.Status = TransferStatus.Confirmed;
            transferRequest.ExecutionTime = DateTime.UtcNow;
            transferRequest.ApproverId = approverId;

            await _dbCtx.SaveChangesAsync();

            // Ensure navigation properties are loaded before creating response DTO to avoid null references
            await _dbCtx.Entry(transferRequest).Reference(tr => tr.Creator).LoadAsync();
            await _dbCtx.Entry(transferRequest).Reference(tr => tr.Approver).LoadAsync();
            await _dbCtx.Entry(transferRequest).Reference(tr => tr.BinFrom).LoadAsync();
            await _dbCtx.Entry(transferRequest).Reference(tr => tr.BinTo).LoadAsync();
            await _dbCtx.Entry(transferRequest).Collection(tr => tr.TransferRequestComponents).LoadAsync();

            return new ApiResult<TransferRequestResp>(new TransferRequestResp(transferRequest, true));
        }
    }
    }
