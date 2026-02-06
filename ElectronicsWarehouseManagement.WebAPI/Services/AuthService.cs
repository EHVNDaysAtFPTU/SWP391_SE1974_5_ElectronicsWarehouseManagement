using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public interface IAuthService
{
    Task<ApiResult> ChangePasswordAsync(string username, ChangePasswordReq request);

    //Task<ApiResult<Guid>> Register(RegisterRequest request);
    Task<ApiResult> LoginAsync(LoginReq request);
    //Task<ApiResult<TokenResponse>> LoginDashboard(LoginDashboardRequest request);
    //Task<ApiResult<TokenResponse>> Refresh(RefreshRequest request);
    //Task<ApiResult<bool>> Logout(LogoutRequest request);
    //Task<ApiResult<bool>> ForgotPassword(ForgotPasswordRequest request);
    //Task<ApiResult<bool>> ResetPassword(ResetPasswordRequest request);
}

class AuthService : IAuthService
{
    readonly EWMDbCtx _dbCtx;

    public AuthService(EWMDbCtx dbCtx)
    {
        _dbCtx = dbCtx;
    }

    public async Task<ApiResult> LoginAsync(LoginReq request)
    {
        if (!request.Verify())
            return new ApiResult(ApiResultCode.InvalidRequest);
        var user = await _dbCtx.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);
        if (user is null)
            return new ApiResult(ApiResultCode.NotFound);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(request.Password));
        if (!string.Equals(user.PasswordHash, Convert.ToBase64String(hash), StringComparison.Ordinal))
            return new ApiResult(ApiResultCode.IncorrectCred);
        return new ApiResult();
    }

    public async Task<ApiResult> ChangePasswordAsync(string username, ChangePasswordReq request)
    {
        if (!request.Verify())
            return new ApiResult(ApiResultCode.InvalidRequest);
        if (request.OldPassword == request.NewPassword)
            return new ApiResult(ApiResultCode.InvalidRequest, "New password cannot be the same as the old password.");
        var user = await _dbCtx.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
            return new ApiResult(ApiResultCode.NotFound);
        var oldHash = SHA256.HashData(Encoding.UTF8.GetBytes(request.OldPassword));
        if (!string.Equals(user.PasswordHash, Convert.ToBase64String(oldHash), StringComparison.Ordinal))
            return new ApiResult(ApiResultCode.IncorrectCred);
        var newHash = SHA256.HashData(Encoding.UTF8.GetBytes(request.NewPassword));
        user.PasswordHash = Convert.ToBase64String(newHash);
        await _dbCtx.SaveChangesAsync();
        return new ApiResult();
    }
}


/*
#r "System.Security.Cryptography.Algorithms.dll"
using System.Security.Cryptography;
Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("P@ssW0rd")))
 */