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
        Task<ApiResult<ItemDTO>> GetItemAsync(int itemId);
        Task<ApiResult<PagedResult<ItemDTO>>> GetItemListAsync(PagingRequest request);
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

        public async Task<ApiResult<ItemDTO>> GetItemAsync(int itemId)
        {
            var item = await _dbCtx.Items.AsNoTracking()
                .Where(i => i.ItemId == itemId)
                .Select(i => new ItemDTO(i))
                .FirstOrDefaultAsync();

            if (item == null)
                return new ApiResult<ItemDTO>(ApiResultCode.NotFound, "Item not found");

            return new ApiResult<ItemDTO>(item);
        }

        public async Task<ApiResult<PagedResult<ItemDTO>>> GetItemListAsync(PagingRequest request)
        {
            var itemList = _dbCtx.Items
                .AsNoTracking();

            int totalCount = await itemList.CountAsync();

            var data = await itemList
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new ItemDTO(i))
                .ToListAsync();

            var pagedResult = new PagedResult<ItemDTO>
            {
                data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new ApiResult<PagedResult<ItemDTO>>(pagedResult);
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
