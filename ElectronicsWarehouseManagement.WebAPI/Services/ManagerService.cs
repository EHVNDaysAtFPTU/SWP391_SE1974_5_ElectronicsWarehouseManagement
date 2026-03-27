using ElectronicsWarehouseManagement.Repositories.DBContext;
using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsWarehouseManagement.WebAPI.Services

{

    public interface IManagerService
    {
        // Get element
        Task<ApiResult<ComponentResp>> GetComponentAsync(int componentId, bool fullInfo);
        Task<ApiResult<TransferRequestResp>> GetTransferAsync(int transferId, bool fullInfo);
        Task<ApiResult<BinResp>> GetBinAsync(int binId, bool fullInfo);
        Task<ApiResult<WarehouseResp>> GetWareHouseAsync(int warehouseId, bool fullInfo);
        // Get list element
        Task<ApiResult<PagedResult<ComponentResp>>> GetComponentListAsync(PagingRequest request);
        Task<ApiResult<PagedResult<TransferRequestResp>>> GetTransferReqListAsync(PagingRequest request);
        Task<ApiResult<PagedResult<BinResp>>> GetBinListAsync(PagingRequest request);
        Task<ApiResult<PagedResult<BinResp>>> GetBinListByWareHouseId(PagingRequest request, int warehouseId, bool fullInfo);
        Task<ApiResult<PagedResult<WarehouseResp>>> GetWareHouseListAsync(PagingRequest request);
        Task<ApiResult<PagedResult<CustomerResp>>> GetCustomerListAsync(PagingRequest request);
        // Crud 
        Task<ApiResult> PostTransferDecisionAsync(int transferId, TransferDecisionType decision, int? approverId);
        Task<ApiResult> CreateCustomerAsync(CustomerReq customerReq);
        Task<ApiResult<CustomerResp>> UpdateCustomerAsync(int customerId, CustomerReq customerReq);
        Task<ApiResult<BinResp>> CreateBinAsync(CreateBinReq request);
        Task<ApiResult<WarehouseResp>> CreateWarehouseAsync(CreateWarehouseReq request);
        Task<ApiResult<UpdateBinResp>> UpdateBinAsync(UpdateBinReq request);
        Task<ApiResult<UpdateWarehouseResp>> UpdateWarehouseAsync(UpdateWarehouseReq request);

        // Dashboard
        Task<ApiResult<DashboardSummaryResp>> GetSummaryAsync();
        Task<ApiResult<DashboardChartResp>> GetChartDataAsync(int days);

        Task<ApiResult<string>> UploadImageAsync(IFormFile image);

    }

    public class ManagerService : IManagerService
    {
        private readonly EWMDbCtx _dbCtx;

        public ManagerService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<ApiResult<ComponentResp>> GetComponentAsync(int componentId, bool fullInfo)
        {
            var component = await _dbCtx.Components
               .AsNoTracking()
               .Include(i => i.Categories)
               .Include(i => i.ComponentBins)
               .Where(i => i.ComponentId == componentId)
               .Select(i => new ComponentResp(i, fullInfo))
               .FirstOrDefaultAsync();

            if (component == null)
                return new ApiResult<ComponentResp>(ApiResultCode.NotFound, "Component not found");

            return new ApiResult<ComponentResp>(component);
        }


        public async Task<ApiResult<PagedResult<ComponentResp>>> GetComponentListAsync(PagingRequest request)
        {
            var query = _dbCtx.Components
                .Include(i => i.Categories)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(c => c.MetadataJson.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "unitPrice":
                        query = query.ApplySort(request.SortDirection, c => c.UnitPrice);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.ComponentId);
                        break;
                }
            }

            int totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<ComponentResp>>(new PagedResult<ComponentResp>
                {
                    data = new List<ComponentResp>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                });
            }

            var data = await query.Include(i => i.ComponentBins).ApplyPaging(request).Select(i => new ComponentResp(i, true)).ToListAsync();

            var pagedResult = new PagedResult<ComponentResp>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<ComponentResp>>(pagedResult);
        }



        public async Task<ApiResult<TransferRequestResp>> GetTransferAsync(int requestId, bool fullInfo)
        {
            var query = _dbCtx.TransferRequests.AsNoTracking();

            if (fullInfo)
            {
                query = query
                    .Include(t => t.Approver)
                    .Include(t => t.Creator)
                    .Include(t => t.WarehouseFrom)
                    .Include(t => t.WarehouseTo)
                    .Include(t => t.Customer)
                    .Include(t => t.FinishedTransferRequestComponents)
                    .ThenInclude(t => t.Bin)
                    .Include(t => t.TransferRequestComponents)
                        .ThenInclude(c => c.Component);
            }

            var transfer = await query
                .Where(t => t.RequestId == requestId)
                .Select(t => new TransferRequestResp(t, fullInfo))
                .FirstOrDefaultAsync();

            if (transfer == null)
                return new ApiResult<TransferRequestResp>(ApiResultCode.NotFound, "Transfer request not found");

            return new ApiResult<TransferRequestResp>(transfer);
        }

        public async Task<ApiResult<PagedResult<TransferRequestResp>>> GetTransferReqListAsync(PagingRequest request)
        {
            IQueryable<TransferRequest> query = _dbCtx.TransferRequests.AsNoTracking().AsQueryable()
                .Include(i => i.Approver)
                .Include(i => i.Creator)
                .Include(i => i.WarehouseFrom)
                .Include(i => i.WarehouseTo);

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(t => t.Description.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "creationTime":
                        query = query.ApplySort(request.SortDirection, c => c.CreationTime);
                        break;
                    case "type":
                        query = query.ApplySort(request.SortDirection, c => c.TypeInt);
                        break;
                    case "status":
                        query = query.ApplySort(request.SortDirection, c => c.StatusInt);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.RequestId);
                        break;
                }
            }
            int totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<TransferRequestResp>>(new PagedResult<TransferRequestResp>
                {
                    data = new List<TransferRequestResp>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                });
            }
            var data = await query
                .ApplyPaging(request)
                .Select(t => new TransferRequestResp(t, false))
                .ToListAsync();

            var pagedResult = new PagedResult<TransferRequestResp>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<TransferRequestResp>>(pagedResult);
        }


        private TransferStatus MapDecisionToStatus(TransferDecisionType decision)
        {
            return decision switch
            {
                TransferDecisionType.ApprovedAndWaitForConfirm => TransferStatus.ApprovedAndWaitForConfirm,
                TransferDecisionType.Rejected => TransferStatus.Rejected,
                _ => throw new ArgumentException("Invalid transfer decision type", nameof(decision)),
            };
        }
        public async Task<ApiResult> PostTransferDecisionAsync(int transferId, TransferDecisionType decision, int? approverId)
        {

            var transferReq = await _dbCtx.TransferRequests.Include(i => i.Approver).FirstOrDefaultAsync(i => i.RequestId == transferId);

            if (transferReq == null)
            {
                return new ApiResult(ApiResultCode.NotFound);
            }
            var approver = await _dbCtx.Users.FirstOrDefaultAsync(u => u.UserId == approverId);
            if (approver == null)
            {
                return new ApiResult(ApiResultCode.NotFound, "Approver not found");
            }
            string approverUsername = approver.Username;
            switch (decision)
            {
                case TransferDecisionType.ApprovedAndWaitForConfirm:
                    transferReq.StatusInt = (int)MapDecisionToStatus(decision);
                    transferReq.ApproverId = approverId;
                    break;
                case TransferDecisionType.Rejected:
                    transferReq.StatusInt = (int)MapDecisionToStatus(decision);
                    transferReq.ApproverId = approverId;
                    break;
                default:
                    return new ApiResult(ApiResultCode.InvalidRequest);
            }
            await _dbCtx.SaveChangesAsync();

            return new ApiResult();
        }

        public async Task<ApiResult<BinResp>> GetBinAsync(int binId, bool fullInfo)
        {
            var bin = await _dbCtx.Bins.AsNoTracking()
                .Include(b => b.Warehouse)
                .Include(b => b.ComponentBins)
                    .ThenInclude(cb => cb.Component)
                .FirstOrDefaultAsync(b => b.BinId == binId);

            if (bin == null)
                return new ApiResult<BinResp>(ApiResultCode.NotFound, "Bin not found");

            return new ApiResult<BinResp>(new BinResp(bin, fullInfo));
        }

        public async Task<ApiResult<PagedResult<BinResp>>> GetBinListByWareHouseId(PagingRequest request, int warehouseId, bool fullInfo)
        {
            var query = _dbCtx.Bins
                .Where(b => b.WarehouseId == warehouseId)
                .Include(b => b.Warehouse)
                .Include(b => b.ComponentBins).ThenInclude(c => c.Component).AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(c => c.LocationInWarehouse.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "status":
                        query = query.ApplySort(request.SortDirection, c => c.StatusInt);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.Warehouse.WarehouseName);
                        break;
                }
            }
            int totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<BinResp>>(ApiResultCode.NotFound);
            }

            var data = await query
                .ApplyPaging(request)
                .Select(b => new BinResp(b, fullInfo))
                .ToListAsync();

            var pagedResult = new PagedResult<BinResp>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<BinResp>>(pagedResult);
        }

        public async Task<ApiResult<PagedResult<WarehouseResp>>> GetWareHouseListAsync(PagingRequest request)
        {
            var query = _dbCtx.Warehouses.AsNoTracking();
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(c => c.WarehouseName.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "warehouseName":
                        query = query.ApplySort(request.SortDirection, c => c.WarehouseName);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.WarehouseId);
                        break;
                }
            }

            var totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<WarehouseResp>>(new PagedResult<WarehouseResp>
                {
                    data = new List<WarehouseResp>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                });
            }

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize).ToListAsync();

            var warehouseList = data.Select(i => new WarehouseResp(i, false)).ToList();
            var pagedResult = new PagedResult<WarehouseResp>
            {
                data = warehouseList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
            return new ApiResult<PagedResult<WarehouseResp>>(pagedResult);

        }

        public async Task<ApiResult<WarehouseResp>> GetWareHouseAsync(int warehouseId, bool fullInfo)
        {
            var warehouse = await _dbCtx.Warehouses.AsNoTracking()
                .Where(i => i.WarehouseId == warehouseId)
                .Select(i => new WarehouseResp(i, fullInfo))
                .FirstOrDefaultAsync();
            if (warehouse == null)
            {
                return new ApiResult<WarehouseResp>(ApiResultCode.NotFound);
            }
            return new ApiResult<WarehouseResp>(warehouse);
        }

        public async Task<ApiResult<PagedResult<BinResp>>> GetBinListAsync(PagingRequest request)
        {
            var query = _dbCtx.Bins
                .Include(b => b.Warehouse)
                .Include(b => b.ComponentBins).ThenInclude(c => c.Component).AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(c => c.LocationInWarehouse.Contains(request.Search));
            }
            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "status":
                        query = query.ApplySort(request.SortDirection, c => c.StatusInt);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.LocationInWarehouse);
                        break;
                }
            }
            int totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<BinResp>>(ApiResultCode.NotFound);
            }

            var data = await query
                .ApplyPaging(request)
                .Select(b => new BinResp(b, true))
                .ToListAsync();

            var pagedResult = new PagedResult<BinResp>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<BinResp>>(pagedResult);
        }

        public async Task<ApiResult<DashboardSummaryResp>> GetSummaryAsync()
        {
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(1);

            var resp = new DashboardSummaryResp
            {
                TotalComponents = await _dbCtx.Components.CountAsync(),

                TotalWarehouses = await _dbCtx.Warehouses.CountAsync(),

                CurrentStock = await _dbCtx.ComponentBins.SumAsync(cb => cb.Quantity),

                LowStockItems = await _dbCtx.Components
                    .Where(c => c.ComponentBins.Sum(cb => cb.Quantity) < 10 && c.ComponentBins.Sum(cb => cb.Quantity) > 0)
                    .CountAsync(),

                OutOfStockItems = await _dbCtx.Components
                    .Where(c => !c.ComponentBins.Any() || c.ComponentBins.Sum(cb => cb.Quantity) == 0)
                    .CountAsync(),

                InboundToday = await _dbCtx.FinishedTransferRequestComponents
                .Where(ftr =>
                    ftr.Request.TypeInt == 1 &&
                    ftr.Request.ExecutionTime >= start &&
                    ftr.Request.ExecutionTime < end)
                .SumAsync(ftr => ftr.Quantity),

                OutboundToday = await _dbCtx.FinishedTransferRequestComponents
                .Where(ftr =>
                    ftr.Request.TypeInt == 2 &&
                    ftr.Request.ExecutionTime >= start &&
                    ftr.Request.ExecutionTime < end)
                .SumAsync(ftr => ftr.Quantity),
            };

            return new ApiResult<DashboardSummaryResp>(resp);
        }

        public async Task<ApiResult<DashboardChartResp>> GetChartDataAsync(int days)
        {
            var start = DateTime.UtcNow.Date.AddDays(-days);

            var data = await _dbCtx.FinishedTransferRequestComponents
                .Include(ftr => ftr.Request)
                .Where(ftr => ftr.Request.ExecutionTime >= start && ftr.Request.TypeInt == 1 || ftr.Request.TypeInt == 2)
                .Select(ftr => new
                {
                    Date = ftr.Request.ExecutionTime.Value.Date,
                    Type = ftr.TypeInt,
                    Quantity = ftr.Quantity
                })
                .GroupBy(x => x.Date)
                .Select(g => new ImportExportChart
                {
                    Date = g.Key,
                    Import = g.Where(x => x.Type == (int)FinishedTransferRequestComponentType.In)
                              .Sum(x => x.Quantity),

                    Export = g.Where(x => x.Type == (int)FinishedTransferRequestComponentType.Out)
                              .Sum(x => x.Quantity)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return new ApiResult<DashboardChartResp>(
                new DashboardChartResp
                {
                    transferChart = data
                });
        }

        public async Task<ApiResult> CreateCustomerAsync(CustomerReq customerReq)
        {
            if (!customerReq.Verify(out string failedreason))
            {
                return new ApiResult(ApiResultCode.InvalidRequest, failedreason);
            }

            if (await _dbCtx.Customers.AnyAsync(x => x.Email == customerReq.Email))
            {
                return new ApiResult(ApiResultCode.InvalidRequest, "Email already exists!");
            }

            if (await _dbCtx.Customers.AnyAsync(x => x.Phone == customerReq.Phone))
            {
                return new ApiResult(ApiResultCode.InvalidRequest, "Phone already exists!");
            }

            var customer = new Customer
            {
                CustomerName = customerReq.CustomerName,
                Phone = customerReq.Phone,
                Email = customerReq.Email,
                Address = customerReq.Address,
                CreatedAt = DateTime.UtcNow,
            };

            _dbCtx.Customers.Add(customer);
            await _dbCtx.SaveChangesAsync();

            return new ApiResult();
        }

        public async Task<ApiResult<CustomerResp>> UpdateCustomerAsync(int customerId, CustomerReq customerReq)
        {
            if (!customerReq.Verify(out string failedreason))
            {
                return new ApiResult<CustomerResp>(ApiResultCode.InvalidRequest, failedreason);
            }

            var customer = await _dbCtx.Customers.FirstOrDefaultAsync(x => x.CustomerId == customerId);
            if (customer == null)
            {
                return new ApiResult<CustomerResp>(ApiResultCode.NotFound, "Customer not found!");
            }

            if (await _dbCtx.Customers.AnyAsync(x => x.Email == customerReq.Email && x.CustomerId != customerId))
            {
                return new ApiResult<CustomerResp>(ApiResultCode.InvalidRequest, "Email already exists!");
            }

            if (await _dbCtx.Customers.AnyAsync(x => x.Phone == customerReq.Phone && x.CustomerId != customerId))
            {
                return new ApiResult<CustomerResp>(ApiResultCode.InvalidRequest, "Phone already exists!");
            }

            customer.CustomerName = customerReq.CustomerName;
            customer.Phone = customerReq.Phone;
            customer.Email = customerReq.Email;
            customer.Address = customerReq.Address;

            await _dbCtx.SaveChangesAsync();

            return new ApiResult<CustomerResp>(new CustomerResp(customer, true));
        }

        public async Task<ApiResult<PagedResult<CustomerResp>>> GetCustomerListAsync(PagingRequest request)
        {
            var query = _dbCtx.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(c => c.CustomerName.Contains(request.Search) || c.Email.Contains(request.Search) || c.Phone.Contains(request.Search));
            }

            if (request.SortBy != null && request.SortDirection != null)
            {
                switch (request.SortBy)
                {
                    case "customerName":
                        query = query.ApplySort(request.SortDirection, c => c.CustomerName);
                        break;
                    case "createdAt":
                        query = query.ApplySort(request.SortDirection, c => c.CreatedAt);
                        break;
                    default:
                        query = query.ApplySort(request.SortDirection, c => c.CustomerId);
                        break;
                }
            }

            int totalCount = await query.CountAsync();
            if (totalCount == 0)
            {
                return new ApiResult<PagedResult<CustomerResp>>(new PagedResult<CustomerResp>
                {
                    data = new List<CustomerResp>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                });
            }

            var data = await query.ApplyPaging(request).Select(i => new CustomerResp(i, false)).ToListAsync();

            var pagedResult = new PagedResult<CustomerResp>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<CustomerResp>>(pagedResult);
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

        public async Task<ApiResult<UpdateBinResp>> UpdateBinAsync(UpdateBinReq request)
        {

            var bin = await _dbCtx.Bins.FirstOrDefaultAsync(x => x.BinId == request.BinId);
            if (bin == null)
            {
                return new ApiResult<UpdateBinResp>(ApiResultCode.NotFound, "Bin not found!");
            }

            bin.LocationInWarehouse = request.LocationInWarehouse;
            bin.StatusInt = request.StatusInt;

            await _dbCtx.SaveChangesAsync();

            return new ApiResult<UpdateBinResp>(new UpdateBinResp(bin));
        }

        public async Task<ApiResult<UpdateWarehouseResp>> UpdateWarehouseAsync(UpdateWarehouseReq request)
        {
            var warehouse = await _dbCtx.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == request.WarehouseId);
            if (warehouse == null)
            {
                return new ApiResult<UpdateWarehouseResp>(ApiResultCode.NotFound, "Warehouse not found!");
            }

            warehouse.WarehouseName = request.WarehouseName;
            warehouse.Description = request.Description;
            warehouse.PhysicalLocation = request.PhysicalLocation;

            await _dbCtx.SaveChangesAsync();

            return new ApiResult<UpdateWarehouseResp>(new UpdateWarehouseResp(warehouse));
        }
    }
}