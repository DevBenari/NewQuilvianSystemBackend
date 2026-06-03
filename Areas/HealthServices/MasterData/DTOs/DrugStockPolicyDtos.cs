using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DrugStockPolicySummaryResponse
    {
        public int TotalDrugStockPolicy { get; set; }
        public int ActiveDrugStockPolicy { get; set; }
        public int InactiveDrugStockPolicy { get; set; }
        public int AutoReorderEnabledPolicy { get; set; }
        public int AllowNegativeStockPolicy { get; set; }
        public int BatchRequiredPolicy { get; set; }
        public int ExpiryDateRequiredPolicy { get; set; }
        public int StockOpnameRequiredPolicy { get; set; }
        public int WithStorageLocationPolicy { get; set; }
        public int WithServiceUnitPolicy { get; set; }
        public int WithClinicPolicy { get; set; }
        public int EffectiveDrugStockPolicy { get; set; }
        public int ExpiredDrugStockPolicy { get; set; }
    }

    public class DrugStockPolicyResponse
    {
        public Guid Id { get; set; }

        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? BrandName { get; set; }

        public Guid? StorageLocationId { get; set; }
        public string? StorageLocationCode { get; set; }
        public string? StorageLocationName { get; set; }
        public string? StorageLocationType { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitCode { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }

        public Guid StockUnitMeasurementId { get; set; }
        public string StockUnitMeasurementCode { get; set; } = string.Empty;
        public string StockUnitMeasurementName { get; set; } = string.Empty;
        public string? StockUnitMeasurementSymbol { get; set; }

        public string StockPolicyCode { get; set; } = string.Empty;
        public string StockPolicyName { get; set; } = string.Empty;

        public decimal MinimumStockQuantity { get; set; }
        public decimal MaximumStockQuantity { get; set; }
        public decimal ReorderPointQuantity { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal SafetyStockQuantity { get; set; }
        public decimal CriticalStockQuantity { get; set; }

        public int LeadTimeDays { get; set; }
        public int ExpiryWarningDays { get; set; }
        public int NearExpiryWarningDays { get; set; }
        public bool IsAutoReorderEnabled { get; set; }
        public bool IsAllowNegativeStock { get; set; }
        public bool IsBatchRequired { get; set; }
        public bool IsExpiryDateRequired { get; set; }
        public bool IsStockOpnameRequired { get; set; }
        public int StockOpnameIntervalDays { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DrugStockPolicyDetailResponse : DrugStockPolicyResponse
    {
        public string? Description { get; set; }
    }

    public class DrugStockPolicyOptionResponse
    {
        public Guid Id { get; set; }

        public Guid DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;

        public Guid? StorageLocationId { get; set; }
        public string? StorageLocationName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public Guid StockUnitMeasurementId { get; set; }
        public string StockUnitMeasurementName { get; set; } = string.Empty;
        public string? StockUnitMeasurementSymbol { get; set; }

        public string StockPolicyCode { get; set; } = string.Empty;
        public string StockPolicyName { get; set; } = string.Empty;

        public decimal MinimumStockQuantity { get; set; }
        public decimal MaximumStockQuantity { get; set; }
        public decimal ReorderPointQuantity { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal SafetyStockQuantity { get; set; }
        public decimal CriticalStockQuantity { get; set; }

        public bool IsAutoReorderEnabled { get; set; }
        public bool IsAllowNegativeStock { get; set; }
        public bool IsBatchRequired { get; set; }
        public bool IsExpiryDateRequired { get; set; }
        public bool IsStockOpnameRequired { get; set; }
    }

    public class DrugStockPolicyFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public DrugStockPolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DrugStockPolicyCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DrugStockPolicySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class DrugStockPolicyDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? DrugId { get; set; }
        public Guid? StorageLocationId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DrugStockPolicyCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DrugStockPolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateDrugStockPolicyRequest
    {
        [Required]
        public Guid DrugId { get; set; }

        public Guid? StorageLocationId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }

        [Required]
        public Guid StockUnitMeasurementId { get; set; }

        [Required]
        [MaxLength(150)]
        public string StockPolicyName { get; set; } = string.Empty;

        public decimal MinimumStockQuantity { get; set; } = 0;
        public decimal MaximumStockQuantity { get; set; } = 0;
        public decimal ReorderPointQuantity { get; set; } = 0;
        public decimal ReorderQuantity { get; set; } = 0;
        public decimal SafetyStockQuantity { get; set; } = 0;
        public decimal CriticalStockQuantity { get; set; } = 0;

        public int LeadTimeDays { get; set; } = 0;
        public int ExpiryWarningDays { get; set; } = 90;
        public int NearExpiryWarningDays { get; set; } = 30;

        public bool IsAutoReorderEnabled { get; set; } = false;
        public bool IsAllowNegativeStock { get; set; } = false;
        public bool IsBatchRequired { get; set; } = true;
        public bool IsExpiryDateRequired { get; set; } = true;
        public bool IsStockOpnameRequired { get; set; } = true;

        public int StockOpnameIntervalDays { get; set; } = 30;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateDrugStockPolicyRequest : CreateDrugStockPolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class DrugStockPolicyCreateResponse
    {
        public Guid Id { get; set; }
        public Guid DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public Guid? StorageLocationId { get; set; }
        public string? StorageLocationName { get; set; }
        public string StockPolicyCode { get; set; } = string.Empty;
        public string StockPolicyName { get; set; } = string.Empty;
        public bool IsAutoReorderEnabled { get; set; }
        public bool IsActive { get; set; }
    }
}
