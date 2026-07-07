using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DiagnosisProcedureRecommendationSummaryResponse
    {
        public int TotalRecommendation { get; set; }
        public int ActiveRecommendation { get; set; }
        public int InactiveRecommendation { get; set; }
        public int DraftFromLiteratureRecommendation { get; set; }
        public int DoctorReviewedRecommendation { get; set; }
        public int ActiveForSoapRecommendation { get; set; }

        public int ProcedureRecommendation { get; set; }
        public int LabRecommendation { get; set; }
        public int RadiologyRecommendation { get; set; }
        public int MonitoringRecommendation { get; set; }
        public int ReferralRecommendation { get; set; }
        public int FollowUpRecommendation { get; set; }
    }

    public class DiagnosisProcedureRecommendationResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;

        public Guid? ProcedureId { get; set; }
        public string RecommendationType { get; set; } = string.Empty;
        public string RecommendationTypeName { get; set; } = string.Empty;
        public string RecommendationName { get; set; } = string.Empty;
        public string? InstructionText { get; set; }

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

    public class DiagnosisProcedureRecommendationDetailResponse : DiagnosisProcedureRecommendationResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DiagnosisProcedureRecommendationOptionResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;

        public Guid? ProcedureId { get; set; }
        public string RecommendationType { get; set; } = string.Empty;
        public string RecommendationTypeName { get; set; } = string.Empty;
        public string RecommendationName { get; set; } = string.Empty;
        public string? InstructionText { get; set; }
        public string ReviewStatus { get; set; } = string.Empty;
        public string ReviewStatusName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class DiagnosisProcedureRecommendationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } = "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset";
        public DiagnosisProcedureRecommendationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DiagnosisProcedureRecommendationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<DiagnosisProcedureRecommendationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DiagnosisProcedureRecommendationStringOptionResponse> RecommendationTypeOptions { get; set; } = new();
        public List<DiagnosisProcedureRecommendationStringOptionResponse> ReviewStatusOptions { get; set; } = new();
        public List<DiagnosisProcedureRecommendationStringOptionResponse> SourceTypeOptions { get; set; } = new();
        public List<DiagnosisProcedureRecommendationQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<DiagnosisProcedureRecommendationFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<DiagnosisProcedureRecommendationFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class DiagnosisProcedureRecommendationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public Guid? DiagnosisId { get; set; }

        public Guid? ProcedureId { get; set; }
        public string? RecommendationType { get; set; }
        public string? ReviewStatus { get; set; }
        public string? SourceType { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "diagnosisCode";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DiagnosisProcedureRecommendationCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class DiagnosisProcedureRecommendationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DiagnosisProcedureRecommendationStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DiagnosisProcedureRecommendationQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class DiagnosisProcedureRecommendationFormFieldMetadataResponse
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

    public class CreateDiagnosisProcedureRecommendationRequest
    {

        [Required]
        public Guid DiagnosisId { get; set; }

        public Guid? ProcedureId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RecommendationType { get; set; } = "Procedure";

        [Required]
        [MaxLength(250)]
        public string RecommendationName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? InstructionText { get; set; }


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

    public class UpdateDiagnosisProcedureRecommendationRequest : CreateDiagnosisProcedureRecommendationRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDiagnosisProcedureRecommendationStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class UpdateDiagnosisProcedureRecommendationReviewStatusRequest
    {
        [Required]
        [MaxLength(50)]
        public string ReviewStatus { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Note { get; set; }
    }

    public class DeleteDiagnosisProcedureRecommendationRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class DiagnosisProcedureRecommendationCreateResponse : DiagnosisProcedureRecommendationResponse { }
    public class DiagnosisProcedureRecommendationUpdateResponse : DiagnosisProcedureRecommendationDetailResponse { }

    public class DiagnosisProcedureRecommendationDeleteResponse
    {
        public Guid Id { get; set; }
        public Guid DiagnosisId { get; set; }
        public string DiagnosisCode { get; set; } = string.Empty;
        public string DiagnosisName { get; set; } = string.Empty;

        public Guid? ProcedureId { get; set; }
        public string RecommendationName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}
