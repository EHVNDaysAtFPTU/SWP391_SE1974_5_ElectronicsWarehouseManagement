
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class SetRoleReq : IVerifiableRequest
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("role_id")]
        public int RoleId { get; set; }

        public bool Verify(out string message) 
        {
            if (UserId < 1) 
            {
                message = "UserId is required.";
                return false;
            }
            if (RoleId < 1) 
            {
                message = "RoleId is required.";
                return false;
            }
            message = string.Empty;
            return true;
        }
    }
}
