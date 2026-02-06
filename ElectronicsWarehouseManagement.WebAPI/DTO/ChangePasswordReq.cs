using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ChangePasswordReq
    {
        [JsonPropertyName("old_pass")]
        public string OldPassword { get; set; } = string.Empty;

        [JsonPropertyName("new_pass")]
        public string NewPassword { get; set; } = string.Empty;

        public bool Verify()
        {
            if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword))
                return false;
            if (OldPassword.Length > 256 || NewPassword.Length > 256)
                return false;
            return true;
        }
    }
}
