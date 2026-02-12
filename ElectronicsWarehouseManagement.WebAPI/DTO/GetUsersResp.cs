using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.Repositories.ExternalEntities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class GetUsersResp
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("status")]
        public UserStatus Status { get; set; }

        public GetUsersResp(int userId, string username, string email, UserStatus status)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Status = status;
        }

        public GetUsersResp(User user)
        {
            UserId = user.UserId;
            Username = user.Username;
            Email = user.Email;
            Status = (UserStatus)user.Status;
        }
    }
}
