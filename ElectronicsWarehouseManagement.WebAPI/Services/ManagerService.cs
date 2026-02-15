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
        Task<ApiResult> GetItem(Item item);
        Task<ApiResult<List<TransferReq>>> GetTransferReqList();
    }
    public class ManagerService : IManagerService
    {
        readonly EWMDbCtx _dbCtx;
        public ManagerService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }
        public Task<ApiResult> GetItem(Item item)
        {
            throw new NotImplementedException();
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
