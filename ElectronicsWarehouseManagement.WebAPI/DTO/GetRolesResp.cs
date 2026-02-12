using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class GetRolesResp
    {
        [JsonPropertyName("role_id")]
        public int RoleId { get; set; }

        [JsonPropertyName("role_name")]
        public string RoleName { get; set; }

        [JsonPropertyName("desc")]
        public string Description { get; set; }

        public GetRolesResp(int roleId, string roleName, string description)
        {
            RoleId = roleId;
            RoleName = roleName;
            Description = description;
        }

        public GetRolesResp(Role role)
        {
            RoleId = role.RoleId;
            RoleName = role.RoleName;
            Description = role.Description;
        }
    }
}
