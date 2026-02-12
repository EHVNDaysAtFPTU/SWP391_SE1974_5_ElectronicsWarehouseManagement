
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class SetRoleReq
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("role_id")]
        public int RoleId { get; set; }

        internal bool Verify()
        {
            return true;
        }
    }
}
