using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class GetUserResp
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("status")]
        public UserStatus Status { get; set; }

        [JsonPropertyName("role")]
        public List<GetRolesResp> Roles { get; set; } = [];

        public GetUserResp(User user)
        {
            UserId = user.UserId;
            Username = user.Username;
            Email = user.Email;
            Status = user.Status;
            foreach (var role in user.Roles)
                Roles.Add(new GetRolesResp(role));
        }

        public GetUserResp(int userId, string username, string email, UserStatus status, IEnumerable<GetRolesResp> roles)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Status = status;
            Roles.AddRange(roles);
        }
    }
}
