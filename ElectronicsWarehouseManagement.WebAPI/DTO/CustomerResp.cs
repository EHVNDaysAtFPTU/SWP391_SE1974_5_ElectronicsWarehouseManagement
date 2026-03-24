using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CustomerResp
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("transfer_requests")]
        public List<TransferRequest>? TransferRequests { get; set; }
        
        public CustomerResp(Customer customer, bool fullInfo)
        {
            ID = customer.CustomerId;
            Name = customer.CustomerName;
            Phone = customer.Phone;
            Email = customer.Email;
            Address = customer.Address;
            CreatedAt = customer.CreatedAt;
            if (fullInfo)
                TransferRequests = customer.TransferRequests.ToList();
        }
    }
}
