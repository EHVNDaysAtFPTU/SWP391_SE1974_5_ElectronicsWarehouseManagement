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

        Task<ApiResult<List<WarehouseResp>>> GetWarehousesAsync();
        Task<ApiResult<WarehouseResp>> GetWarehouseAsync(int warehouseId);
        Task<ApiResult<WarehouseResp>> CreateWarehouseAsync(CreateWarehouseReq request);

        Task<ApiResult<BinResp>> UpdateBinStatusAsync(int binId, BinStatus status);

        Task<ApiResult<List<TransferRequestResp>>> GetTransferRequestsAsync(int creatorId);
        Task<ApiResult<TransferRequestResp>> GetTransferRequestAsync(int requestId, int creatorId);
        Task<ApiResult<TransferRequestResp>> CreateTransferRequestAsync(CreateTransferRequestReq request, TransferType type, int creatorId);
        Task<ApiResult<TransferRequestResp>> ConfirmTransferRequestAsync(ConfirmTransferRequestReq request, int approverId);
   
        Task<ApiResult<int>> GetWarehouseCountAsync();
        Task<ApiResult<int>> GetBinCountAsync();
        Task<ApiResult<int>> GetComponentInBinCountAsync();
        Task<ApiResult<int>> GetTransferRequestCountAsync(int creatorId);
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


        public async Task<ApiResult<List<ComponentResp>>> GetComponentsAsync()
        {
            var components = await _dbCtx.Components.AsNoTracking().Select(c => new ComponentResp(c, false)).ToListAsync();
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


        public async Task<ApiResult<List<WarehouseResp>>> GetWarehousesAsync()
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


        public async Task<ApiResult<List<TransferRequestResp>>> GetTransferRequestsAsync(int creatorId)
        {
            var transferReqs = await _dbCtx.TransferRequests.AsNoTracking()
                .Where(tr => tr.CreatorId == creatorId)
                .Select(tr => new TransferRequestResp(tr, false)).ToListAsync();
            return new ApiResult<List<TransferRequestResp>>(transferReqs);
        }

        public async Task<ApiResult<TransferRequestResp>> GetTransferRequestAsync(int requestId, int creatorId)
        {
            return new ApiResult<TransferRequestResp>(await _dbCtx.TransferRequests.AsNoTracking()
                .Include(tr => tr.Creator).Include(tr => tr.Approver)
                .Include(tr => tr.BinFrom).Include(tr => tr.BinTo)
                .Include(tr => tr.TransferRequestComponents)
                .Where(tr => tr.RequestId == requestId && tr.CreatorId == creatorId)
                .Select(tr => new TransferRequestResp(tr, true)).FirstOrDefaultAsync());
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
                    if (binTo.Status == BinStatus.Locked)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinToId}' is full.");
                    break;
                case TransferType.Outbound:
                    if (binFrom is null)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' does not exist.");
                    if (binFrom.Status == BinStatus.Locked)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' is full.");
                    break;
                case TransferType.InternalTransfer:
                    if (binFrom is null)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' does not exist.");
                    if (binTo is null)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinToId}' does not exist.");
                    if (binFrom.Status == BinStatus.Locked)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinFromId}' is full.");
                    if (binTo.Status == BinStatus.Locked)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{request.BinToId}' is full.");
                    break;
            }
            List<TransferRequestComponent> tComponents = [];
            foreach (var componentReq in request.Components)
            {
                var component = await _dbCtx.Components.FindAsync(componentReq.ComponentId);
                if (component is null)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Component with ID '{componentReq.ComponentId}' does not exist.");
                if (type == TransferType.Outbound && componentReq.Quantity > component.TotalQuantity)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Requested quantity for component with ID '{componentReq.ComponentId}' exceeds available quantity.");
                TransferRequestComponent item = new TransferRequestComponent()
                {
                    ComponentId = componentReq.ComponentId,
                    Quantity = componentReq.Quantity,
                };
                if (type == TransferType.Inbound)
                {
                    if (componentReq.UnitPrice is null)
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Unit price is required for component with ID '{componentReq.ComponentId}' in inbound transfer request.");
                    item.UnitPrice = componentReq.UnitPrice.Value;
                }
                tComponents.Add(item);
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

        public async Task<ApiResult<TransferRequestResp>> ConfirmTransferRequestAsync(ConfirmTransferRequestReq request, int approverId)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, failedReason);
            TransferRequest? transferRequest = await _dbCtx.TransferRequests.Include(tr => tr.TransferRequestComponents).FirstOrDefaultAsync(tr => tr.RequestId == request.RequestId);
            if (transferRequest is null)
                return new ApiResult<TransferRequestResp>(ApiResultCode.NotFound, $"Transfer request with ID '{request.RequestId}' does not exist.");
            if (transferRequest.Status != TransferStatus.ApprovedAndWaitForConfirm)
                return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Transfer request with ID '{request.RequestId}' cannot be confirmed.");
            // verify components in confirmation request to match those in original transfer request
            List<TransferRequestComponent> originalComponentsInTransferRequest = transferRequest.TransferRequestComponents.OrderBy(c => c.ComponentId).ToList();

            List<ConfirmTransferRequestComponentReq> confirmComponentsTakeFromBin = [];
            foreach (ConfirmTransferBinReq ctBinTakeFrom in request.BinsFrom ?? [])
            {
                Bin? bin = await _dbCtx.Bins.FindAsync(ctBinTakeFrom.BinId);
                if (bin is null)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{ctBinTakeFrom.BinId}' does not exist.");
                if (bin.Status == BinStatus.Locked)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{ctBinTakeFrom.BinId}' is full.");
                foreach (ConfirmTransferRequestComponentReq ctComponentTakeFromBin in ctBinTakeFrom.Components.OrderBy(c => c.ComponentId))
                {
                    if (!originalComponentsInTransferRequest.Any(c => c.ComponentId == ctComponentTakeFromBin.ComponentId))
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Component with ID '{ctComponentTakeFromBin.ComponentId}' is not in the original transfer request.");
                    var confirmComponentTakeFromBin = confirmComponentsTakeFromBin.FirstOrDefault(c => c.ComponentId == ctComponentTakeFromBin.ComponentId);
                    if (confirmComponentTakeFromBin is null)
                    {
                        confirmComponentsTakeFromBin.Add(new ConfirmTransferRequestComponentReq
                        {
                            ComponentId = ctComponentTakeFromBin.ComponentId,
                            Quantity = ctComponentTakeFromBin.Quantity
                        });
                    }
                    else
                        confirmComponentTakeFromBin.Quantity += ctComponentTakeFromBin.Quantity;
                }
            }

            List<ConfirmTransferRequestComponentReq> confirmComponentsAddToBin = [];
            foreach (ConfirmTransferBinReq ctBinAddTo in request.BinsTo ?? [])
            {
                Bin? bin = await _dbCtx.Bins.FindAsync(ctBinAddTo.BinId);
                if (bin is null)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{ctBinAddTo.BinId}' does not exist.");
                if (bin.Status == BinStatus.Locked)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Bin with ID '{ctBinAddTo.BinId}' is full.");
                foreach (ConfirmTransferRequestComponentReq ctComponentReq in ctBinAddTo.Components.OrderBy(c => c.ComponentId))
                {
                    if (!originalComponentsInTransferRequest.Any(c => c.ComponentId == ctComponentReq.ComponentId))
                        return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Component with ID '{ctComponentReq.ComponentId}' is not in the original transfer request.");
                    var confirmComponentAddToBin = confirmComponentsAddToBin.FirstOrDefault(c => c.ComponentId == ctComponentReq.ComponentId);
                    if (confirmComponentAddToBin is null)
                    {
                        confirmComponentsAddToBin.Add(new ConfirmTransferRequestComponentReq
                        {
                            ComponentId = ctComponentReq.ComponentId,
                            Quantity = ctComponentReq.Quantity
                        });
                    }
                    else
                        confirmComponentAddToBin.Quantity += ctComponentReq.Quantity;
                }
            }

            foreach (ConfirmTransferRequestComponentReq confirmComponent in confirmComponentsAddToBin.Concat(confirmComponentsTakeFromBin))
            {
                var originalComponent = originalComponentsInTransferRequest.First(c => c.ComponentId == confirmComponent.ComponentId);
                if (confirmComponent.Quantity != originalComponent.Quantity)
                    return new ApiResult<TransferRequestResp>(ApiResultCode.InvalidRequest, $"Confirmed quantity for component with ID '{confirmComponent.ComponentId}' does not match the original transfer request.");
            }

            transferRequest.Status = TransferStatus.Confirmed;
            transferRequest.ExecutionTime = DateTime.UtcNow;

            // add/remove/update components in bins
            foreach (ConfirmTransferBinReq binFrom in request.BinsFrom ?? [])
            {
                Bin bin = await _dbCtx.Bins.Include(b => b.ComponentBins).FirstAsync(b => b.BinId == binFrom.BinId);
                foreach (ConfirmTransferRequestComponentReq componentTakeFromBin in binFrom.Components)
                {
                    ComponentBin? componentBin = bin.ComponentBins.FirstOrDefault(cb => cb.ComponentId == componentTakeFromBin.ComponentId);
                    if (componentBin is null)
                        throw new InvalidOperationException($"Component with ID '{componentTakeFromBin.ComponentId}' is not in bin with ID '{bin.BinId}'.");
                    if (componentBin.Quantity < componentTakeFromBin.Quantity)
                        throw new InvalidOperationException($"Not enough quantity of component with ID '{componentTakeFromBin.ComponentId}' in bin with ID '{bin.BinId}' to take from.");
                    componentBin.Quantity -= componentTakeFromBin.Quantity;
                    if (componentBin.Quantity == 0)
                        _dbCtx.ComponentBins.Remove(componentBin);
                }
                if (bin.Status != BinStatus.Empty && bin.ComponentBins.Count == 0)
                    bin.Status = BinStatus.Empty;
            }
            foreach (ConfirmTransferBinReq binTo in request.BinsTo ?? [])
            {
                Bin bin = await _dbCtx.Bins.Include(b => b.ComponentBins).FirstAsync(b => b.BinId == binTo.BinId);
                foreach (ConfirmTransferRequestComponentReq componentAddToBin in binTo.Components)
                {
                    ComponentBin? componentBin = bin.ComponentBins.FirstOrDefault(cb => cb.ComponentId == componentAddToBin.ComponentId);
                    if (componentBin is null)
                    {
                        componentBin = new ComponentBin
                        {
                            BinId = bin.BinId,
                            ComponentId = componentAddToBin.ComponentId,
                            Quantity = componentAddToBin.Quantity
                        };
                        _dbCtx.ComponentBins.Add(componentBin);
                    }
                    else
                        componentBin.Quantity += componentAddToBin.Quantity;
                }
                if (bin.Status == BinStatus.Empty)
                    bin.Status = BinStatus.Available;
            }

            // calculate new average price for the component after transfer
            if (transferRequest.Type == TransferType.Inbound)
            {
                foreach (TransferRequestComponent tComponent in originalComponentsInTransferRequest)
                {
                    Component component = await _dbCtx.Components.FirstAsync(c => c.ComponentId == tComponent.ComponentId);
                    double newPrice = (component.TotalPrice + tComponent.Quantity * tComponent.UnitPrice) / (component.TotalQuantity + tComponent.Quantity);
                    component.UnitPrice = newPrice;
                }
            }

            await _dbCtx.SaveChangesAsync();

            await _dbCtx.Entry(transferRequest).Reference(tr => tr.Creator).LoadAsync();
            await _dbCtx.Entry(transferRequest).Reference(tr => tr.Approver).LoadAsync();
            await _dbCtx.Entry(transferRequest).Reference(tr => tr.BinFrom).LoadAsync();
            await _dbCtx.Entry(transferRequest).Reference(tr => tr.BinTo).LoadAsync();
            await _dbCtx.Entry(transferRequest).Collection(tr => tr.TransferRequestComponents).LoadAsync();

            return new ApiResult<TransferRequestResp>(new TransferRequestResp(transferRequest, true));
        }


        public async Task<ApiResult<int>> GetWarehouseCountAsync()
        {
            int result = await _dbCtx.Warehouses.CountAsync();
            return new ApiResult<int>(result);
        }

        public async Task<ApiResult<int>> GetBinCountAsync()
        {
            int result = await _dbCtx.Bins.CountAsync();
            return new ApiResult<int>(result);
        }

        public async Task<ApiResult<int>> GetComponentInBinCountAsync()
        {
            int result = await _dbCtx.ComponentBins.CountAsync();
            return new ApiResult<int>(result);
        }

        public async Task<ApiResult<int>> GetTransferRequestCountAsync(int creatorId)
        {
            int result = await _dbCtx.TransferRequests.Where(tr => tr.CreatorId == creatorId).CountAsync();
            return new ApiResult<int>(result);
        }
    }
}