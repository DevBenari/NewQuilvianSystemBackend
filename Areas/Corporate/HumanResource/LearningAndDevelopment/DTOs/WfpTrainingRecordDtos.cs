using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class WfpTrainingRecordSummaryResponse
    {
        public int TotalTraining { get; set; }
        public int ActiveTraining { get; set; }
        public int InactiveTraining { get; set; }
        public int VerifiedTraining { get; set; }
        public int UnverifiedTraining { get; set; }
        public int MandatoryTraining { get; set; }
        public int ExternalTraining { get; set; }
        public decimal TotalCreditPoint { get; set; }
    }

    public class WfpTrainingRecordResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? TrainingCatalogId { get; set; }
        public string? TrainingCatalogCode { get; set; }
        public string? TrainingCatalogName { get; set; }
        public string? CatalogTrainingType { get; set; }
        public string? CatalogDeliveryMethod { get; set; }
        public decimal? CatalogDurationHours { get; set; }
        public int? CatalogValidityMonths { get; set; }
        public bool? CatalogRequiresAssessment { get; set; }
        public decimal? CatalogMinimumPassingScore { get; set; }
        public bool? CatalogIssuesCertificate { get; set; }
        public Guid? TrainingCategoryId { get; set; }
        public string? TrainingCategoryCode { get; set; }
        public string? TrainingCategoryName { get; set; }
        public Guid? MandatoryTrainingRuleId { get; set; }
        public string? MandatoryTrainingRuleCode { get; set; }
        public string? MandatoryTrainingRuleName { get; set; }
        public Guid? TrainingParticipantId { get; set; }
        public string? RequirementCode { get; set; }
        public string TrainingType { get; set; } = string.Empty;
        public string TrainingName { get; set; } = string.Empty;
        public string? Organizer { get; set; }
        public string? Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CertificateNumber { get; set; }
        public decimal CreditPoint { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public bool HasFile { get; set; }
        public bool IsVerified { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsExternalTraining { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpTrainingRecordDetailResponse : WfpTrainingRecordResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpTrainingRecordFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public WfpTrainingRecordDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpTrainingRecordStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpTrainingRecordStringOptionResponse> TrainingTypeOptions { get; set; } = new();
        public List<WfpTrainingCatalogOptionResponse> TrainingCatalogOptions { get; set; } = new();
        public List<WfpTrainingCategoryOptionResponse> TrainingCategoryOptions { get; set; } = new();
        public List<WfpMandatoryTrainingRuleOptionResponse> MandatoryTrainingRuleOptions { get; set; } = new();
        public List<WfpTrainingRecordSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpTrainingRecordDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? TrainingCatalogId { get; set; }
        public Guid? TrainingCategoryId { get; set; }
        public string? TrainingType { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsMandatory { get; set; }
        public bool? IsExternalTraining { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "startDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpTrainingRecordStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpTrainingRecordSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpTrainingCatalogOptionResponse
    {
        public Guid Id { get; set; }
        public string TrainingCode { get; set; } = string.Empty;
        public string TrainingName { get; set; } = string.Empty;
        public Guid TrainingCategoryId { get; set; }
        public string TrainingCategoryName { get; set; } = string.Empty;
        public string TrainingType { get; set; } = string.Empty;
        public string DeliveryMethod { get; set; } = string.Empty;
        public string? DefaultProviderName { get; set; }
        public decimal DurationHours { get; set; }
        public int? ValidityMonths { get; set; }
        public bool IsMandatory { get; set; }
        public bool RequiresAssessment { get; set; }
        public decimal? MinimumPassingScore { get; set; }
        public bool IssuesCertificate { get; set; }
    }

    public class WfpTrainingCategoryOptionResponse
    {
        public Guid Id { get; set; }
        public string TrainingCategoryCode { get; set; } = string.Empty;
        public string TrainingCategoryName { get; set; } = string.Empty;
        public bool IsMandatoryCategory { get; set; }
    }

    public class WfpMandatoryTrainingRuleOptionResponse
    {
        public Guid Id { get; set; }
        public Guid TrainingCatalogId { get; set; }
        public string TrainingCatalogName { get; set; } = string.Empty;
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public int CompletionDueDaysFromJoin { get; set; }
        public int? RecurrenceMonths { get; set; }
        public int GracePeriodDays { get; set; }
        public bool RequiresPassingResult { get; set; }
        public decimal? MinimumPassingScore { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int Priority { get; set; }
    }

    public class CreateWfpTrainingRecordRequest
    {
        public Guid? TrainingCatalogId { get; set; }
        public Guid? TrainingCategoryId { get; set; }
        public Guid? MandatoryTrainingRuleId { get; set; }
        public Guid? TrainingParticipantId { get; set; }

        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [MaxLength(50)]
        public string? TrainingType { get; set; }

        [MaxLength(250)]
        public string? TrainingName { get; set; }

        [MaxLength(250)]
        public string? Organizer { get; set; }

        [MaxLength(500)]
        public string? Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(150)]
        public string? CertificateNumber { get; set; }

        [Range(typeof(decimal), "0", "999999999999.99")]
        public decimal CreditPoint { get; set; }

        [MaxLength(1000)]
        public string? FilePath { get; set; }

        [MaxLength(150)]
        public string? FileContentType { get; set; }

        public bool IsVerified { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsExternalTraining { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(2000)]
        public string? Description { get; set; }
    }

    public class UpdateWfpTrainingRecordRequest : CreateWfpTrainingRecordRequest { }

    public class UpdateWfpTrainingRecordStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWfpTrainingRecordRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}
