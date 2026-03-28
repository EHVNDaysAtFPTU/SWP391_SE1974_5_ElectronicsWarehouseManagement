using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.DTO
{
    public class SetStatusReq : IVerifiableRequest
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        public bool Verify(out string failedReason)
        {
            if (!Enum.IsDefined(typeof(UserStatus), Status))
            {
                failedReason = "Invalid status value.";
                return false;
            }
            UserStatus userStatus = (UserStatus)Status;
            if (userStatus == UserStatus.Uninitialized)
            {
                failedReason = "Status cannot be Uninitialized.";
                return false;
            }
            if (userStatus == UserStatus.Deleted)
            {
                failedReason = "Status cannot be Deleted.";
                return false;
            }
            failedReason = string.Empty;
            return true;
        }
    }
}
