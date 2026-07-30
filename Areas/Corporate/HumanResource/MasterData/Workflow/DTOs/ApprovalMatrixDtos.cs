using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs
{
    public class ApprovalMatrixSummaryResponse
    {
        public int TotalApprovalMatrix { get; set; }
        public int ActiveApprovalMatrix { get; set; }
        public int InactiveApprovalMatrix { get; set; }
        public int FallbackApprovalMatrix { get; set; }
        public int ScopedApprovalMatrix { get; set; }
        public int AmountBasedApprovalMatrix { get; set; }
        public int DurationBasedApprovalMatrix { get; set; }
        public int JsonConditionApprovalMatrix { get; set; }
    }

    public class ApprovalMatrixResponse : WorkflowMasterAuditResponse
    {
        public Guid Id { get; set; }
        public Guid WorkflowDefinitionId { get; set; }
        public string? WorkflowCode { get; set; }
        public string? WorkflowName { get; set; }
        public Guid WorkflowStepId { get; set; }
        public string? WorkflowStepCode { get; set; }
        public string? WorkflowStepName { get; set; }
        public string ApprovalMatrixCode { get; set; } = string.Empty;
        public string ApprovalMatrixName { get; set; } = string.Empty;
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RequesterPositionId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public decimal? MinimumAmount { get; set; }
        public decimal? MaximumAmount { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? MinimumDurationHours { get; set; }
        public decimal? MaximumDurationHours { get; set; }
        public int? MinimumDurationDays { get; set; }
        public int? MaximumDurationDays { get; set; }
        public string ApproverSourceType { get; set; } = string.Empty;
        public Guid? ApproverPositionId { get; set; }
        public Guid? ApproverOrganizationUnitId { get; set; }
        public Guid? SpecificApproverUserId { get; set; }
        public string? SpecificApproverUserName { get; set; }
        public string? ApproverRoleCode { get; set; }
        public int? ManagerLevel { get; set; }
        public int Priority { get; set; }
        public bool IsFallback { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? ConditionDefinitionJson { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int StepInstanceCount { get; set; }
        public int ApproverAssignmentCount { get; set; }
    }

    public class ApprovalMatrixDetailResponse : ApprovalMatrixResponse
    {
    }

    public class ApprovalMatrixOptionResponse
    {
        public Guid Id { get; set; }
        public Guid WorkflowDefinitionId { get; set; }
        public string? WorkflowCode { get; set; }
        public Guid WorkflowStepId { get; set; }
        public string? WorkflowStepCode { get; set; }
        public string ApprovalMatrixCode { get; set; } = string.Empty;
        public string ApprovalMatrixName { get; set; } = string.Empty;
        public string ApproverSourceType { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsFallback { get; set; }
    }

    public class ApprovalMatrixOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<ApprovalMatrixOptionResponse> Items { get; set; } = new();
    }

    public class ApprovalMatrixFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public ApprovalMatrixDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkflowMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkflowMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkflowMasterLookupOptionResponse> ApproverSources { get; set; } = new();
    }

    public class ApprovalMatrixDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowStepId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? ApproverSourceType { get; set; }
        public bool? IsFallback { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "priority";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateApprovalMatrixRequest
    {
        [Required]
        public Guid WorkflowDefinitionId { get; set; }

        [Required]
        public Guid WorkflowStepId { get; set; }

        [Required, MaxLength(50)]
        public string ApprovalMatrixCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ApprovalMatrixName { get; set; } = string.Empty;

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RequesterPositionId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public decimal? MinimumAmount { get; set; }
        public decimal? MaximumAmount { get; set; }

        [MaxLength(10)]
        public string? CurrencyCode { get; set; } = "IDR";

        public decimal? MinimumDurationHours { get; set; }
        public decimal? MaximumDurationHours { get; set; }
        public int? MinimumDurationDays { get; set; }
        public int? MaximumDurationDays { get; set; }

        [Required, MaxLength(50)]
        public string ApproverSourceType { get; set; } = "RequesterManager";

        public Guid? ApproverPositionId { get; set; }
        public Guid? ApproverOrganizationUnitId { get; set; }
        public Guid? SpecificApproverUserId { get; set; }

        [MaxLength(100)]
        public string? ApproverRoleCode { get; set; }

        [Range(1, int.MaxValue)]
        public int? ManagerLevel { get; set; }

        public int Priority { get; set; }
        public bool IsFallback { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? ConditionDefinitionJson { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateApprovalMatrixRequest : CreateApprovalMatrixRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class ApprovalMatrixCreateResponse
    {
        public Guid Id { get; set; }
        public string ApprovalMatrixCode { get; set; } = string.Empty;
        public string ApprovalMatrixName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsActive { get; set; }
    }
}
