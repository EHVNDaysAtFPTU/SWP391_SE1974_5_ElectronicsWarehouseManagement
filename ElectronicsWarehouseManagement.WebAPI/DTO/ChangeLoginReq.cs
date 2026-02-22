using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ChangeLoginReq : IVerifiableRequest
    {
        [JsonPropertyName("user")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        public bool Verify(out string failedReason)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
            {
                failedReason = "Username or email cannot be empty.";
                return false;
            }
            if (Username.Length > 256 || Email.Length > 256)
            {
                failedReason = "Username or email is too long.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
