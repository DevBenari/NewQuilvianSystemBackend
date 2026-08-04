using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs
{
    public class WorkflowDefinitionSummaryResponse
    {
        public int TotalWorkflowDefinition { get; set; }
        public int ActiveWorkflowDefinition { get; set; }
        public int InactiveWorkflowDefinition { get; set; }
        public int DraftWorkflowDefinition { get; set; }
        public int PublishedWorkflowDefinition { get; set; }
        public int RetiredWorkflowDefinition { get; set; }
        public int DefaultWorkflowDefinition { get; set; }
        public int GlobalWorkflowDefinition { get; set; }
        public int ScopedWorkflowDefinition { get; set; }
    }

    public class WorkflowDefinitionResponse : WorkflowMasterAuditResponse
    {
        public Guid Id { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string WorkflowCode { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string WorkflowCategory { get; set; } = string.Empty;
        public int Version { get; set; }
        public string WorkflowStatus { get; set; } = string.Empty;
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool AllowRequesterCancel { get; set; }
        public bool AllowRequesterWithdraw { get; set; }
        public bool AllowParallelApproval { get; set; }
        public bool AllowStepSkip { get; set; }
        public bool StopOnRejection { get; set; }
        public bool IsDefault { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int WorkflowStepCount { get; set; }
        public int ApprovalMatrixCount { get; set; }
        public int RequestReasonCount { get; set; }
        public int RejectionReasonCount { get; set; }
        public int DelegationPolicyCount { get; set; }
        public int WorkflowInstanceCount { get; set; }
    }

    public class WorkflowDefinitionDetailResponse : WorkflowDefinitionResponse
    {
    }

    public class WorkflowDefinitionOptionResponse
    {
        public Guid Id { get; set; }
        public string WorkflowCode { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public int Version { get; set; }
        public string WorkflowStatus { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
    }

    public class WorkflowDefinitionOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkflowDefinitionOptionResponse> Items { get; set; } = new();
    }

    public class WorkflowDefinitionFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public WorkflowDefinitionDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkflowMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkflowMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkflowMasterLookupOptionResponse> WorkflowStatuses { get; set; } = new();
        public List<WorkflowMasterLookupOptionResponse> WorkflowCategories { get; set; } = new();
    }

    public class WorkflowDefinitionDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? RequestType { get; set; }
        public string? WorkflowCategory { get; set; }
        public string? WorkflowStatus { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "workflowName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateWorkflowDefinitionRequest
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }

        [Required, MaxLength(50)]
        public string WorkflowCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string WorkflowName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string RequestType { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string WorkflowCategory { get; set; } = "HumanResource";

        [Range(1, int.MaxValue)]
        public int Version { get; set; } = 1;

        [Required, MaxLength(50)]
        public string WorkflowStatus { get; set; } = "Draft";

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool AllowRequesterCancel { get; set; } = true;
        public bool AllowRequesterWithdraw { get; set; } = true;
        public bool AllowParallelApproval { get; set; }
        public bool AllowStepSkip { get; set; }
        public bool StopOnRejection { get; set; } = true;
        public bool IsDefault { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateWorkflowDefinitionRequest : CreateWorkflowDefinitionRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkflowDefinitionStatusRequest
    {
        [Required, MaxLength(50)]
        public string WorkflowStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class WorkflowDefinitionCreateResponse
    {
        public Guid Id { get; set; }
        public string WorkflowCode { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public int Version { get; set; }
        public string WorkflowStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class WorkflowDefinitionStructureResponse
    {
        public WorkflowDefinitionDetailResponse Definition { get; set; } = new();
        public List<WorkflowStepResponse> Steps { get; set; } = new();
        public List<ApprovalMatrixResponse> ApprovalMatrices { get; set; } = new();
        public List<RequestReasonResponse> RequestReasons { get; set; } = new();
        public List<RejectionReasonResponse> RejectionReasons { get; set; } = new();
        public List<ApprovalDelegationPolicyResponse> DelegationPolicies { get; set; } = new();
    }
}
