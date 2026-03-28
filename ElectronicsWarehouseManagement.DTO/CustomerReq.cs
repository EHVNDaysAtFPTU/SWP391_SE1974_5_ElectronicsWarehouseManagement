using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ElectronicsWarehouseManagement.DTO
{
    public partial class CustomerReq
    {
        [JsonPropertyName("customer_name")]
        public string CustomerName { get; set; } = "";

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("address")]
        public string Address { get; set; } = "";
        
        public bool Verify(out string failedreason)
        {
            if (string.IsNullOrWhiteSpace(CustomerName)
                || string.IsNullOrWhiteSpace(Phone)
                || string.IsNullOrWhiteSpace(Email)
                || string.IsNullOrWhiteSpace(Address))
            {
                failedreason = "Customer name, phone, email or address can not be empty!";
                return false;
            }

            if (!EmailRegex().IsMatch(Email))
            {
                failedreason = "Invalid email format!";
                return false;
            }

            if (!PhoneNumberRegex().IsMatch(Phone))
            {
                failedreason = "Phone must be 10-11 digits!";
                return false;
            }

            failedreason = string.Empty;
            return true;
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"^\d{10,11}$")]
        private static partial Regex PhoneNumberRegex();
    }
}