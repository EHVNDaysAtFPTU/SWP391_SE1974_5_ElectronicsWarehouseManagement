using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO;

public class ForgotPasswordReq : IVerifiableRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = "";

    public bool Verify(out string failedReason)
    {
        failedReason = "";
        if (string.IsNullOrWhiteSpace(Email))
        {
            failedReason = "Email is required.";
            return false;
        }
        if (!Email.Contains("@"))
        {
            failedReason = "Invalid email format.";
            return false;
        }
        return true;
    }
}

public class ResetPasswordReq : IVerifiableRequest
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = "";

    [JsonPropertyName("new_password")]
    public string NewPassword { get; set; } = "";

    [JsonPropertyName("confirm_password")]
    public string ConfirmPassword { get; set; } = "";

    public bool Verify(out string failedReason)
    {
        failedReason = "";
        if (string.IsNullOrWhiteSpace(Token))
        {
            failedReason = "Reset token is required.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            failedReason = "New password is required.";
            return false;
        }
        if (NewPassword.Length < 6)
        {
            failedReason = "Password must be at least 6 characters long.";
            return false;
        }
        if (NewPassword != ConfirmPassword)
        {
            failedReason = "Passwords do not match.";
            return false;
        }
        return true;
    }
}
