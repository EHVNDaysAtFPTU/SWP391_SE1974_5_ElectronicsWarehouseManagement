using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.Repositories.ExternalEntities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IManagerService
    {
        Task<ApiResult<PagedResult<ItemDTO>>> GetItemList();
        Task<ApiResult<ItemDTO>> GetItem(int itemId);
        Task<ApiResult<List<TransferReq>>> GetTransferReqList();
    }
    public class ManagerService : IManagerService
    {
        readonly EWMDbCtx _dbCtx;
        public ManagerService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }
        public async Task<ApiResult<ItemDTO>> GetItem(int itemId)
        {
            try
            {
                var result = await _dbCtx.Items.Where(x => x.ItemId == itemId).Select(x => new ItemDTO
                {
                    ItemId = x.ItemId,
                    Unit = x.Unit,
                    UnitPrice = x.UnitPrice,
                }).FirstOrDefaultAsync();
                return new ApiResult<ItemDTO>(result);
            }
            catch (Exception ex)
            {
                return new ApiResult<ItemDTO>(ApiResultCode.NotFound);
            }
            

        }

        public async Task<ApiResult<PagedResult<ItemDTO>>> GetItemList()
        {
            try
            {
                int totalCount = await _dbCtx.Items.CountAsync();
                var data = await _dbCtx.Items.AsNoTracking().Select(i => new ItemDTO
                {
                    ItemId = i.ItemId,
                    Unit = i.Unit,
                    Categories = i.Categories,
                    UnitPrice = i.UnitPrice

                }).ToListAsync();
                var pagedResult = new PagedResult<ItemDTO>
                {
                    Item = data,
                    TotalRecord = totalCount,
                    PageIndex = 1,
                    PageSize = totalCount
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
