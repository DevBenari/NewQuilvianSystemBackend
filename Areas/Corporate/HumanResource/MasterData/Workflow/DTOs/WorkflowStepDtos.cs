using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs
{
    public class WorkflowStepSummaryResponse
    {
        public int TotalWorkflowStep { get; set; }
        public int ActiveWorkflowStep { get; set; }
        public int InactiveWorkflowStep { get; set; }
        public int ApprovalStep { get; set; }
        public int VerificationStep { get; set; }
        public int NotificationStep { get; set; }
        public int ParallelStep { get; set; }
        public int DelegationAllowedStep { get; set; }
    }

    public class WorkflowStepResponse : WorkflowMasterAuditResponse
    {
        public Guid Id { get; set; }
        public Guid WorkflowDefinitionId { get; set; }
        public string? WorkflowCode { get; set; }
        public string? WorkflowName { get; set; }
        public int WorkflowVersion { get; set; }
        public string StepCode { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public string StepType { get; set; } = string.Empty;
        public string ApprovalMode { get; set; } = string.Empty;
        public int RequiredApprovalCount { get; set; }
        public decimal? RequiredApprovalPercentage { get; set; }
        public string ApproverSourceType { get; set; } = string.Empty;
        public Guid? ApproverPositionId { get; set; }
        public Guid? ApproverOrganizationUnitId { get; set; }
        public Guid? SpecificApproverUserId { get; set; }
        public string? SpecificApproverUserName { get; set; }
        public string? ApproverRoleCode { get; set; }
        public int? ManagerLevel { get; set; }
        public bool IsRequired { get; set; }
        public bool IsParallel { get; set; }
        public bool AllowDelegation { get; set; }
        public bool AllowSelfApproval { get; set; }
        public int? ReminderAfterHours { get; set; }
        public int? EscalationAfterHours { get; set; }
        public int? AutoApproveAfterHours { get; set; }
        public int? AutoRejectAfterHours { get; set; }
        public string? OnApproveNextStepCode { get; set; }
        public string? OnRejectStepCode { get; set; }
        public string? Instructions { get; set; }
        public bool IsActive { get; set; }
        public int ApprovalMatrixCount { get; set; }
        public int RejectionReasonCount { get; set; }
        public int DelegationPolicyCount { get; set; }
        public int StepInstanceCount { get; set; }
    }

    public class WorkflowStepDetailResponse : WorkflowStepResponse
    {
    }

    public class WorkflowStepOptionResponse
    {
        public Guid Id { get; set; }
        public Guid WorkflowDefinitionId { get; set; }
        public string? WorkflowCode { get; set; }
        public string StepCode { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public string StepType { get; set; } = string.Empty;
        public string ApprovalMode { get; set; } = string.Empty;
        public string ApproverSourceType { get; set; } = string.Empty;
    }

    public class WorkflowStepOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkflowStepOptionResponse> Items { get; set; } = new();
    }

    public class WorkflowStepFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public WorkflowStepDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkflowMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkflowMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkflowMasterLookupOptionResponse> StepTypes { get; set; } = new();
        public List<WorkflowMasterLookupOptionResponse> ApprovalModes { get; set; } = new();
        public List<WorkflowMasterLookupOptionResponse> ApproverSources { get; set; } = new();
    }

    public class WorkflowStepDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? StepType { get; set; }
        public string? ApprovalMode { get; set; }
        public string? ApproverSourceType { get; set; }
        public bool? IsParallel { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "stepOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateWorkflowStepRequest
    {
        [Required]
        public Guid WorkflowDefinitionId { get; set; }

        [Required, MaxLength(50)]
        public string StepCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string StepName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int StepOrder { get; set; } = 1;

        [Required, MaxLength(50)]
        public string StepType { get; set; } = "Approval";

        [Required, MaxLength(50)]
        public string ApprovalMode { get; set; } = "Any";

        [Range(1, int.MaxValue)]
        public int RequiredApprovalCount { get; set; } = 1;

        [Range(0.01, 100.0)]
        public decimal? RequiredApprovalPercentage { get; set; }

        [Required, MaxLength(50)]
        public string ApproverSourceType { get; set; } = "RequesterManager";

        public Guid? ApproverPositionId { get; set; }
        public Guid? ApproverOrganizationUnitId { get; set; }
        public Guid? SpecificApproverUserId { get; set; }

        [MaxLength(100)]
        public string? ApproverRoleCode { get; set; }

        [Range(1, int.MaxValue)]
        public int? ManagerLevel { get; set; }

        public bool IsRequired { get; set; } = true;
        public bool IsParallel { get; set; }
        public bool AllowDelegation { get; set; } = true;
        public bool AllowSelfApproval { get; set; }

        [Range(0, int.MaxValue)]
        public int? ReminderAfterHours { get; set; }

        [Range(0, int.MaxValue)]
        public int? EscalationAfterHours { get; set; }

        [Range(0, int.MaxValue)]
        public int? AutoApproveAfterHours { get; set; }

        [Range(0, int.MaxValue)]
        public int? AutoRejectAfterHours { get; set; }

        [MaxLength(50)]
        public string? OnApproveNextStepCode { get; set; }

        [MaxLength(50)]
        public string? OnRejectStepCode { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }
    }

    public class UpdateWorkflowStepRequest : CreateWorkflowStepRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class WorkflowStepCreateResponse
    {
        public Guid Id { get; set; }
        public Guid WorkflowDefinitionId { get; set; }
        public string StepCode { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public int StepOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
