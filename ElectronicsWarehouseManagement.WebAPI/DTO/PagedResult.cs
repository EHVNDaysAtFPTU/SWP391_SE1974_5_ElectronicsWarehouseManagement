namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class PagedResult<T>
    {
        public List<T> Item { get; set; }
        public int TotalRecord {  get; set; }
        public int PageIndex { get; set; }
        public int PageSize {  get; set; }
    }
}
