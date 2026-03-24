using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IAdminService
    {
        Task<ApiResult> CreateUserAsync(CreateUserReq request);
        Task<ApiResult> DeleteUserAsync(int userId, int currentUserId);
        Task<ApiResult<List<RoleResp>>> GetRolesAsync();
        Task<ApiResult<List<UserResp>>> GetUsersAsync();
        Task<ApiResult> SetRoleAsync(int userId, SetRoleReq request, int currentUserId);
        Task<ApiResult<UserResp>> GetUserAsync(int userId);
        Task<ApiResult<List<UserResp>>> SearchUsersAsync(string query);
        Task<ApiResult> SetStatusAsync(int userId, SetStatusReq setStatusReq);
        Task<ApiResult<int>> GetUserCountAsync();
        Task<ApiResult<SystemConfigDto>> GetSystemConfigAsync();
        Task<ApiResult> SaveSystemConfigAsync(SystemConfigDto config);
        Task<ApiResult> ResetSystemConfigAsync();
        Task<bool> IsUnderMaintenanceAsync();
    }

    public class AdminService : IAdminService
    {
        readonly EWMDbCtx _dbCtx;

        public AdminService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<ApiResult> CreateUserAsync(CreateUserReq request)
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

        public async Task<ApiResult> SetRoleAsync(int userId, SetRoleReq request, int currentUserId)
        {
            if (userId < 1)
                return new ApiResult(ApiResultCode.InvalidRequest, "Invalid user ID.");
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            var user = await _dbCtx.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "User does not exist.");
            List<Role> roles = [];
            foreach (int id in request.RoleIds)
            {
                var role = await _dbCtx.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
                if (role is null)
                    return new ApiResult(ApiResultCode.InvalidRequest, "Role does not exist.");
                if (user.UserId == currentUserId && role.RoleId != 1)
                    return new ApiResult(ApiResultCode.InvalidRequest, "Cannot change own role.");
                roles.Add(role);
            }
            user.Roles = roles;
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

        public async Task<ApiResult> SetStatusAsync(int userId, SetStatusReq setStatusReq)
        {
            if (userId < 1)
                return new ApiResult(ApiResultCode.InvalidRequest, "Invalid user ID.");
            if (!setStatusReq.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            var user = await _dbCtx.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null)
                return new ApiResult(ApiResultCode.InvalidRequest, "User does not exist.");
            user.Status = (UserStatus)setStatusReq.Status;
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<int>> GetUserCountAsync()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            int count = await _dbCtx.Users.CountAsync(u => u.StatusInt != (int)UserStatus.Deleted);
#pragma warning restore CS0618 // Type or member is obsolete
            return new ApiResult<int>(count);
        }

        public async Task<ApiResult<SystemConfigDto>> GetSystemConfigAsync()
        {
            try
            {
                if (!File.Exists("systemconfig.json"))
                {
                    var defaultConfig = new SystemConfigDto
                    {
                        MaintenanceMode = false,
                        MaintenanceMessage = "",
                        MaintenanceStartTime = null,
                        MaintenanceEndTime = null
                    };

                    var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    await File.WriteAllTextAsync("systemconfig.json", json);
                    return new ApiResult<SystemConfigDto>(defaultConfig);
                }

                var content = await File.ReadAllTextAsync("systemconfig.json");
                var config = JsonSerializer.Deserialize<SystemConfigDto>(content);

                return new ApiResult<SystemConfigDto>(config!);
            }
            catch (Exception ex)
            {
                return new ApiResult<SystemConfigDto>(ApiResultCode.ServerError, ex.Message);
            }
        }

        public async Task<ApiResult> SaveSystemConfigAsync(SystemConfigDto config)
        {
            try
            {
                // validate trước
                if (config.MaintenanceStartTime.HasValue && config.MaintenanceEndTime.HasValue)
                {
                    if (config.MaintenanceStartTime > config.MaintenanceEndTime)
                        return new ApiResult(ApiResultCode.InvalidRequest, "Start must be before End");
                }

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync("systemconfig.json", json);

                return new ApiResult(); 
            }
            catch (Exception ex)
            {
                return new ApiResult(ApiResultCode.ServerError, ex.Message);
            }
        }

        public async Task<ApiResult> ResetSystemConfigAsync()
        {
            try
            {
                var defaultConfig = new SystemConfigDto
                {
                    MaintenanceMode = false,
                    MaintenanceMessage = "",
                    MaintenanceStartTime = null,
                    MaintenanceEndTime = null
                };

                var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync("systemconfig.json", json);

                return new ApiResult();
            }
            catch (Exception ex)
            {
                return new ApiResult(ApiResultCode.ServerError, ex.Message);
            }
        }
        public async Task<bool> IsUnderMaintenanceAsync()
        {
            var result = await GetSystemConfigAsync();
            if (!result.Success) return false;

            var cfg = result.Data;
            var now = DateTime.Now;

            if (cfg.MaintenanceMode)
                return true;

            if (cfg.MaintenanceStartTime.HasValue && cfg.MaintenanceEndTime.HasValue)
            {
                return now >= cfg.MaintenanceStartTime && now <= cfg.MaintenanceEndTime;
            }

            return false;
        }
    }
}
