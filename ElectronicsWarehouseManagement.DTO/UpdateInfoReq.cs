using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class UpdateInfoReq : IVerifiableRequest
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("user")]
        public string? Username { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        public bool Verify(out string failedReason)
        {
            if (Username is not null)
            {
                if (string.IsNullOrEmpty(Username))
                {
                    failedReason = "Username cannot be empty.";
                    return false;
                }
                if (!Username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
                {
                    failedReason = "Username can only contain letters, digits, underscores, or hyphens.";
                    return false;
                }
            }
            if (DisplayName is not null)
            {
                if (string.IsNullOrEmpty(DisplayName))
                {
                    failedReason = "Display name cannot be empty.";
                    return false;
                }
                if (DisplayName.Length < 3 || DisplayName.Length > 50)
                {
                    failedReason = "Display name is too short or too long.";
                    return false;
                }
            }
            if (Email is not null)
            {
                if (string.IsNullOrEmpty(Email))
                {
                    failedReason = "Email cannot be empty.";
                    return false;
                }
                try
                {
                    var addr = new System.Net.Mail.MailAddress(Email);
                    if (addr.Address != Email)
                    {
                        failedReason = "Invalid email format.";
                        return false;
                    }
                }
                catch
                {
                    failedReason = "Invalid email format.";
                    return false;
                }
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
