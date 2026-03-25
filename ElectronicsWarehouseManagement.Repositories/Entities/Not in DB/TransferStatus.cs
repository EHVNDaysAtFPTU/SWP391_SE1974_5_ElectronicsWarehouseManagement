namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public enum TransferStatus
    {
        Unknown,
        Pending,
        ApprovedAndWaitForConfirm,
        Rejected,
        Confirmed,
        MissingComponents,
        Canceled
    }
}
