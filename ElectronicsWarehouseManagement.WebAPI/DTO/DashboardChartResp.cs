using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class DashboardChartResp
    {
        [JsonPropertyName("transfer_chart")]
        public List<ImportExportChart> transferChart { get; set; } = [];
    }

    public class ImportExportChart
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("import")]
        public double Import { get; set; }

        [JsonPropertyName("export")]
        public double Export { get; set; }
    }
}