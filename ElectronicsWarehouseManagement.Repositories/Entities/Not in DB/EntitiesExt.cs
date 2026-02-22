#pragma warning disable CS0618 // Type or member is obsolete
using System.Text.Json;

namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public static class EntitiesExt
    {
        extension(ItemDefinition itemDef)
        {
            public ComponentMetadata? Metadata
            {
                get => JsonSerializer.Deserialize<ComponentMetadata>(itemDef.MetadataJson);
                set => itemDef.MetadataJson = JsonSerializer.Serialize(value);
            }
        }

        extension(User user)
        {
            public UserStatus Status
            {
                get => (UserStatus)user.StatusInt;
                set => user.StatusInt = (int)value;
            }
        }

        extension(TransferReq transferReq)
        {
            public TransferStatus Status
            {
                get => (TransferStatus)transferReq.StatusInt;
                set => transferReq.StatusInt = (int)value;
            }

            public TransferType Type
            {
                get => (TransferType)transferReq.TypeInt;
                set => transferReq.TypeInt = (int)value;
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