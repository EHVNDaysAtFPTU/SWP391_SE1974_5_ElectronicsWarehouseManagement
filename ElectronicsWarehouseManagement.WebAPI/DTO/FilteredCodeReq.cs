namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class FilteredCodeReq
    {
        public bool filtered { get; set; }
        public int? filterCode {  get; set; }
        public string filterAction {  get; set; }

    }
    public enum FilteredCode
    {
        None,
        BySortId,
        BySortName,
        ByName,
    }
}
