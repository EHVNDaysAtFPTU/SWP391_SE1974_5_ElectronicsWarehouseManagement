
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class SetRoleReq : IVerifiableRequest
    {
        [JsonPropertyName("role_ids")]
        public int[] RoleIds { get; set; } = [];

        public bool Verify(out string message) 
        {
            if (RoleIds is null || RoleIds.Length == 0)
            {
                message = "At least one RoleId is required.";
                return false;
            }
            foreach (var roleId in RoleIds)
            {
                if (roleId >= 1)
                    continue;
                message = "RoleId is required.";
                return false;
            }
            message = string.Empty;
            return true;
        }
    }
}
