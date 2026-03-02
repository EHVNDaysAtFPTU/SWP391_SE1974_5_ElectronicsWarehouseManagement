namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class PagingRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        //search name
        public string? Search { get; set; }

        //sort
        public string? SortBy { get; set; }      // name, price, createdDate
        public string? SortDirection { get; set; } = "asc";  // asc | desc

    }
}
