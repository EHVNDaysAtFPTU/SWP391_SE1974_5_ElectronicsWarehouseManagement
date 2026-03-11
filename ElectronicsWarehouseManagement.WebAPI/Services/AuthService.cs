using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public interface IAuthService
{
    Task<(ApiResult resp, User? user)> LoginAsync(LoginReq request);
    Task<ApiResult> LogoutAsync(int userId);
    //TODO: reset password
}

class AuthService : IAuthService
{
    readonly EWMDbCtx _dbCtx;

    List<int> loggedInUsers = [];

    public AuthService(EWMDbCtx dbCtx)
    {
        _dbCtx = dbCtx;
    }

    public async Task<(ApiResult resp, User? user)> LoginAsync(LoginReq request)
    {
        if (!request.Verify(out string failedReason))
            return (new ApiResult(ApiResultCode.InvalidRequest, failedReason), null);

        var user = await _dbCtx.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u =>
                u.Username == request.UsernameOrEmail ||
                u.Email == request.UsernameOrEmail);

        if (user is null)
            return (new ApiResult(ApiResultCode.NotFound), null);
        var passwordHasher = new PasswordHasher<User>();
        if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) != PasswordVerificationResult.Success)
            return (new ApiResult(ApiResultCode.IncorrectCred), null);
        switch (user.Status)
        {
            case UserStatus.Inactive:
                return (new ApiResult(ApiResultCode.InvalidRequest, "Account is inactive."), null);

            case UserStatus.Suspended:
                return (new ApiResult(ApiResultCode.InvalidRequest, "Account is suspended."), null);

            case UserStatus.Deleted:
                return (new ApiResult(ApiResultCode.NotFound), null);

            case UserStatus.Uninitialized:
                return (new ApiResult(ApiResultCode.InvalidRequest, "Account not initialized."), null);
        }
        if (loggedInUsers.Contains(user.UserId))
            return (new ApiResult(ApiResultCode.MultipleSessions), null);
        loggedInUsers.Add(user.UserId);
        return (new ApiResult(), user);
    }

    public async Task<ApiResult> LogoutAsync(int userId)
    {
        if (!loggedInUsers.Contains(userId))
            return new ApiResult(ApiResultCode.InvalidRequest, "User is not logged in.");
        loggedInUsers.Remove(userId);
        return new ApiResult();
    }
}