using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs
{
    public class MandatoryTrainingRuleSummaryResponse
    {
        public int TotalRule { get; set; }
        public int ActiveRule { get; set; }
        public int InactiveRule { get; set; }
        public int CurrentlyEffectiveRule { get; set; }
        public int CredentialingRequiredRule { get; set; }
        public int IndependentPracticeRequiredRule { get; set; }
        public int PassingResultRequiredRule { get; set; }
        public int RecurringRule { get; set; }
    }

    public class MandatoryTrainingRuleResponse
    {
        public Guid Id { get; set; }
        public Guid TrainingCatalogId { get; set; }
        public string TrainingCode { get; set; } = string.Empty;
        public string TrainingName { get; set; } = string.Empty;
        public Guid TrainingCategoryId { get; set; }
        public string TrainingCategoryName { get; set; } = string.Empty;
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public Guid? SpecializationId { get; set; }
        public string? SpecializationName { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public string? EmployeeCategoryName { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public string? EmploymentTypeName { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public int CompletionDueDaysFromJoin { get; set; }
        public int? RecurrenceMonths { get; set; }
        public int GracePeriodDays { get; set; }
        public bool IsRequiredBeforeAssignment { get; set; }
        public bool IsRequiredForCredentialing { get; set; }
        public bool IsRequiredBeforeIndependentPractice { get; set; }
        public bool RequiresPassingResult { get; set; }
        public decimal? MinimumPassingScore { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsCurrentlyEffective { get; set; }
        public int Priority { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int WorkforceTrainingRecordCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class MandatoryTrainingRuleDetailResponse : MandatoryTrainingRuleResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class MandatoryTrainingRuleOptionResponse
    {
        public Guid Id { get; set; }
        public Guid TrainingCatalogId { get; set; }
        public string TrainingName { get; set; } = string.Empty;
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

    public class MandatoryTrainingRuleOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<MandatoryTrainingRuleOptionResponse> Items { get; set; } = new();
    }

    public class MandatoryTrainingRuleFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public MandatoryTrainingRuleDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<MandatoryTrainingRuleStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<MandatoryTrainingRuleSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class MandatoryTrainingRuleDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? TrainingCatalogId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public bool? IsRequiredForCredentialing { get; set; }
        public bool? IsRequiredBeforeIndependentPractice { get; set; }
        public bool? RequiresPassingResult { get; set; }
        public bool? IsCurrentlyEffective { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "priority";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class MandatoryTrainingRuleStringOptionResponse { public string Value { get; set; } = string.Empty; public string Label { get; set; } = string.Empty; }
    public class MandatoryTrainingRuleSortOptionResponse { public string Value { get; set; } = string.Empty; public string Label { get; set; } = string.Empty; }

    public class CreateMandatoryTrainingRuleRequest
    {
        [Required] public Guid TrainingCatalogId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        [Required, MaxLength(200)] public string RuleName { get; set; } = string.Empty;
        [Range(0, int.MaxValue)] public int CompletionDueDaysFromJoin { get; set; }
        [Range(1, int.MaxValue)] public int? RecurrenceMonths { get; set; }
        [Range(0, int.MaxValue)] public int GracePeriodDays { get; set; }
        public bool IsRequiredBeforeAssignment { get; set; }
        public bool IsRequiredForCredentialing { get; set; }
        public bool IsRequiredBeforeIndependentPractice { get; set; }
        public bool RequiresPassingResult { get; set; }
        [Range(typeof(decimal), "0", "100")] public decimal? MinimumPassingScore { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [Range(0, int.MaxValue)] public int Priority { get; set; }
        [MaxLength(1000)] public string? Description { get; set; }
    }

    public class UpdateMandatoryTrainingRuleRequest : CreateMandatoryTrainingRuleRequest { public bool IsActive { get; set; } = true; }
    public class UpdateMandatoryTrainingRuleStatusRequest { public bool IsActive { get; set; } }
    public class MandatoryTrainingRuleCreateResponse
    {
        public Guid Id { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
