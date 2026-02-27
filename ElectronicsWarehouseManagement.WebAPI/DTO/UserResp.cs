using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class UserResp
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("status")]
        public UserStatus Status { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("role")]
        public List<RoleResp>? Roles { get; set; }

        public UserResp(User user, bool fullInfo)
        {
            UserId = user.UserId;
            Username = user.Username;
            Email = user.Email;
            Status = user.Status;
            if (fullInfo)
                Roles = user.Roles.Select(r => new RoleResp(r)).ToList();
        }
    }
}
