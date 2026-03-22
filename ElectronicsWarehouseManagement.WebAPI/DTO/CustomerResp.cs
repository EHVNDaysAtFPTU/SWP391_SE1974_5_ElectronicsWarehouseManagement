using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CustomerResp
    {
        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; }

        [JsonPropertyName("customer_name")]
        public string CustomerName { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }
        public CustomerResp() { }

        public CustomerResp(Customer c)
        {
            CustomerId = c.CustomerId;
            CustomerName = c.CustomerName;
            Phone = c.Phone;
            Email = c.Email;
            Address = c.Address;
            CreatedAt = c.CreatedAt;
        }
    }
}
