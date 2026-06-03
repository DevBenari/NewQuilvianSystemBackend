using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DrugUnitConversionSummaryResponse
    {
        public int TotalDrugUnitConversion { get; set; }
        public int ActiveDrugUnitConversion { get; set; }
        public int InactiveDrugUnitConversion { get; set; }
        public int DefaultConversion { get; set; }
        public int BidirectionalConversion { get; set; }
        public int PurchaseConversion { get; set; }
        public int StockConversion { get; set; }
        public int DispensingConversion { get; set; }
        public int PrescriptionConversion { get; set; }
        public int CompoundConversion { get; set; }
        public int EffectiveConversion { get; set; }
        public int ExpiredConversion { get; set; }
    }

    public class DrugUnitConversionResponse
    {
        public Guid Id { get; set; }

        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;

        public string ConversionCode { get; set; } = string.Empty;
        public string ConversionName { get; set; } = string.Empty;

        public Guid FromMeasurementId { get; set; }
        public string FromMeasurementCode { get; set; } = string.Empty;
        public string FromMeasurementName { get; set; } = string.Empty;
        public string? FromMeasurementSymbol { get; set; }

        public Guid ToMeasurementId { get; set; }
        public string ToMeasurementCode { get; set; } = string.Empty;
        public string ToMeasurementName { get; set; } = string.Empty;
        public string? ToMeasurementSymbol { get; set; }

        public decimal FromQuantity { get; set; }
        public decimal ToQuantity { get; set; }
        public decimal ConversionFactor { get; set; }
        public string ConversionType { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
        public bool IsBidirectional { get; set; }
        public bool IsForPurchase { get; set; }
        public bool IsForStock { get; set; }
        public bool IsForDispensing { get; set; }
        public bool IsForPrescription { get; set; }
        public bool IsForCompound { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DrugUnitConversionDetailResponse : DrugUnitConversionResponse
    {
        public string? Description { get; set; }
    }

    public class DrugUnitConversionOptionResponse
    {
        public Guid Id { get; set; }

        public Guid DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;

        public string ConversionCode { get; set; } = string.Empty;
        public string ConversionName { get; set; } = string.Empty;

        public Guid FromMeasurementId { get; set; }
        public string FromMeasurementName { get; set; } = string.Empty;
        public string? FromMeasurementSymbol { get; set; }

        public Guid ToMeasurementId { get; set; }
        public string ToMeasurementName { get; set; } = string.Empty;
        public string? ToMeasurementSymbol { get; set; }

        public decimal FromQuantity { get; set; }
        public decimal ToQuantity { get; set; }
        public decimal ConversionFactor { get; set; }
        public string ConversionType { get; set; } = string.Empty;

        public bool IsDefault { get; set; }
        public bool IsBidirectional { get; set; }
        public bool IsForPurchase { get; set; }
        public bool IsForStock { get; set; }
        public bool IsForDispensing { get; set; }
        public bool IsForPrescription { get; set; }
        public bool IsForCompound { get; set; }
    }

    public class DrugUnitConversionFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public DrugUnitConversionDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DrugUnitConversionCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DrugUnitConversionSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> ConversionTypeOptions { get; set; } = new();
    }

    public class DrugUnitConversionDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? DrugId { get; set; }
        public Guid? FromMeasurementId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DrugUnitConversionCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DrugUnitConversionSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateDrugUnitConversionRequest
    {
        [Required]
        public Guid DrugId { get; set; }

        [Required]
        [MaxLength(150)]
        public string ConversionName { get; set; } = string.Empty;

        [Required]
        public Guid FromMeasurementId { get; set; }

        [Required]
        public Guid ToMeasurementId { get; set; }

        public decimal FromQuantity { get; set; } = 1;
        public decimal ToQuantity { get; set; } = 1;
        public decimal ConversionFactor { get; set; } = 1;

        [Required]
        [MaxLength(50)]
        public string ConversionType { get; set; } = "General";

        public bool IsDefault { get; set; } = false;
        public bool IsBidirectional { get; set; } = true;
        public bool IsForPurchase { get; set; } = false;
        public bool IsForStock { get; set; } = true;
        public bool IsForDispensing { get; set; } = true;
        public bool IsForPrescription { get; set; } = true;
        public bool IsForCompound { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateDrugUnitConversionRequest : CreateDrugUnitConversionRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class DrugUnitConversionCreateResponse
    {
        public Guid Id { get; set; }
        public Guid DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public string ConversionCode { get; set; } = string.Empty;
        public string ConversionName { get; set; } = string.Empty;
        public Guid FromMeasurementId { get; set; }
        public string FromMeasurementName { get; set; } = string.Empty;
        public Guid ToMeasurementId { get; set; }
        public string ToMeasurementName { get; set; } = string.Empty;
        public decimal ConversionFactor { get; set; }
        public string ConversionType { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }
}
