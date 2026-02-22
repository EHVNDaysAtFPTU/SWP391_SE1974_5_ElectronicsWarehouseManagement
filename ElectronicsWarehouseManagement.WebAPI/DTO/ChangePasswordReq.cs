using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ChangePasswordReq : IVerifiableRequest
    {
        [JsonPropertyName("old_pass")]
        public string OldPassword { get; set; } = string.Empty;

        [JsonPropertyName("new_pass")]
        public string NewPassword { get; set; } = string.Empty;

        public bool Verify(out string failedReason)
        {
            if (string.IsNullOrWhiteSpace(OldPassword) || string.IsNullOrWhiteSpace(NewPassword))
            {
                failedReason = "Password cannot be empty.";
                return false;
            }
            if (OldPassword.Length > 256 || NewPassword.Length > 256)
            {
                failedReason = "Password cannot exceed 256 characters.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
