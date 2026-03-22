using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public partial class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<TransferRequest> TransferRequests { get; set; } = new List<TransferRequest>();
    }
}
