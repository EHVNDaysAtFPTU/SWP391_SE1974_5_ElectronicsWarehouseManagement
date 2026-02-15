using ElectronicsWarehouseManagement.Repositories.Entities;
using ElectronicsWarehouseManagement.Repositories.ExternalEntities;
using System.Text.Json;

namespace ElectronicsWarehouseManagement.Repositories
{
    public static class Ext
    {
        extension(Item item)
        {
            public ComponentMetadata? MD
            {
                get => JsonSerializer.Deserialize<ComponentMetadata>(item.Metadata);
                set => item.Metadata = JsonSerializer.Serialize(value);
            }
        }
    }
}
