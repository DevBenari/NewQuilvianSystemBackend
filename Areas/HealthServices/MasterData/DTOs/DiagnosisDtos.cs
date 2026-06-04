using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DiagnosisSummaryResponse
    {
        public int TotalDiagnosis { get; set; }
        public int ActiveDiagnosis { get; set; }
        public int InactiveDiagnosis { get; set; }
        public int SelectableDiagnosis { get; set; }
        public int BillableDiagnosis { get; set; }
        public int PrimaryDiagnosisAllowed { get; set; }
        public int SecondaryDiagnosisAllowed { get; set; }
        public int ChronicDiseaseDiagnosis { get; set; }
        public int InfectiousDiseaseDiagnosis { get; set; }
        public int ExternalCauseDiagnosis { get; set; }
        public int PregnancyRelatedDiagnosis { get; set; }
        public int MentalHealthRelatedDiagnosis { get; set; }
        public int RareDiseaseDiagnosis { get; set; }
        public int WithChapterDiagnosis { get; set; }
        public int WithParentDiagnosis { get; set; }
    }

    public class DiagnosisResponse
    {
        public Guid Id { get; set; }

        public Guid? DiagnosisChapterId { get; set; }
        public string? ChapterCode { get; set; }
        public string? ChapterName { get; set; }

        public Guid? ParentDiagnosisId { get; set; }
        public string? ParentDiagnosisCode { get; set; }
        public string? ParentDiagnosisName { get; set; }

        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string? DiagnosisNameEnglish { get; set; }
        public string? ShortName { get; set; }
        public string? DiagnosisGroupName { get; set; }
        public string? DiagnosisCategoryName { get; set; }
        public string DiagnosisType { get; set; } = string.Empty;
        public string IcdVersion { get; set; } = string.Empty;

        public bool IsSelectableForClinicalUse { get; set; }
        public bool IsBillable { get; set; }
        public bool IsPrimaryDiagnosisAllowed { get; set; }
        public bool IsSecondaryDiagnosisAllowed { get; set; }
        public bool IsChronicDisease { get; set; }
        public bool IsInfectiousDisease { get; set; }
        public bool IsExternalCause { get; set; }
        public bool IsPregnancyRelated { get; set; }
        public bool IsMentalHealthRelated { get; set; }
        public bool IsRareDisease { get; set; }

        public string? GenderRestriction { get; set; }
        public int? MinimumAgeYear { get; set; }
        public int? MaximumAgeYear { get; set; }
        public string? ExternalDiagnosisCode { get; set; }
        public string? IntegrationCode { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DiagnosisDetailResponse : DiagnosisResponse
    {
        public string? Description { get; set; }
    }

    public class DiagnosisOptionResponse
    {
        public Guid Id { get; set; }

        public Guid? DiagnosisChapterId { get; set; }
        public string? ChapterCode { get; set; }
        public string? ChapterName { get; set; }

        public Guid? ParentDiagnosisId { get; set; }
        public string? ParentDiagnosisCode { get; set; }
        public string? ParentDiagnosisName { get; set; }

        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string? ShortName { get; set; }
        public string? DiagnosisGroupName { get; set; }
        public string? DiagnosisCategoryName { get; set; }
        public string DiagnosisType { get; set; } = string.Empty;
        public string IcdVersion { get; set; } = string.Empty;
        public bool IsSelectableForClinicalUse { get; set; }
        public bool IsBillable { get; set; }
        public bool IsPrimaryDiagnosisAllowed { get; set; }
        public bool IsSecondaryDiagnosisAllowed { get; set; }
        public string? GenderRestriction { get; set; }
        public int? MinimumAgeYear { get; set; }
        public int? MaximumAgeYear { get; set; }
    }

    public class DiagnosisOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<DiagnosisOptionResponse> Items { get; set; } = new();
    }

    public class DiagnosisFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public DiagnosisDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DiagnosisCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DiagnosisSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DiagnosisQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<DiagnosisFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<DiagnosisFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class DiagnosisDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? DiagnosisChapterId { get; set; }
        public Guid? ParentDiagnosisId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DiagnosisCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class DiagnosisSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DiagnosisQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class DiagnosisFormFieldMetadataResponse
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

    public class CreateDiagnosisRequest
    {
        public Guid? DiagnosisChapterId { get; set; }
        public Guid? ParentDiagnosisId { get; set; }

        [Required]
        [MaxLength(50)]
        public string DiagnosisCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string DiagnosisName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DiagnosisNameEnglish { get; set; }

        [MaxLength(200)]
        public string? ShortName { get; set; }

        [MaxLength(300)]
        public string? DiagnosisGroupName { get; set; }

        [MaxLength(300)]
        public string? DiagnosisCategoryName { get; set; }

        [Required]
        [MaxLength(50)]
        public string DiagnosisType { get; set; } = "ICD10";

        [Required]
        [MaxLength(100)]
        public string IcdVersion { get; set; } = "ICD-10";

        public bool IsSelectableForClinicalUse { get; set; } = true;
        public bool IsBillable { get; set; } = true;
        public bool IsPrimaryDiagnosisAllowed { get; set; } = true;
        public bool IsSecondaryDiagnosisAllowed { get; set; } = true;
        public bool IsChronicDisease { get; set; } = false;
        public bool IsInfectiousDisease { get; set; } = false;
        public bool IsExternalCause { get; set; } = false;
        public bool IsPregnancyRelated { get; set; } = false;
        public bool IsMentalHealthRelated { get; set; } = false;
        public bool IsRareDisease { get; set; } = false;

        [MaxLength(50)]
        public string? GenderRestriction { get; set; }

        [Range(0, 150)]
        public int? MinimumAgeYear { get; set; }

        [Range(0, 150)]
        public int? MaximumAgeYear { get; set; }

        [MaxLength(50)]
        public string? ExternalDiagnosisCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateDiagnosisRequest : CreateDiagnosisRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class DiagnosisCreateResponse
    {
        public Guid Id { get; set; }
        public Guid? DiagnosisChapterId { get; set; }
        public Guid? ParentDiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string DiagnosisType { get; set; } = string.Empty;
        public string IcdVersion { get; set; } = string.Empty;
        public bool IsSelectableForClinicalUse { get; set; }
        public bool IsBillable { get; set; }
        public bool IsActive { get; set; }
    }

    public class DiagnosisUpdateResponse : DiagnosisCreateResponse
    {
    }
}
