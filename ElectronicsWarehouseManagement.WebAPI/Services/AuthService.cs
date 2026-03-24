using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.WebAPI.DTO;
using ElectronicsWarehouseManagement.WebAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

public interface IAuthService
{
    Task<(ApiResult resp, User? user)> LoginAsync(LoginReq request);
    Task<ApiResult> LogoutAsync(int userId);
    Task<ApiResult> ForgotPasswordAsync(ForgotPasswordReq request, string baseUrl);
    Task<ApiResult> ResetPasswordAsync(ResetPasswordReq request);
}

internal class AuthService : IAuthService
{
    readonly EWMDbCtx _dbCtx;
    readonly IEmailService _emailService;
    readonly ILogger<AuthService> _logger;

    static List<int> loggedInUsers = [];
    static Dictionary<string, (int userId, DateTime expiration)> _passwordResetTokens = [];

    public AuthService(EWMDbCtx dbCtx, IEmailService emailService, ILogger<AuthService> logger)
    {
        _dbCtx = dbCtx;
        _emailService = emailService;
        _logger = logger;
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

    public async Task<ApiResult> ForgotPasswordAsync(ForgotPasswordReq request, string baseUrl)
    {
        if (!request.Verify(out string failedReason))
            return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
        var user = await _dbCtx.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            return new ApiResult(ApiResultCode.NotFound, "User with this email not found.");
        foreach (var kvp in _passwordResetTokens)
        {
            if (kvp.Value.userId == user.UserId)
            {
                if (DateTime.UtcNow > kvp.Value.expiration)
                    _passwordResetTokens.Remove(kvp.Key);
                else
                    return new ApiResult(ApiResultCode.InvalidRequest, "A reset token has already been sent. Please check your email.");
            }
        }
        var token = GenerateResetToken();
        var expirationTime = DateTime.UtcNow.AddMinutes(10);
        _passwordResetTokens[token] = (user.UserId, expirationTime);
        var resetLink = $"{baseUrl}/auth/reset-password?token={Uri.EscapeDataString(token)}";
        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);
            return new ApiResult();
        }
        catch (Exception)
        {
            return new ApiResult(ApiResultCode.InternalError, "Failed to send reset email. Please try again later.");
        }
    }

    public async Task<ApiResult> ResetPasswordAsync(ResetPasswordReq request)
    {
        if (!request.Verify(out string failedReason))
            return new ApiResult(ApiResultCode.InvalidRequest, failedReason);
        if (!_passwordResetTokens.TryGetValue(request.Token, out var tokenData))
            return new ApiResult(ApiResultCode.InvalidRequest, "Invalid or expired reset token.");
        if (DateTime.UtcNow > tokenData.expiration)
        {
            _passwordResetTokens.Remove(request.Token);
            return new ApiResult(ApiResultCode.InvalidRequest, "Reset token has expired.");
        }
        var user = await _dbCtx.Users.FindAsync(tokenData.userId);
        if (user is null)
            return new ApiResult(ApiResultCode.NotFound);
        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        _dbCtx.Users.Update(user);
        await _dbCtx.SaveChangesAsync();
        _passwordResetTokens.Remove(request.Token);
        _logger.LogInformation($"Password reset successfully for user {user.UserId}");
        return new ApiResult();
    }

    static string GenerateResetToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
}