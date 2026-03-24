using System;
using System.Linq;
using System.Text.Json;

namespace ElectronicsWarehouseManagement.Repositories.Entities
{
    public partial class Component
    {
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public ComponentMetadata? Metadata
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MetadataJson)) return null;
                try { return JsonSerializer.Deserialize<ComponentMetadata>(MetadataJson); } catch { return null; }
            }
            set
            {
                if (value is null) MetadataJson = null;
                else MetadataJson = JsonSerializer.Serialize(value);
            }
        }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public double TotalQuantity => ComponentBins?.Sum(cb => cb.Quantity) ?? 0.0;

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public double TotalPrice => TotalQuantity * UnitPrice;
    }

    public partial class User
    {
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public UserStatus Status
        {
            get => (UserStatus)StatusInt;
            set => StatusInt = (int)value;
        }
    }

    public partial class TransferRequest
    {
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public TransferStatus Status
        {
            get => (TransferStatus)StatusInt;
            set => StatusInt = (int)value;
        }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public TransferType Type
        {
            get => (TransferType)TypeInt;
            set => TypeInt = (int)value;
        }
    }

    public partial class Bin
    {
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public BinStatus Status
        {
            get => (BinStatus)StatusInt;
            set => StatusInt = (int)value;
        }
    }
}
