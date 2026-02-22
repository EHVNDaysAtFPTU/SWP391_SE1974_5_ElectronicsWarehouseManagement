namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public interface IVerifiableRequest
    {
        public bool Verify(out string failedReason);
    }
}
