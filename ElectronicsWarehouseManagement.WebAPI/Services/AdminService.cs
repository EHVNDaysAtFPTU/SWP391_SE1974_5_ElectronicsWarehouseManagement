using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.Repositories.ExternalEntities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IAdminService
    {
        Task<ApiResult> CreateAccountAsync(CreateAccReq createAccReq);
        Task<ApiResult> DeleteAccountAsync(int userId);
        Task<ApiResult<List<GetRolesResp>>> GetRolesAsync();
        Task<ApiResult<List<GetUsersResp>>> GetUsersAsync();
        Task<ApiResult> SetRoleAsync(SetRoleReq setRoleReq);
        Task<ApiResult<GetUserResp>> GetUserAsync(int userId);
        Task<ApiResult<List<GetUsersResp>>> SearchUsersAsync(string query);
    }

    public class AdminService : IAdminService
    {
        readonly EWMDbCtx _dbCtx;

        public AdminService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<ApiResult> CreateAccountAsync(CreateAccReq createAccReq)
        {
            if (!createAccReq.Verify())
                return new ApiResult(ApiResultCode.InvalidRequest);
            // Check if username or email already exists
            User? existingUser = await _dbCtx.Users.FirstOrDefaultAsync(u => u.Username == createAccReq.Username || u.Email == createAccReq.Email);
            if (existingUser is not null && existingUser.Status != (int)UserStatus.Deleted)
            {
                if (existingUser.Username == createAccReq.Username)
                    return new ApiResult(ApiResultCode.InvalidRequest, "Username already exists.");
                if (existingUser.Email == createAccReq.Email)
                    return new ApiResult(ApiResultCode.InvalidRequest, "Email already exists.");
            }
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(createAccReq.Password));
            var role = await _dbCtx.Roles.FirstOrDefaultAsync(r => r.RoleId == createAccReq.RoleId);
            if (role is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "Role does not exist.");
            if (role.RoleId == 1)
                return new ApiResult(ApiResultCode.InvalidRequest, "Cannot assign Admin role.");
            if (existingUser is null)
            {
                var user = new User
                {
                    Username = createAccReq.Username,
                    Email = createAccReq.Email,
                    PasswordHash = Convert.ToBase64String(hash),
                    Status = (int)UserStatus.Active,
                    Roles = [role]
                };
                _dbCtx.Users.Add(user);
            }
            else
            {
                existingUser.PasswordHash = Convert.ToBase64String(hash);
                existingUser.Email = createAccReq.Email;
                existingUser.Status = (int)UserStatus.Active;
                existingUser.Roles = [role];
            }
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<List<GetRolesResp>>> GetRolesAsync() => new ApiResult<List<GetRolesResp>>((await _dbCtx.Roles.AsNoTracking().ToListAsync()).Select(r => new GetRolesResp(r)).ToList());

        public async Task<ApiResult<List<GetUsersResp>>> GetUsersAsync() => new ApiResult<List<GetUsersResp>>((await _dbCtx.Users.AsNoTracking().Include(u => u.Roles).ToListAsync()).Select(u => new GetUsersResp(u)).ToList());

        public async Task<ApiResult> SetRoleAsync(SetRoleReq setRoleReq)
        {
            if (!setRoleReq.Verify())
                return new ApiResult(ApiResultCode.InvalidRequest);
            var user = await _dbCtx.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == setRoleReq.UserId);
            if (user is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "User does not exist.");
            var role = await _dbCtx.Roles.FirstOrDefaultAsync(r => r.RoleId == setRoleReq.RoleId);
            if (role is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "Role does not exist.");
            user.Roles = [role];
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult> DeleteAccountAsync(int userId)
        {
            var user = await _dbCtx.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "User does not exist.");
            user.Status = (int)UserStatus.Deleted;
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<GetUserResp>> GetUserAsync(int userId)
        {
            var user = await _dbCtx.Users.AsNoTracking().Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null)
                return new ApiResult<GetUserResp>(ApiResultCode.InvalidRequest, "User does not exist.");
            return new ApiResult<GetUserResp>(new GetUserResp(user));
        }

        public async Task<ApiResult<List<GetUsersResp>>> SearchUsersAsync(string query)
        {
            var users = await _dbCtx.Users.AsNoTracking().Include(u => u.Roles)
                .Where(u => u.Username.Contains(query) || u.Email.Contains(query))
                .ToListAsync();
            return new ApiResult<List<GetUsersResp>>(users.Select(u => new GetUsersResp(u)).ToList());
        }
    }
}
