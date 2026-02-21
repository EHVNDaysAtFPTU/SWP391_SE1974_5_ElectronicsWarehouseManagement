namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class TransferDecisionRequest
    {
        public TransferDecisionType Decision { get; set; }
    }
    public enum TransferDecisionType
    {
        Approve = 1,
        Reject = 2,
        Cancel = 3
    }


}
