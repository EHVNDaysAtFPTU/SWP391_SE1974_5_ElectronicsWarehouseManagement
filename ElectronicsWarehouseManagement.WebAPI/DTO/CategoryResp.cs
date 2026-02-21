using ElectronicsWarehouseManagement.Repositories.Entities;
using System.Text.Json.Serialization;

namespace ElectronicsWarehouseManagement.WebAPI.DTO
{
    public class CategoryResp
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        public CategoryResp(Category category)
        {
            Id = category.CategoryId;
            Name = category.CategoryName;
        }

        public CategoryResp(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
