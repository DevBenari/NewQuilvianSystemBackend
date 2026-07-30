using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs
{
    public class ApprovalDelegationStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ApprovalDelegationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ApprovalDelegationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Period { get; set; }
        public Guid? DelegatorUserId { get; set; }
        public Guid? DelegateUserId { get; set; }
        public Guid? ApprovalDelegationPolicyId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowStepId { get; set; }
        public string? DelegationStatus { get; set; }
        public bool? AppliesToAllWorkflows { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ApprovalDelegationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public ApprovalDelegationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ApprovalDelegationStringOptionResponse> PeriodOptions { get; set; } = new();
        public List<ApprovalDelegationStringOptionResponse> StatusOptions { get; set; } = new();
        public List<ApprovalDelegationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ApprovalDelegationSummaryResponse
    {
        public int TotalData { get; set; }
        public int Draft { get; set; }
        public int Submitted { get; set; }
        public int Approved { get; set; }
        public int Active { get; set; }
        public int Expired { get; set; }
        public int Rejected { get; set; }
        public int Revoked { get; set; }
        public int Cancelled { get; set; }
        public int DelegatedByCurrentUser { get; set; }
        public int DelegatedToCurrentUser { get; set; }
        public int EffectiveToday { get; set; }
    }

    public class ApprovalDelegationListResponse
    {
        public Guid Id { get; set; }
        public string DelegationNumber { get; set; } = string.Empty;
        public string DelegationStatus { get; set; } = string.Empty;

        public Guid DelegatorUserId { get; set; }
        public Guid? DelegatorWorkforceProfileId { get; set; }
        public string? DelegatorProfileCode { get; set; }
        public string? DelegatorName { get; set; }

        public Guid DelegateUserId { get; set; }
        public Guid? DelegateWorkforceProfileId { get; set; }
        public string? DelegateProfileCode { get; set; }
        public string? DelegateName { get; set; }

        public Guid? ApprovalDelegationPolicyId { get; set; }
        public string? DelegationPolicyCode { get; set; }
        public string? DelegationPolicyName { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }
        public string? WorkflowCode { get; set; }
        public string? WorkflowName { get; set; }

        public Guid? WorkflowStepId { get; set; }
        public string? WorkflowStepCode { get; set; }
        public string? WorkflowStepName { get; set; }

        public DateTime EffectiveStartAt { get; set; }
        public DateTime EffectiveEndAt { get; set; }
        public int DelegationDurationDays { get; set; }
        public bool AppliesToAllWorkflows { get; set; }
        public bool AllowSubDelegation { get; set; }
        public bool PreserveDelegatorAccountability { get; set; }
        public bool IsCurrentlyEffective { get; set; }
        public bool IsActive { get; set; }
        public int AppliedAssignmentCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class ApprovalDelegationDetailResponse : ApprovalDelegationListResponse
    {
        public string? DelegationReason { get; set; }
        public string? ApprovalWorkflowCode { get; set; }
        public bool RequiresManagerApproval { get; set; }
        public bool RequiresHrVerification { get; set; }
        public string? ScopeDefinitionJson { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? RevokedAt { get; set; }
        public Guid? RevokedByUserId { get; set; }
        public string? RevokedByName { get; set; }
        public string? DecisionOrRevocationReason { get; set; }
        public Guid? ApprovalWorkflowInstanceId { get; set; }
        public string? ApprovalWorkflowRequestNumber { get; set; }
        public string? ApprovalWorkflowStatus { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }

    public class CreateApprovalDelegationRequest
    {
        [Required]
        public Guid DelegateUserId { get; set; }

        public Guid? ApprovalDelegationPolicyId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowStepId { get; set; }

        [Required]
        public DateTime EffectiveStartAt { get; set; }

        [Required]
        public DateTime EffectiveEndAt { get; set; }

        [MaxLength(1000)]
        public string? DelegationReason { get; set; }

        public bool AppliesToAllWorkflows { get; set; }
        public bool AllowSubDelegation { get; set; }
        public bool PreserveDelegatorAccountability { get; set; } = true;
        public string? ScopeDefinitionJson { get; set; }
    }

    public class UpdateApprovalDelegationRequest : CreateApprovalDelegationRequest
    {
    }

    public class SubmitApprovalDelegationRequest
    {
        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class ApproveApprovalDelegationRequest
    {
        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class RejectApprovalDelegationRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class RevokeApprovalDelegationRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class CancelApprovalDelegationRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}
