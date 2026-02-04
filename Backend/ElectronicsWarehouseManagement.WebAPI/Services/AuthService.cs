using ElectronicsWarehouseManagement.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public interface IAuthService
{
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
            return new ApiResult(1, "Invalid request");
        var user = await _dbCtx.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);
        if (user is null)
            return new ApiResult(2, "Username or email not found");
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(request.Password));
        if (!string.Equals(user.PasswordHash, Convert.ToBase64String(hash), StringComparison.Ordinal))
            return new ApiResult(3, "Incorrect password");
        return new ApiResult();
    }
}


/*
#r "System.Security.Cryptography.Algorithms.dll"
using System.Security.Cryptography;
Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("P@ssW0rd")))
 */