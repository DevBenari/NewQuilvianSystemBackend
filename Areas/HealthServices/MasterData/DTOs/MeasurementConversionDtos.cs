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
        public decimal? ReverseConversionFactor { get; set; }
        public bool IsBidirectional { get; set; }
        public bool IsStandardConversion { get; set; }
        public string? ConversionGroupName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class MeasurementConversionDetailResponse : MeasurementConversionResponse
    {
        public string? FormulaNote { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class MeasurementConversionOptionResponse
    {
        public Guid Id { get; set; }

        public Guid FromMeasurementId { get; set; }
        public string FromMeasurementCode { get; set; } = string.Empty;
        public string FromMeasurementName { get; set; } = string.Empty;
        public string? FromMeasurementSymbol { get; set; }

        public Guid ToMeasurementId { get; set; }
        public string ToMeasurementCode { get; set; } = string.Empty;
        public string ToMeasurementName { get; set; } = string.Empty;
        public string? ToMeasurementSymbol { get; set; }

        public decimal ConversionFactor { get; set; }
        public decimal? ReverseConversionFactor { get; set; }
        public bool IsBidirectional { get; set; }
        public bool IsStandardConversion { get; set; }
        public string? ConversionGroupName { get; set; }
        public bool IsActive { get; set; }
    }

    public class MeasurementConversionOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<MeasurementConversionOptionResponse> Items { get; set; } = new();
    }

    public class MeasurementConversionFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset Filter";

        public MeasurementConversionDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<MeasurementConversionCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<MeasurementConversionSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<MeasurementConversionQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<MeasurementConversionFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<MeasurementConversionFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class MeasurementConversionDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? FromMeasurementId { get; set; }
        public Guid? ToMeasurementId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsBidirectional { get; set; }
        public bool? IsStandardConversion { get; set; }
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
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class MeasurementConversionSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MeasurementConversionQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class MeasurementConversionFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool IsReadonly { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string? Placeholder { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateMeasurementConversionRequest
    {
        [Required]
        public Guid FromMeasurementId { get; set; }

        [Required]
        public Guid ToMeasurementId { get; set; }

        [Required]
        [Range(typeof(decimal), "0.0000000001", "999999999999")]
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

    public class UpdateMeasurementConversionStatusRequest
    {
        [Required]
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Reason { get; set; }
    }

    public class DeleteMeasurementConversionRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class MeasurementConversionCreateResponse
    {
        public Guid Id { get; set; }
        public Guid FromMeasurementId { get; set; }
        public string FromMeasurementName { get; set; } = string.Empty;
        public Guid ToMeasurementId { get; set; }
        public string ToMeasurementName { get; set; } = string.Empty;
        public decimal ConversionFactor { get; set; }
        public decimal? ReverseConversionFactor { get; set; }
        public bool IsBidirectional { get; set; }
        public bool IsStandardConversion { get; set; }
        public bool IsActive { get; set; }
    }

    public class MeasurementConversionUpdateResponse : MeasurementConversionCreateResponse
    {
    }

    public class MeasurementConversionDeleteResponse
    {
        public Guid Id { get; set; }
        public Guid FromMeasurementId { get; set; }
        public Guid ToMeasurementId { get; set; }
        public bool IsDelete { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DeleteDateTime { get; set; }
    }
}
