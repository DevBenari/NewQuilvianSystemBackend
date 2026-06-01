using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDrugStorageLocation", Schema = "public")]
    public class MstDrugStorageLocation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? ParentStorageLocationId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? RoomId { get; set; }

        [Required]
        [MaxLength(50)]
        public string StorageLocationCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string StorageLocationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string StorageLocationType { get; set; } = "General";
        // General, MainWarehouse, Pharmacy, Clinic, Emergency, OperatingRoom, ColdStorage, NarcoticStorage, Quarantine

        [MaxLength(100)]
        public string? LocationGroupName { get; set; }

        [MaxLength(100)]
        public string? FloorName { get; set; }

        [MaxLength(100)]
        public string? RoomName { get; set; }

        [MaxLength(100)]
        public string? RackCode { get; set; }

        [MaxLength(100)]
        public string? ShelfCode { get; set; }

        [MaxLength(100)]
        public string? BinCode { get; set; }

        public decimal? MinimumTemperatureCelsius { get; set; }

        public decimal? MaximumTemperatureCelsius { get; set; }

        public decimal? MinimumHumidityPercent { get; set; }

        public decimal? MaximumHumidityPercent { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsMainWarehouse { get; set; } = false;

        public bool IsPharmacyLocation { get; set; } = true;

        public bool IsColdChain { get; set; } = false;

        public bool IsControlledDrugStorage { get; set; } = false;

        public bool IsHighAlertStorage { get; set; } = false;

        public bool IsQuarantineLocation { get; set; } = false;

        public bool IsAllowReceiving { get; set; } = true;

        public bool IsAllowDispensing { get; set; } = true;

        public bool IsAllowTransferIn { get; set; } = true;

        public bool IsAllowTransferOut { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDrugStorageLocation? ParentStorageLocation { get; set; }

        public ICollection<MstDrugStorageLocation> ChildStorageLocations { get; set; } = new List<MstDrugStorageLocation>();

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstRoom? Room { get; set; }
    }
}