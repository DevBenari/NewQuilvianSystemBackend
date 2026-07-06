using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DiagnosisEducationRecommendationSummaryResponse
    {
        public int TotalRecommendation { get; set; }
        public int ActiveRecommendation { get; set; }
        public int InactiveRecommendation { get; set; }
        public int GeneralEducation { get; set; }
        public int DietEducation { get; set; }
        public int ActivityEducation { get; set; }
        public int MedicationUseEducation { get; set; }
        public int WarningSignEducation { get; set; }
        public int FollowUpEducation { get; set; }
        public int PreventionEducation { get; set; }
    }

    public class DiagnosisEducationRecommendationResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string EducationType { get; set; } = string.Empty;
        public string EducationTypeName { get; set; } = string.Empty;
        public string EducationTitle { get; set; } = string.Empty;
        public string EducationText { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DiagnosisEducationRecommendationDetailResponse : DiagnosisEducationRecommendationResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DiagnosisEducationRecommendationOptionResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string EducationType { get; set; } = string.Empty;
        public string EducationTypeName { get; set; } = string.Empty;
        public string EducationTitle { get; set; } = string.Empty;
        public string EducationText { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class DiagnosisEducationRecommendationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset";

        public DiagnosisEducationRecommendationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DiagnosisEducationRecommendationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DiagnosisEducationRecommendationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DiagnosisEducationRecommendationStringOptionResponse> EducationTypeOptions { get; set; } = new();
        public List<DiagnosisEducationRecommendationQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<DiagnosisEducationRecommendationFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<DiagnosisEducationRecommendationFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class DiagnosisEducationRecommendationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public Guid? DiagnosisId { get; set; }
        public string? EducationType { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "diagnosisCode";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DiagnosisEducationRecommendationCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class DiagnosisEducationRecommendationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DiagnosisEducationRecommendationStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DiagnosisEducationRecommendationQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class DiagnosisEducationRecommendationFormFieldMetadataResponse
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

    public class CreateDiagnosisEducationRecommendationRequest
    {
        [Required]
        public Guid DiagnosisId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EducationType { get; set; } = "General";

        [Required]
        [MaxLength(250)]
        public string EducationTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string EducationText { get; set; } = string.Empty;
    }

    public class UpdateDiagnosisEducationRecommendationRequest : CreateDiagnosisEducationRecommendationRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDiagnosisEducationRecommendationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteDiagnosisEducationRecommendationRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class DiagnosisEducationRecommendationCreateResponse : DiagnosisEducationRecommendationResponse { }
    public class DiagnosisEducationRecommendationUpdateResponse : DiagnosisEducationRecommendationDetailResponse { }

    public class DiagnosisEducationRecommendationDeleteResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;
        public string EducationTitle { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}
