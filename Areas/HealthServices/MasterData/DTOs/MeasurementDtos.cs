using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class MeasurementSummaryResponse
    {
        public int TotalMeasurement { get; set; }
        public int ActiveMeasurement { get; set; }
        public int InactiveMeasurement { get; set; }
        public int BaseUnitMeasurement { get; set; }
        public int DecimalAllowedMeasurement { get; set; }
        public int DrugMeasurement { get; set; }
        public int LaboratoryMeasurement { get; set; }
        public int VitalSignMeasurement { get; set; }
        public int GeneralUseMeasurement { get; set; }
    }

    public class MeasurementResponse
    {
        public Guid Id { get; set; }
        public string MeasurementCode { get; set; } = string.Empty;
        public string MeasurementName { get; set; } = string.Empty;
        public string? MeasurementSymbol { get; set; }
        public string MeasurementType { get; set; } = string.Empty;
        public string? MeasurementGroupName { get; set; }
        public bool IsBaseUnit { get; set; }
        public bool IsDecimalAllowed { get; set; }
        public int DecimalPrecision { get; set; }
        public bool IsForDrug { get; set; }
        public bool IsForLaboratory { get; set; }
        public bool IsForVitalSign { get; set; }
        public bool IsForGeneralUse { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class MeasurementDetailResponse : MeasurementResponse
    {
        public string? Description { get; set; }
    }

    public class MeasurementOptionResponse
    {
        public Guid Id { get; set; }
        public string MeasurementCode { get; set; } = string.Empty;
        public string MeasurementName { get; set; } = string.Empty;
        public string? MeasurementSymbol { get; set; }
        public string MeasurementType { get; set; } = string.Empty;
        public string? MeasurementGroupName { get; set; }
        public bool IsBaseUnit { get; set; }
        public bool IsDecimalAllowed { get; set; }
        public int DecimalPrecision { get; set; }
        public bool IsForDrug { get; set; }
        public bool IsForLaboratory { get; set; }
        public bool IsForVitalSign { get; set; }
        public bool IsForGeneralUse { get; set; }
    }

    public class MeasurementOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<MeasurementOptionResponse> Items { get; set; } = new();
    }

    public class MeasurementFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public MeasurementDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<MeasurementCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<MeasurementSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> MeasurementTypeOptions { get; set; } = new();
        public List<MeasurementQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<MeasurementFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<MeasurementFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class MeasurementDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class MeasurementCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class MeasurementSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MeasurementQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class MeasurementFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool IsReadonly { get; set; }
        public string? Placeholder { get; set; }
        public string? Description { get; set; }
    }

    public class CreateMeasurementRequest
    {
        [Required]
        [MaxLength(150)]
        public string MeasurementName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? MeasurementSymbol { get; set; }

        [Required]
        [MaxLength(50)]
        public string MeasurementType { get; set; } = "General";

        [MaxLength(100)]
        public string? MeasurementGroupName { get; set; }

        public bool IsBaseUnit { get; set; } = false;

        public bool IsDecimalAllowed { get; set; } = true;

        [Range(0, 8)]
        public int DecimalPrecision { get; set; } = 2;

        public bool IsForDrug { get; set; } = false;
        public bool IsForLaboratory { get; set; } = false;
        public bool IsForVitalSign { get; set; } = false;
        public bool IsForGeneralUse { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateMeasurementRequest : CreateMeasurementRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class MeasurementCreateResponse
    {
        public Guid Id { get; set; }
        public string MeasurementCode { get; set; } = string.Empty;
        public string MeasurementName { get; set; } = string.Empty;
        public string? MeasurementSymbol { get; set; }
        public string MeasurementType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class MeasurementUpdateResponse : MeasurementCreateResponse
    {
    }
}
