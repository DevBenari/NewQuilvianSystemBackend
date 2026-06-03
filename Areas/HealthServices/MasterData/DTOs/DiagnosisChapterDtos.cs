using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DiagnosisChapterSummaryResponse
    {
        public int TotalDiagnosisChapter { get; set; }
        public int ActiveDiagnosisChapter { get; set; }
        public int InactiveDiagnosisChapter { get; set; }
        public int HasDiagnosisCodeRangeChapter { get; set; }
        public int WithoutDiagnosisCodeRangeChapter { get; set; }
    }

    public class DiagnosisChapterResponse
    {
        public Guid Id { get; set; }
        public string ChapterCode { get; set; } = string.Empty;
        public string ChapterName { get; set; } = string.Empty;
        public string? DiagnosisCodeRangeStart { get; set; }
        public string? DiagnosisCodeRangeEnd { get; set; }
        public string IcdVersion { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DiagnosisChapterDetailResponse : DiagnosisChapterResponse
    {
        public string? Description { get; set; }
    }

    public class DiagnosisChapterOptionResponse
    {
        public Guid Id { get; set; }
        public string ChapterCode { get; set; } = string.Empty;
        public string ChapterName { get; set; } = string.Empty;
        public string? DiagnosisCodeRangeStart { get; set; }
        public string? DiagnosisCodeRangeEnd { get; set; }
        public string IcdVersion { get; set; } = string.Empty;
    }

    public class DiagnosisChapterFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public DiagnosisChapterDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DiagnosisChapterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DiagnosisChapterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DiagnosisChapterQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<DiagnosisChapterFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<DiagnosisChapterFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class DiagnosisChapterDefaultFilterResponse
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

    public class DiagnosisChapterCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class DiagnosisChapterSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DiagnosisChapterQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class DiagnosisChapterFormFieldMetadataResponse
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

    public class CreateDiagnosisChapterRequest
    {
        [Required]
        [MaxLength(50)]
        public string ChapterCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string ChapterName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? DiagnosisCodeRangeStart { get; set; }

        [MaxLength(50)]
        public string? DiagnosisCodeRangeEnd { get; set; }

        [Required]
        [MaxLength(100)]
        public string IcdVersion { get; set; } = "ICD-10";

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateDiagnosisChapterRequest : CreateDiagnosisChapterRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class DiagnosisChapterCreateResponse
    {
        public Guid Id { get; set; }
        public string ChapterCode { get; set; } = string.Empty;
        public string ChapterName { get; set; } = string.Empty;
        public string? DiagnosisCodeRangeStart { get; set; }
        public string? DiagnosisCodeRangeEnd { get; set; }
        public string IcdVersion { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class DiagnosisChapterUpdateResponse : DiagnosisChapterCreateResponse
    {
    }
}
