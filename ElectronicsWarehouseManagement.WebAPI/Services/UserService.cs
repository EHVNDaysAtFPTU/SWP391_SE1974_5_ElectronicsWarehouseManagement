using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsWarehouseManagement.WebAPI.Services
{
    public interface IUserService
    {
        Task<ApiResult> ChangePasswordAsync(string username, ChangePasswordReq request);
        Task<ApiResult<UserResp>> UpdateInfoAsync(string username, UpdateInfoReq request);
        Task<ApiResult<UserResp>> GetMyInfoAsync(string username);
    }

    internal class UserService : IUserService
    {
        readonly EWMDbCtx _dbCtx;

        public UserService(EWMDbCtx dbCtx)
        {
            _dbCtx = dbCtx;
        }

        public async Task<ApiResult> ChangePasswordAsync(string username, ChangePasswordReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
            if (request.OldPassword == request.NewPassword)
                return new ApiResult(ApiResultCode.InvalidRequest, "New password cannot be the same as the old password.");
            var user = await _dbCtx.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null)
                return new ApiResult(ApiResultCode.NotFound);
            var passwordHasher = new PasswordHasher<User>();
            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword) != PasswordVerificationResult.Success)
                return new ApiResult(ApiResultCode.IncorrectCred);
            user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
            await _dbCtx.SaveChangesAsync();
            return new ApiResult();
        }

        public async Task<ApiResult<UserResp>> UpdateInfoAsync(string username, UpdateInfoReq request)
        {
            if (!request.Verify(out string failedReason))
                return new ApiResult<UserResp>(ApiResultCode.InvalidRequest, failedReason);
            var user = await _dbCtx.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Username == username);
            if (user is null)
                return new ApiResult<UserResp>(ApiResultCode.NotFound);
            if (request.Username is not null)
            {
                bool usernameExists = await _dbCtx.Users.AnyAsync(u => u.Username == request.Username && u.UserId != user.UserId);
                if (usernameExists)
                    return new ApiResult<UserResp>(ApiResultCode.InvalidRequest, "Username already exists.");
                user.Username = request.Username;
            }
            if (request.DisplayName is not null)
            {
                user.DisplayName = request.DisplayName;
            }
            if (request.Email is not null)
            {
                bool emailExists = await _dbCtx.Users.AnyAsync(u => u.Email == request.Email && u.UserId != user.UserId);
                if (emailExists)
                    return new ApiResult<UserResp>(ApiResultCode.InvalidRequest, "Email already exists.");
                user.Email = request.Email;
            }
            await _dbCtx.SaveChangesAsync();
            return new ApiResult<UserResp>(new UserResp(user, true));
        }

        public async Task<ApiResult<UserResp>> GetMyInfoAsync(string username)
        {
            var user = await _dbCtx.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Username == username);
            if (user is null)
                return new ApiResult<UserResp>(ApiResultCode.NotFound);
            return new ApiResult<UserResp>(new UserResp(user, true));
        }
    }
}
