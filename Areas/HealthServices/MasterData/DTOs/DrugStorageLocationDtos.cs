using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DrugStorageLocationSummaryResponse
    {
        public int TotalStorageLocation { get; set; }
        public int ActiveStorageLocation { get; set; }
        public int InactiveStorageLocation { get; set; }
        public int DefaultStorageLocation { get; set; }
        public int MainWarehouseLocation { get; set; }
        public int PharmacyLocation { get; set; }
        public int ColdChainLocation { get; set; }
        public int ControlledDrugStorageLocation { get; set; }
        public int HighAlertStorageLocation { get; set; }
        public int QuarantineLocation { get; set; }
        public int AllowReceivingLocation { get; set; }
        public int AllowDispensingLocation { get; set; }
        public int AllowTransferInLocation { get; set; }
        public int AllowTransferOutLocation { get; set; }
    }

    public class DrugStorageLocationResponse
    {
        public Guid Id { get; set; }

        public Guid? ParentStorageLocationId { get; set; }
        public string? ParentStorageLocationCode { get; set; }
        public string? ParentStorageLocationName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitCode { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }

        public Guid? RoomId { get; set; }
        public string? RoomCode { get; set; }
        public string? MasterRoomName { get; set; }

        public string StorageLocationCode { get; set; } = string.Empty;
        public string StorageLocationName { get; set; } = string.Empty;
        public string StorageLocationType { get; set; } = string.Empty;
        public string? LocationGroupName { get; set; }
        public string? FloorName { get; set; }
        public string? RoomName { get; set; }
        public string? RackCode { get; set; }
        public string? ShelfCode { get; set; }
        public string? BinCode { get; set; }

        public decimal? MinimumTemperatureCelsius { get; set; }
        public decimal? MaximumTemperatureCelsius { get; set; }
        public decimal? MinimumHumidityPercent { get; set; }
        public decimal? MaximumHumidityPercent { get; set; }

        public bool IsDefault { get; set; }
        public bool IsMainWarehouse { get; set; }
        public bool IsPharmacyLocation { get; set; }
        public bool IsColdChain { get; set; }
        public bool IsControlledDrugStorage { get; set; }
        public bool IsHighAlertStorage { get; set; }
        public bool IsQuarantineLocation { get; set; }
        public bool IsAllowReceiving { get; set; }
        public bool IsAllowDispensing { get; set; }
        public bool IsAllowTransferIn { get; set; }
        public bool IsAllowTransferOut { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DrugStorageLocationDetailResponse : DrugStorageLocationResponse
    {
        public string? Description { get; set; }
    }

    public class DrugStorageLocationOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? ParentStorageLocationId { get; set; }
        public string? ParentStorageLocationName { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public string StorageLocationCode { get; set; } = string.Empty;
        public string StorageLocationName { get; set; } = string.Empty;
        public string StorageLocationType { get; set; } = string.Empty;
        public string? LocationGroupName { get; set; }
        public bool IsDefault { get; set; }
        public bool IsMainWarehouse { get; set; }
        public bool IsPharmacyLocation { get; set; }
        public bool IsColdChain { get; set; }
        public bool IsControlledDrugStorage { get; set; }
        public bool IsHighAlertStorage { get; set; }
        public bool IsQuarantineLocation { get; set; }
        public bool IsAllowReceiving { get; set; }
        public bool IsAllowDispensing { get; set; }
        public bool IsAllowTransferIn { get; set; }
        public bool IsAllowTransferOut { get; set; }
    }

    public class DrugStorageLocationOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<DrugStorageLocationOptionResponse> Items { get; set; } = new();
    }

    public class DrugStorageLocationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public DrugStorageLocationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DrugStorageLocationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DrugStorageLocationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> StorageLocationTypeOptions { get; set; } = new();
    }

    public class DrugStorageLocationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? ParentStorageLocationId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DrugStorageLocationCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DrugStorageLocationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateDrugStorageLocationRequest
    {
        public Guid? ParentStorageLocationId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? RoomId { get; set; }

        [Required]
        [MaxLength(150)]
        public string StorageLocationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string StorageLocationType { get; set; } = "General";

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
    }

    public class UpdateDrugStorageLocationRequest : CreateDrugStorageLocationRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class DrugStorageLocationCreateResponse
    {
        public Guid Id { get; set; }
        public string StorageLocationCode { get; set; } = string.Empty;
        public string StorageLocationName { get; set; } = string.Empty;
        public string StorageLocationType { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsMainWarehouse { get; set; }
        public bool IsPharmacyLocation { get; set; }
        public bool IsActive { get; set; }
    }

    public class DrugStorageLocationUpdateResponse : DrugStorageLocationCreateResponse
    {
        public DateTime? UpdateDateTime { get; set; }
    }
}
