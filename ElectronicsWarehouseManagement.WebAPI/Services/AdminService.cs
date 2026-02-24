using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IAdminService
    {
        Task<ApiResult> CreateAccountAsync(CreateAccReq request);
        Task<ApiResult> DeleteAccountAsync(int userId);
        Task<ApiResult<List<GetRolesResp>>> GetRolesAsync();
        Task<ApiResult<List<GetUsersResp>>> GetUsersAsync();
        Task<ApiResult> SetRoleAsync(SetRoleReq request);
        Task<ApiResult<GetUserResp>> GetUserAsync(int userId);
        Task<ApiResult<List<GetUsersResp>>> SearchUsersAsync(string query);
        Task<ApiResult> SetStatusAsync(SetStatusReq setStatusReq);
    }

    public class AdminService : IAdminService
    {
        readonly EWMDbCtx _dbCtx;

        public AdminService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<ApiResult> CreateAccountAsync(CreateAccReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            User? existingUser = await _dbCtx.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);
            if (existingUser is not null && existingUser.Status != UserStatus.Deleted)
            {
                if (existingUser.Username == request.Username)
                    return new ApiResult(ApiResultCode.InvalidRequest, "Username already exists.");
                if (existingUser.Email == request.Email)
                    return new ApiResult(ApiResultCode.InvalidRequest, "Email already exists.");
            }
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(request.Password));
            var role = await _dbCtx.Roles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId);
            if (role is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "Role does not exist.");
            if (role.RoleId == 1)
                return new ApiResult(ApiResultCode.InvalidRequest, "Cannot assign Admin role.");
            if (existingUser is null)
            {
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = Convert.ToBase64String(hash),
                    Status = UserStatus.Active,
                    Roles = [role]
                };
                _dbCtx.Users.Add(user);
            }
            else
            {
                existingUser.PasswordHash = Convert.ToBase64String(hash);
                existingUser.Email = request.Email;
                existingUser.Status = UserStatus.Active;
                existingUser.Roles.Clear();
                existingUser.Roles.Add(role);
            }
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<List<GetRolesResp>>> GetRolesAsync() => new ApiResult<List<GetRolesResp>>((await _dbCtx.Roles.AsNoTracking().ToListAsync()).Select(r => new GetRolesResp(r)).ToList());

#pragma warning disable CS0618 // Type or member is obsolete
        public async Task<ApiResult<List<GetUsersResp>>> GetUsersAsync() => new ApiResult<List<GetUsersResp>>((await _dbCtx.Users.AsNoTracking().Include(u => u.Roles).Where(u => u.StatusInt != (int)UserStatus.Deleted).ToListAsync()).Select(u => new GetUsersResp(u)).ToList());
#pragma warning restore CS0618 // Type or member is obsolete

        public async Task<ApiResult> SetRoleAsync(SetRoleReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            var user = await _dbCtx.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == request.UserId);
            if (user is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "User does not exist.");
            var role = await _dbCtx.Roles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId);
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
            user.Status = UserStatus.Deleted;
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<GetUserResp>> GetUserAsync(int userId)
        {
            var user = await _dbCtx.Users.AsNoTracking().Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null || user.Status == UserStatus.Deleted)
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

        public async Task<ApiResult> SetStatusAsync(SetStatusReq setStatusReq)
        {
            if (!setStatusReq.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            var user = await _dbCtx.Users.FirstOrDefaultAsync(u => u.UserId == setStatusReq.UserId);
            if (user is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "User does not exist.");
            user.Status = (UserStatus)setStatusReq.Status;
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }
    }
}
