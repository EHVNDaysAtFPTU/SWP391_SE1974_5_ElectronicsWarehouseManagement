namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public enum UserStatus
    {
        Uninitialized,
        Active,
        Inactive,
        Suspended,
        Deleted = int.MaxValue
    }
}
