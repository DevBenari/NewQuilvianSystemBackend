using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DiagnosisSummaryResponse
    {
        public int TotalDiagnosis { get; set; }
        public int ActiveDiagnosis { get; set; }
        public int InactiveDiagnosis { get; set; }
        public int SelectableDiagnosis { get; set; }
        public int NonSelectableDiagnosis { get; set; }
        public int WithChapterDiagnosis { get; set; }
        public int WithoutChapterDiagnosis { get; set; }
        public int WithParentDiagnosis { get; set; }
        public int ParentDiagnosis { get; set; }
        public int DetailDiagnosis { get; set; }
        public int PrimaryDiagnosisAllowed { get; set; }
        public int SecondaryDiagnosisAllowed { get; set; }
        public int Icd10Diagnosis { get; set; }
        public int Icd9Diagnosis { get; set; }
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
        public string DiagnosisType { get; set; } = string.Empty;
        public string DiagnosisTypeName { get; set; } = string.Empty;
        public string IcdVersion { get; set; } = string.Empty;
        public bool IsSelectableForClinicalUse { get; set; }
        public bool IsPrimaryDiagnosisAllowed { get; set; }
        public bool IsSecondaryDiagnosisAllowed { get; set; }
        public int DrugRecommendationCount { get; set; }
        public int ProcedureRecommendationCount { get; set; }
        public int EducationRecommendationCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DiagnosisDetailResponse : DiagnosisResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
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
        public string DiagnosisType { get; set; } = string.Empty;
        public string DiagnosisTypeName { get; set; } = string.Empty;
        public string IcdVersion { get; set; } = string.Empty;
        public bool IsSelectableForClinicalUse { get; set; }
        public bool IsPrimaryDiagnosisAllowed { get; set; }
        public bool IsSecondaryDiagnosisAllowed { get; set; }
        public bool IsActive { get; set; }
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
        public string ResetButtonLabel { get; set; } = "Reset";

        public DiagnosisDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DiagnosisCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DiagnosisSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DiagnosisStringOptionResponse> DiagnosisTypeOptions { get; set; } = new();
        public List<DiagnosisStringOptionResponse> IcdVersionOptions { get; set; } = new();
        public List<DiagnosisChapterOptionResponse> DiagnosisChapterOptions { get; set; } = new();
        public List<DiagnosisQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<DiagnosisFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<DiagnosisFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class DiagnosisDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public Guid? DiagnosisChapterId { get; set; }
        public Guid? ParentDiagnosisId { get; set; }
        public string? DiagnosisType { get; set; }
        public string? IcdVersion { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSelectableForClinicalUse { get; set; }
        public bool? IsPrimaryDiagnosisAllowed { get; set; }
        public bool? IsSecondaryDiagnosisAllowed { get; set; }
        public string SortBy { get; set; } = "diagnosisCode";
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

    public class DiagnosisStringOptionResponse
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
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
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

        [Required]
        [MaxLength(50)]
        public string DiagnosisType { get; set; } = "ICD10";

        [Required]
        [MaxLength(100)]
        public string IcdVersion { get; set; } = "ICD-10";

        public bool IsSelectableForClinicalUse { get; set; } = true;
        public bool IsPrimaryDiagnosisAllowed { get; set; } = true;
        public bool IsSecondaryDiagnosisAllowed { get; set; } = true;
    }

    public class UpdateDiagnosisRequest : CreateDiagnosisRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDiagnosisStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteDiagnosisRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
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
        public bool IsPrimaryDiagnosisAllowed { get; set; }
        public bool IsSecondaryDiagnosisAllowed { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DiagnosisUpdateResponse
    {
        public Guid Id { get; set; }
        public Guid? DiagnosisChapterId { get; set; }
        public Guid? ParentDiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string DiagnosisType { get; set; } = string.Empty;
        public string IcdVersion { get; set; } = string.Empty;
        public bool IsSelectableForClinicalUse { get; set; }
        public bool IsPrimaryDiagnosisAllowed { get; set; }
        public bool IsSecondaryDiagnosisAllowed { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DiagnosisDeleteResponse
    {
        public Guid Id { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}
