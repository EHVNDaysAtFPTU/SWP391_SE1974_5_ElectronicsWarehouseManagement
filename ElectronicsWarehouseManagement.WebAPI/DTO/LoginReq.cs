using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class LoginReq : IVerifiableRequest
    {
        [JsonPropertyName("user")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [JsonPropertyName("pass")]
        public string Password { get; set; } = string.Empty;

        public bool Verify(out string failedReason)
        {
            if (string.IsNullOrWhiteSpace(UsernameOrEmail) || string.IsNullOrWhiteSpace(Password))
            {
                failedReason = "Username/email or password cannot be empty.";
                return false;
            }
            if (UsernameOrEmail.Length > 256 || Password.Length > 256)
            {
                failedReason = "Username/email or password is too long.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}