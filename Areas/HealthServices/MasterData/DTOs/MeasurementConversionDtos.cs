using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class MeasurementConversionSummaryResponse
    {
        public int TotalMeasurementConversion { get; set; }
        public int ActiveMeasurementConversion { get; set; }
        public int InactiveMeasurementConversion { get; set; }
        public int BidirectionalConversion { get; set; }
        public int OneWayConversion { get; set; }
        public int StandardConversion { get; set; }
        public int NonStandardConversion { get; set; }
    }

    public class MeasurementConversionResponse
    {
        public Guid Id { get; set; }

        public Guid FromMeasurementId { get; set; }
        public string FromMeasurementCode { get; set; } = string.Empty;
        public string FromMeasurementName { get; set; } = string.Empty;
        public string? FromMeasurementSymbol { get; set; }
        public string FromMeasurementType { get; set; } = string.Empty;

        public Guid ToMeasurementId { get; set; }
        public string ToMeasurementCode { get; set; } = string.Empty;
        public string ToMeasurementName { get; set; } = string.Empty;
        public string? ToMeasurementSymbol { get; set; }
        public string ToMeasurementType { get; set; } = string.Empty;

        public decimal ConversionFactor { get; set; }
        public bool IsBidirectional { get; set; }
        public bool IsStandardConversion { get; set; }
        public string? ConversionGroupName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class MeasurementConversionDetailResponse : MeasurementConversionResponse
    {
        public string? FormulaNote { get; set; }
        public string? Description { get; set; }
    }

    public class MeasurementConversionOptionResponse
    {
        public Guid Id { get; set; }

        public Guid FromMeasurementId { get; set; }
        public string FromMeasurementName { get; set; } = string.Empty;
        public string? FromMeasurementSymbol { get; set; }

        public Guid ToMeasurementId { get; set; }
        public string ToMeasurementName { get; set; } = string.Empty;
        public string? ToMeasurementSymbol { get; set; }

        public decimal ConversionFactor { get; set; }
        public bool IsBidirectional { get; set; }
        public bool IsStandardConversion { get; set; }
        public string? ConversionGroupName { get; set; }
    }

    public class MeasurementConversionFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public MeasurementConversionDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<MeasurementConversionCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<MeasurementConversionSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class MeasurementConversionDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? FromMeasurementId { get; set; }
        public Guid? ToMeasurementId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class MeasurementConversionCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MeasurementConversionSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateMeasurementConversionRequest
    {
        [Required]
        public Guid FromMeasurementId { get; set; }

        [Required]
        public Guid ToMeasurementId { get; set; }

        [Required]
        public decimal ConversionFactor { get; set; } = 1;

        public bool IsBidirectional { get; set; } = true;

        public bool IsStandardConversion { get; set; } = true;

        [MaxLength(100)]
        public string? ConversionGroupName { get; set; }

        [MaxLength(250)]
        public string? FormulaNote { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateMeasurementConversionRequest : CreateMeasurementConversionRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class MeasurementConversionCreateResponse
    {
        public Guid Id { get; set; }
        public Guid FromMeasurementId { get; set; }
        public string FromMeasurementName { get; set; } = string.Empty;
        public Guid ToMeasurementId { get; set; }
        public string ToMeasurementName { get; set; } = string.Empty;
        public decimal ConversionFactor { get; set; }
        public bool IsBidirectional { get; set; }
        public bool IsStandardConversion { get; set; }
        public bool IsActive { get; set; }
    }
}
