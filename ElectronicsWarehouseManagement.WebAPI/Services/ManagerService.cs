using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IManagerService
    {
        Task<ApiResult<ItemResp>> GetItemAsync(int itemId, bool fullInfo);
        Task<ApiResult<PagedResult<ItemResp>>> GetItemListAsync(PagingRequest request);
        Task<ApiResult<TransferReqDTO>> GetTransferAsync(int transferId);
        Task<ApiResult<PagedResult<TransferReqDTO>>> GetTransferReqListAsync(PagingRequest request);
        Task<ApiResult> PostTransferDecisionAsync(int transferId,TransferDecisionType decision);
    }

    public class ManagerService : IManagerService
    {
        private readonly EWMDbCtx _dbCtx;

        public ManagerService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<ApiResult<ItemResp>> GetItemAsync(int itemId, bool fullInfo)
        {
            var item = await _dbCtx.Items
                .AsNoTracking()
                .Include(i => i.ItemDef)
                .Include(i => i.Bins)
                    .ThenInclude(b => b.Warehouse)
                .Include(i => i.Transfer)
                .Include(i => i.Inbound)
                .Include(i => i.Outbound)
                .Where(i => i.ItemId == itemId)
                .Select(i => new ItemResp(i, fullInfo))
                .FirstOrDefaultAsync();

            if (item == null)
                return new ApiResult<ItemResp>(ApiResultCode.NotFound, "Item not found");

            return new ApiResult<ItemResp>(item);
        }

        public async Task<ApiResult<PagedResult<ItemResp>>> GetItemListAsync(PagingRequest request)
        {
            var itemList = _dbCtx.Items
                .AsNoTracking().Include(i => i.ItemDef)
                .Include(i => i.Bins)
                .ThenInclude(b => b.Warehouse)
                .Include(i => i.Transfer)
                .Include(i => i.Inbound)
                .Include(i => i.Outbound);

            int totalCount = await itemList.CountAsync();

            var data = await itemList
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new ItemResp(i,false))
                .ToListAsync();

            var pagedResult = new PagedResult<ItemResp>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<ItemResp>>(pagedResult);
        }

        public async Task<ApiResult<TransferReqDTO>> GetTransferAsync(int transferId)
        {
            var transfer = await _dbCtx.TransferReqs.AsNoTracking()
                .Where(t => t.TransferId == transferId)
                .Select(t => new TransferReqDTO(t))
                .FirstOrDefaultAsync();

            if (transfer == null)
                return new ApiResult<TransferReqDTO>(ApiResultCode.NotFound, "Transfer request not found");

            return new ApiResult<TransferReqDTO>(transfer);
        }

        public async Task<ApiResult<PagedResult<TransferReqDTO>>> GetTransferReqListAsync(PagingRequest request)
        {
            var query = _dbCtx.TransferReqs.AsNoTracking();

            int totalCount = await query.CountAsync();
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TransferReqDTO(t))
                .ToListAsync();

            var pagedResult = new PagedResult<TransferReqDTO>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<TransferReqDTO>>(pagedResult);
        }



        private TransferStatus MapDecisionToStatus(TransferDecisionType decision) { 
            return decision switch { 
                TransferDecisionType.Approve => TransferStatus.Approved, 
                TransferDecisionType.Reject => TransferStatus.Rejected,
            }; 
        }
        public async Task<ApiResult> PostTransferDecisionAsync(int transferId, TransferDecisionType decision)
        {
            var transferReq = await _dbCtx.TransferReqs.FirstOrDefaultAsync(i => i.TransferId == transferId);

            if(transferReq == null)
            {
                return new ApiResult(ApiResultCode.NotFound);
            }
            switch (decision)
            {
                case TransferDecisionType.Approve:
                    transferReq.StatusInt = (int)MapDecisionToStatus(decision);
                    transferReq.ApproverId = 2;
                    break;
                case TransferDecisionType.Reject:
                    transferReq.StatusInt = (int)MapDecisionToStatus(decision);
                    transferReq.ApproverId = 2;
                    break;
                default:
                    return new ApiResult(ApiResultCode.InvalidRequest);
            }
            await _dbCtx.SaveChangesAsync();

            return new ApiResult();
        }

    }

}
