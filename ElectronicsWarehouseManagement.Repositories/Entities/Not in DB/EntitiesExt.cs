#pragma warning disable CS0618 // Type or member is obsolete
using System.Text.Json;

namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public static class EntitiesExt
    {
        extension(Component component)
        {
            public ComponentMetadata? Metadata
            {
                get => JsonSerializer.Deserialize<ComponentMetadata>(component.MetadataJson);
                set => component.MetadataJson = JsonSerializer.Serialize(value);
            }

            public double TotalQuantity => component.ComponentBins.Sum(cb => cb.Quantity);

            public double TotalPrice => component.TotalQuantity * component.UnitPrice;
        }

        extension(User user)
        {
            public UserStatus Status
            {
                get => (UserStatus)user.StatusInt;
                set => user.StatusInt = (int)value;
            }
        }

        extension(TransferRequest transferRequest)
        {
            public TransferStatus Status
            {
                get => (TransferStatus)transferRequest.StatusInt;
                set => transferRequest.StatusInt = (int)value;
            }

            public TransferType Type
            {
                get => (TransferType)transferRequest.TypeInt;
                set => transferRequest.TypeInt = (int)value;
            }
        }

        extension(Bin bin)
        {
            public BinStatus Status
            {
                get => (BinStatus)bin.StatusInt;
                set => bin.StatusInt = (int)value;
            }
        }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete