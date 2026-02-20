using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.Repositories.ExternalEntities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IManagerService
    {
        Task<ApiResult<PagedResult<ItemDTO>>> GetItemListAsync(PagingRequest request);
        Task<ApiResult<ItemDTO>> GetItemAsync(int itemId);
        Task<ApiResult<List<TransferReq>>> GetTransferReqList();
    }
    public class ManagerService : IManagerService
    {
        readonly EWMDbCtx _dbCtx;
        public ManagerService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }
        public async Task<ApiResult<ItemDTO>> GetItemAsync(int itemId)
        {
            try
            {
                var query = _dbCtx.Items.AsNoTracking();
                var result = await query.Where(i => i.ItemId == itemId).Select(i => new ItemDTO(i)).FirstOrDefaultAsync();
                if(result == null)
                {
                    return new ApiResult<ItemDTO>(ApiResultCode.NotFound);
                }
                return new ApiResult<ItemDTO>(result);
            }catch(Exception)
            {
                return new ApiResult<ItemDTO>(ApiResultCode.UnknownError);
            }
        }

        public async Task<ApiResult<PagedResult<ItemDTO>>> GetItemListAsync(PagingRequest request)
        {
            try
            {
                var query = _dbCtx.Items.AsNoTracking();

                int totalCount = await query.CountAsync();

                var items = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(i => new ItemDTO(i))
                    .ToListAsync();

                var pagedResult = new PagedResult<ItemDTO>
                {
                    data = items,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return new ApiResult<PagedResult<ItemDTO>>(pagedResult);
            }
            catch (Exception ex)
            {
                return new ApiResult<PagedResult<ItemDTO>>(ApiResultCode.UnknownError, ex.Message);
            }
        }
        public Task<ApiResult<List<TransferReq>>> GetTransferReqList()
        {
            throw new NotImplementedException();
        }

    }
}
