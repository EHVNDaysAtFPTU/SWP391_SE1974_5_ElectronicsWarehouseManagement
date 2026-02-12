using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CreateAccReq
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("role_id")]
        public int RoleId { get; set; }

        public bool Verify()
        {
            if (RoleId == 0) 
                return false;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                return false;
            if (Username.Length < 3 || Username.Length > 50)
                return false;
            //TODO: validate email format, password more robustly
            if (Email.Length < 5 || Email.Length > 100 || !Email.Contains("@"))
                return false;
            if (Password.Length < 6 || Password.Length > 100)
                return false;
            return true;
        }
    }
}
