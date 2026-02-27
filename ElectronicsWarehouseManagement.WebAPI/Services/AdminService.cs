using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IAdminService
    {
        Task<ApiResult> CreateAccountAsync(CreateAccReq request);
        Task<ApiResult> DeleteUserAsync(int userId, int currentUserId);
        Task<ApiResult<List<RoleResp>>> GetRolesAsync();
        Task<ApiResult<List<UserResp>>> GetUsersAsync();
        Task<ApiResult> SetRoleAsync(SetRoleReq request, int currentUserId);
        Task<ApiResult<UserResp>> GetUserAsync(int userId);
        Task<ApiResult<List<UserResp>>> SearchUsersAsync(string query);
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
            var role = await _dbCtx.Roles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId);
            if (role is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "Role does not exist.");
            if (role.RoleId == 1)
                return new ApiResult(ApiResultCode.InvalidRequest, "Cannot assign Admin role.");
            var passwordHasher = new PasswordHasher<User>();
            if (existingUser is null)
            {
                var user = new User
                {
                    Username = request.Username,
                    DisplayName = request.DisplayName,
                    Email = request.Email,
                    Status = UserStatus.Active,
                    Roles = [role]
                };
                _dbCtx.Users.Add(user);
                existingUser = user;
            }
            else
            {
                existingUser.DisplayName = request.DisplayName;
                existingUser.Email = request.Email;
                existingUser.Status = UserStatus.Active;
                existingUser.Roles.Clear();
                existingUser.Roles.Add(role);
            }
            existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, request.Password);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<List<RoleResp>>> GetRolesAsync() => new ApiResult<List<RoleResp>>((await _dbCtx.Roles.AsNoTracking().ToListAsync()).Select(r => new RoleResp(r)).ToList());

#pragma warning disable CS0618 // Type or member is obsolete
        public async Task<ApiResult<List<UserResp>>> GetUsersAsync() => new ApiResult<List<UserResp>>((await _dbCtx.Users.AsNoTracking().Include(u => u.Roles).Where(u => u.StatusInt != (int)UserStatus.Deleted).ToListAsync()).Select(u => new UserResp(u, false)).ToList());
#pragma warning restore CS0618 // Type or member is obsolete

        public async Task<ApiResult> SetRoleAsync(SetRoleReq request, int currentUserId)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            var user = await _dbCtx.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == request.UserId);
            if (user is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "User does not exist.");
            var role = await _dbCtx.Roles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId);
            if (role is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "Role does not exist.");
            if (user.UserId == currentUserId && role.RoleId != 1)
                return new ApiResult(ApiResultCode.InvalidRequest, "Cannot change own role.");
            user.Roles = [role];
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult> DeleteUserAsync(int userId, int currentUserId)
        {
            var user = await _dbCtx.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "User does not exist.");
            if (user.UserId == currentUserId)
                return new ApiResult(ApiResultCode.InvalidRequest, "Cannot delete own account.");
            user.Status = UserStatus.Deleted;
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<UserResp>> GetUserAsync(int userId)
        {
            var user = await _dbCtx.Users.AsNoTracking().Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null || user.Status == UserStatus.Deleted)
                return new ApiResult<UserResp>(ApiResultCode.InvalidRequest, "User does not exist.");
            return new ApiResult<UserResp>(new UserResp(user, true));
        }

        public async Task<ApiResult<List<UserResp>>> SearchUsersAsync(string query)
        {
            var users = await _dbCtx.Users.AsNoTracking().Include(u => u.Roles)
                .Where(u => u.Username.Contains(query) || u.Email.Contains(query))
                .ToListAsync();
            return new ApiResult<List<UserResp>>(users.Select(u => new UserResp(u, false)).ToList());
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
