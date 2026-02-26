using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class ComponentCategoryResp
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        public ComponentCategoryResp(ComponentCategory category)
        {
            Id = category.CategoryId;
            Name = category.CategoryName;
        }
    }
}
