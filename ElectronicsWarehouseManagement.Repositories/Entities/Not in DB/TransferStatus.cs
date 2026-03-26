namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public enum TransferStatus
    {
        Unknown,
        Pending,
        ApprovedAndWaitForConfirm,
        Rejected,
        Finished,
        MissingComponents,
        Canceled
    }
}
