using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CreateAccReq : IVerifiableRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("role_id")]
        public int RoleId { get; set; }

        //TODO: validate email format, password more robustly
        public bool Verify(out string failedReason)
        {
            if (RoleId < 1)
            {
                failedReason = "RoleId is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
            {
                failedReason = "Username or Email is empty.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                failedReason = "Password is empty.";
                return false;
            }
            if (Username.Length < 3 || Username.Length > 50)
            {
                failedReason = "Username is too short or too long.";
                return false;
            }
            if (Email.Length < 5 || Email.Length > 100)
            {
                failedReason = "Email is too short or too long.";
                return false;
            }
            if (!Email.Contains("@"))
            {
                failedReason = "Email is invalid.";
                return false;
            }
            if (Password.Length < 6 || Password.Length > 100)
            {
                failedReason = "Password is too short or too long.";
                return false;
            }
            if (string.IsNullOrEmpty(DisplayName))
                DisplayName = Username;
            failedReason = string.Empty;
            return true;
        }
    }
}
