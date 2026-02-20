using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.Repositories.ExternalEntities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    using ElectronicsWarehouseManagement.Repositories.Entities;
    using ElectronicsWarehouseManagement.Repositories.ExternalEntities;

    namespace ElectronicsWarehouseManagement.WebAPI.DTO
    {
        public class GetUserResp
        {
            public int UserId { get; set; }

            public string Username { get; set; } = "";

            public string Email { get; set; } = "";

            // Trả về string thay vì enum
            public string Status { get; set; } = "";

            public List<GetRolesResp> Roles { get; set; } = new();

            public GetUserResp(User user)
            {
                UserId = user.UserId;
                Username = user.Username;
                Email = user.Email;

                // Convert enum sang string
                Status = ((UserStatus)user.Status).ToString();

                // Null-safe
                Roles = user.Roles != null
                    ? user.Roles.Select(r => new GetRolesResp(r)).ToList()
                    : new List<GetRolesResp>();
            }
        }
    }
}
