using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicsWarehouseManagement.Repositories.Entities 
{ public enum TransferStatus 
    {
        Unknown,
        Pending,
        ApprovedAndWaitForConfirm,
        Rejected,
        Confirmed,
    }
}
