using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeSelfService.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeSelfService.DTOs
{
    public class EmployeeProfileChangeWorkflowSubmitRequest
    {
        [MaxLength(500)]
        public string? Note { get; set; }

        [MaxLength(30)]
        public string SourceChannel { get; set; } = "Web";

        [MaxLength(100)]
        public string? RequestCorrelationId { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        public List<Guid> SelectedApproverUserIds { get; set; } = new();
    }

    public class EmployeeProfileChangeWorkflowCancelRequest
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    public class EmployeeProfileChangeWorkflowResponse
    {
        public Guid ProfileChangeRequestId { get; set; }

        public string ProfileChangeRequestNumber { get; set; } = string.Empty;

        public string ProfileChangeStatus { get; set; } = string.Empty;

        public bool HasWorkflow { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public string? WorkflowRequestNumber { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        public string? WorkflowCode { get; set; }

        public string? WorkflowName { get; set; }

        public string? WorkflowStatus { get; set; }

        public int CurrentStepOrder { get; set; }

        public string? CurrentStepCode { get; set; }

        public bool IsSynchronized { get; set; }

        public bool IsAutoApplyPending { get; set; }

        public EmployeeProfileChangeResponse? ProfileChange { get; set; }

        public WorkflowInstanceDetailResponse? Workflow { get; set; }
    }

    public class EmployeeProfileChangeWorkflowLinkResponse
    {
        public Guid ProfileChangeRequestId { get; set; }

        public string ProfileChangeRequestNumber { get; set; } = string.Empty;

        public string ProfileChangeStatus { get; set; } = string.Empty;

        public bool HasWorkflow { get; set; }

        public bool IsSynchronized { get; set; }

        public bool IsAutoApplyPending { get; set; }

        public WorkflowInstanceDetailResponse? Workflow { get; set; }
    }

    public class EmployeeProfileChangeWorkflowSynchronizationResponse
    {
        public Guid ProfileChangeRequestId { get; set; }

        public Guid WorkflowInstanceId { get; set; }

        public string PreviousProfileChangeStatus { get; set; } = string.Empty;

        public string CurrentProfileChangeStatus { get; set; } = string.Empty;

        public string WorkflowStatus { get; set; } = string.Empty;

        public bool StatusChanged { get; set; }

        public bool AutoApplyAttempted { get; set; }

        public bool AutoApplySucceeded { get; set; }

        public string? WarningMessage { get; set; }
    }
}
