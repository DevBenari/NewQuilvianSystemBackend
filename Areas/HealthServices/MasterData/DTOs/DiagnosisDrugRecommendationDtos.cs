using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DiagnosisDrugRecommendationSummaryResponse
    {
        public int TotalRecommendation { get; set; }
        public int ActiveRecommendation { get; set; }
        public int InactiveRecommendation { get; set; }
        public int DraftFromLiteratureRecommendation { get; set; }
        public int DoctorReviewedRecommendation { get; set; }
        public int ActiveForSoapRecommendation { get; set; }

        public int FirstLineRecommendation { get; set; }
        public int AlternativeRecommendation { get; set; }
        public int SymptomaticRecommendation { get; set; }
        public int SupportiveRecommendation { get; set; }
        public int ConditionalRecommendation { get; set; }
    }

    public class DiagnosisDrugRecommendationResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;

        public Guid DrugId { get; set; }
        public string RecommendationType { get; set; } = string.Empty;
        public string RecommendationTypeName { get; set; } = string.Empty;
        public string? IndicationText { get; set; }
        public string? DoseText { get; set; }
        public string? Route { get; set; }
        public string? Frequency { get; set; }
        public string? DurationText { get; set; }
        public string? CautionNote { get; set; }

        public string ReviewStatus { get; set; } = string.Empty;
        public string ReviewStatusName { get; set; } = string.Empty;
        public string? SourceType { get; set; }
        public string? SourceTypeName { get; set; }
        public string? SourceTitle { get; set; }
        public string? SourceYear { get; set; }
        public string? SourceUrl { get; set; }
        public string? SourceNote { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewedByUserName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }
        public Guid? ActivatedByUserId { get; set; }
        public string? ActivatedByUserName { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public string? ActivationNote { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DiagnosisDrugRecommendationDetailResponse : DiagnosisDrugRecommendationResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DiagnosisDrugRecommendationOptionResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;

        public Guid DrugId { get; set; }
        public string RecommendationType { get; set; } = string.Empty;
        public string RecommendationTypeName { get; set; } = string.Empty;
        public string? IndicationText { get; set; }
        public string? DoseText { get; set; }
        public string? Route { get; set; }
        public string? Frequency { get; set; }
        public string? DurationText { get; set; }
        public string? CautionNote { get; set; }
        public string ReviewStatus { get; set; } = string.Empty;
        public string ReviewStatusName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class DiagnosisDrugRecommendationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } = "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public DiagnosisDrugRecommendationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DiagnosisDrugRecommendationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DiagnosisDrugRecommendationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DiagnosisDrugRecommendationStringOptionResponse> RecommendationTypeOptions { get; set; } = new();
        public List<DiagnosisDrugRecommendationStringOptionResponse> ReviewStatusOptions { get; set; } = new();
        public List<DiagnosisDrugRecommendationStringOptionResponse> SourceTypeOptions { get; set; } = new();
        public List<DiagnosisDrugRecommendationQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<DiagnosisDrugRecommendationFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<DiagnosisDrugRecommendationFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class DiagnosisDrugRecommendationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public Guid? DiagnosisId { get; set; }

        public Guid? DrugId { get; set; }
        public string? RecommendationType { get; set; }
        public string? ReviewStatus { get; set; }
        public string? SourceType { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "diagnosisCode";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DiagnosisDrugRecommendationCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class DiagnosisDrugRecommendationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DiagnosisDrugRecommendationStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DiagnosisDrugRecommendationQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class DiagnosisDrugRecommendationFormFieldMetadataResponse
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

    public class CreateDiagnosisDrugRecommendationRequest
    {

        [Required]
        public Guid DiagnosisId { get; set; }

        [Required]
        public Guid DrugId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecommendationType { get; set; } = "Supportive";

        [MaxLength(500)]
        public string? IndicationText { get; set; }

        [MaxLength(250)]
        public string? DoseText { get; set; }

        [MaxLength(100)]
        public string? Route { get; set; }

        [MaxLength(100)]
        public string? Frequency { get; set; }

        [MaxLength(100)]
        public string? DurationText { get; set; }

        [MaxLength(500)]
        public string? CautionNote { get; set; }


        [MaxLength(50)]
        public string? SourceType { get; set; }

        [MaxLength(300)]
        public string? SourceTitle { get; set; }

        [MaxLength(20)]
        public string? SourceYear { get; set; }

        [MaxLength(1000)]
        public string? SourceUrl { get; set; }

        [MaxLength(1000)]
        public string? SourceNote { get; set; }
    }

    public class UpdateDiagnosisDrugRecommendationRequest : CreateDiagnosisDrugRecommendationRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDiagnosisDrugRecommendationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class UpdateDiagnosisDrugRecommendationReviewStatusRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReviewStatus { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class DeleteDiagnosisDrugRecommendationRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class DiagnosisDrugRecommendationCreateResponse : DiagnosisDrugRecommendationResponse { }
    public class DiagnosisDrugRecommendationUpdateResponse : DiagnosisDrugRecommendationDetailResponse { }

    public class DiagnosisDrugRecommendationDeleteResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;

        public Guid DrugId { get; set; }
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}
