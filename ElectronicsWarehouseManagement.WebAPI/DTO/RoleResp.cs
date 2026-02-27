using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class RoleResp
    {
        [JsonPropertyName("role_id")]
        public int RoleId { get; set; }

        [JsonPropertyName("role_name")]
        public string RoleName { get; set; }

        [JsonPropertyName("desc")]
        public string Description { get; set; }

        public RoleResp(Role role)
        {
            RoleId = role.RoleId;
            RoleName = role.RoleName;
            Description = role.Description;
        }
    }
}
