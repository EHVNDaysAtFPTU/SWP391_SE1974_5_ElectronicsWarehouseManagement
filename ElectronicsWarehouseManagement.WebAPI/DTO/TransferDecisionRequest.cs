namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class TransferDecisionRequest
    {
        public TransferDecisionType Decision { get; set; }
    }
    public enum TransferDecisionType
    {
        Unknown,
        Pending,
        ApprovedAndWaitForConfirm,
        Rejected,
        Confirmed,
    }


}
