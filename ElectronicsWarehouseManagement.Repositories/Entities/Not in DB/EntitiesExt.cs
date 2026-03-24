#pragma warning disable CS0618 // Type or member is obsolete
using System.Text.Json;
using System.Linq;

namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public static class EntitiesExt
    {
        // Component helpers
        public static ComponentMetadata? GetMetadata(this Component component)
        {
            if (component == null) return null;
            if (string.IsNullOrWhiteSpace(component.MetadataJson)) return null;
            try { return JsonSerializer.Deserialize<ComponentMetadata>(component.MetadataJson); } catch { return null; }
        }

        public static void SetMetadata(this Component component, ComponentMetadata? metadata)
        {
            component.MetadataJson = metadata is null ? null : JsonSerializer.Serialize(metadata);
        }

        public static double GetTotalQuantity(this Component component)
        {
            return component?.ComponentBins?.Sum(cb => cb.Quantity) ?? 0.0;
        }

        public static double GetTotalPrice(this Component component)
        {
            return component.GetTotalQuantity() * component.UnitPrice;
        }

        // User helpers
        public static UserStatus GetStatus(this User user)
            => (UserStatus)user.StatusInt;

        public static void SetStatus(this User user, UserStatus status)
            => user.StatusInt = (int)status;

        // TransferRequest helpers
        public static TransferStatus GetStatus(this TransferRequest tr)
            => (TransferStatus)tr.StatusInt;

        public static void SetStatus(this TransferRequest tr, TransferStatus status)
            => tr.StatusInt = (int)status;

        public static TransferType GetType(this TransferRequest tr)
            => (TransferType)tr.TypeInt;

        public static void SetType(this TransferRequest tr, TransferType type)
            => tr.TypeInt = (int)type;

        // Bin helpers
        public static BinStatus GetStatus(this Bin bin)
            => (BinStatus)bin.StatusInt;

        public static void SetStatus(this Bin bin, BinStatus status)
            => bin.StatusInt = (int)status;
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
