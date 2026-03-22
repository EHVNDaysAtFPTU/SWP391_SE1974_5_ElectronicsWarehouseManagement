#nullable disable
using System;

namespace ElectronicsWarehouseManagement.Repositories.Entities;

public partial class TransferRequest
{
    // Map to DB column customer_id if present
    public int? CustomerId { get; set; }
}
