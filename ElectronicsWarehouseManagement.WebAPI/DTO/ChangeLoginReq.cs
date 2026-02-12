using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ChangeLoginReq
    {
        [JsonPropertyName("user")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        internal bool Verify()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email))
                return false;
            if (Username.Length > 256 || Email.Length > 256)
                return false;
            return true;
        }
    }
}
