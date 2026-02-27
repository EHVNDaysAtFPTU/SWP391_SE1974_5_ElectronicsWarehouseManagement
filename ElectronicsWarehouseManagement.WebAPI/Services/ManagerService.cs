using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IManagerService
    {
        Task<ApiResult<ComponentResp>> GetComponentAsync(int componentId, bool fullInfo);
        Task<ApiResult<PagedResult<ComponentResp>>> GetComponentListAsync(PagingRequest request);
        Task<ApiResult<TransferRequestResp>> GetTransferAsync(int transferId, bool fullInfo);
        Task<ApiResult<PagedResult<TransferRequestResp>>> GetTransferReqListAsync(PagingRequest request);
        Task<ApiResult> PostTransferDecisionAsync(int transferId, TransferDecisionType decision, int? approverId);
        ////Task<ApiResult<PagedResult<ItemDefResp>>> GetFilteredItemListAsync(PagingRequest request, FilteredCodeReq fReq);
        Task<ApiResult<BinResp>> GetBin(int binId, bool fullInfo);
        Task<ApiResult<PagedResult<BinResp>>> GetBinList(PagingRequest request, int warehouseId, bool fullInfo);
        Task<ApiResult<PagedResult<WarehouseResp>>> GetWareHouseListAsync(PagingRequest request);
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
               .Include(i=> i.Categories)
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
             .Include(i => i.Categories);

            int totalCount = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new ComponentResp(i, false))
                .ToListAsync();

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
                    .Include(t => t.BinFrom)
                    .Include(t => t.BinTo)
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
            var query = _dbCtx.TransferRequests.AsNoTracking();
            int totalCount = await query.CountAsync();
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
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
                TransferDecisionType.ApprovedAndWaitForConfirm  => TransferStatus.ApprovedAndWaitForConfirm,
                TransferDecisionType.Rejected => TransferStatus.Rejected,
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

        public async Task<ApiResult<BinResp>> GetBin(int binId, bool fullInfo)
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

        public async Task<ApiResult<PagedResult<BinResp>>> GetBinList(PagingRequest request, int warehouseId, bool fullInfo)
        {
            var query = _dbCtx.Bins.AsNoTracking()
                .Where(b => b.WarehouseId == warehouseId)
                .Include(b => b.Warehouse)
                .Include(b => b.ComponentBins).ThenInclude(c => c.Component);

            int totalCount = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var binRespList = data.Select(b => new BinResp(b, fullInfo)).ToList();

            var pagedResult = new PagedResult<BinResp>
            {
                data = binRespList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<BinResp>>(pagedResult);
        }

        public async Task<ApiResult<PagedResult<WarehouseResp>>> GetWareHouseListAsync(PagingRequest request)
        {
            var query = _dbCtx.Warehouses.AsNoTracking();
            var totalCount = await query.CountAsync();
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

        //    public async Task<ApiResult<PagedResult<ItemResp>>> GetFilteredItemListAsync(PagingRequest request, FilteredCodeReq fReq)
        //    {
        //        if (fReq.filtered)
        //        {
        //            return new ApiResult<PagedResult<ItemResp>>(ApiResultCode.NotFound);
        //        }
        //        switch (fReq.filterCode)
        //        {
        //            case 1:
        //                {
        //                    var item = await _dbCtx.Items
        //           .AsNoTracking()
        //           .Include(i => i.ItemDef)
        //           .Include(i => i.Bins)
        //               .ThenInclude(b => b.Warehouse)
        //               .Where(i => i.item)
        //           .Select(i => new ItemResp(i))
        //                }
        //        }


        //        switch (fReq.filterCode)
        //        {

        //        }


        //    }
        //    public int? CountAsync()
        //    {

        //    }
        //}

    }

    }