using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

public interface IAuthService
{
    Task<ApiResult> ChangeLoginAsync(string username, ChangeLoginReq request);
    Task<ApiResult> ChangePasswordAsync(string username, ChangePasswordReq request);
    Task<(ApiResult resp, User? user)> LoginAsync(LoginReq request);
}

class AuthService : IAuthService
{
    readonly EWMDbCtx _dbCtx;

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

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(request.Password));

        if (!string.Equals(user.PasswordHash,
            Convert.ToBase64String(hash),
            StringComparison.Ordinal))
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
        return (new ApiResult(), user);
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
        var oldHash = SHA256.HashData(Encoding.UTF8.GetBytes(request.OldPassword));
        if (!string.Equals(user.PasswordHash, Convert.ToBase64String(oldHash), StringComparison.Ordinal))
            return new ApiResult(ApiResultCode.IncorrectCred);
        var newHash = SHA256.HashData(Encoding.UTF8.GetBytes(request.NewPassword));
        user.PasswordHash = Convert.ToBase64String(newHash);
        await _dbCtx.SaveChangesAsync();
        return new ApiResult();
    }

    public async Task<ApiResult> ChangeLoginAsync(string username, ChangeLoginReq request)
    {
        if (!request.Verify(out string failedReason))
            return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
        var user = await _dbCtx.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
            return new ApiResult(ApiResultCode.NotFound);
        // Check if new username or email already exists
        bool usernameExists = await _dbCtx.Users.AnyAsync(u => u.Username == request.Username && u.UserId != user.UserId);
        if (usernameExists)
            return new ApiResult(ApiResultCode.InvalidRequest, "Username already exists.");
        bool emailExists = await _dbCtx.Users.AnyAsync(u => u.Email == request.Email && u.UserId != user.UserId);
        if (emailExists)
            return new ApiResult(ApiResultCode.InvalidRequest, "Email already exists.");
        user.Username = request.Username;
        user.Email = request.Email;
        await _dbCtx.SaveChangesAsync();
        return new ApiResult();
    }
}


/*
#r "System.Security.Cryptography.Algorithms.dll"
using System.Security.Cryptography;
Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes("P@ssW0rd")))
 */