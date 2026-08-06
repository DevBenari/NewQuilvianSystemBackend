using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public class ResignationFilterMetadataResponse
    {
        public List<string> RequestStatusOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ResignationSummaryResponse
    {
        public int TotalData { get; set; }
        public int Draft { get; set; }
        public int WaitingApproval { get; set; }
        public int NeedRevision { get; set; }
        public int Approved { get; set; }
        public int HandoffCompleted { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
    }

    public class ResignationListResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime ProposedLastWorkingDate { get; set; }
        public int NoticePeriodDays { get; set; }
        public string ResignationReason { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? EmployeeSeparationId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public bool CanEdit { get; set; }
        public bool CanSubmit { get; set; }
        public bool CanCancel { get; set; }
        public bool CanDelete { get; set; }
    }

    public class ResignationDetailResponse : ResignationListResponse
    {
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? HandoverPlan { get; set; }
        public string? ManagerComment { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? WithdrawnAt { get; set; }
        public string? WithdrawalReason { get; set; }
    }

    public class CreateResignationSelfServiceRequest
    {
        public Guid? RequestReasonId { get; set; }
        [Required] public DateTime ProposedLastWorkingDate { get; set; }
        [Required, MaxLength(2000)] public string ResignationReason { get; set; } = string.Empty;
        [MaxLength(2000)] public string? HandoverPlan { get; set; }
    }

    public class UpdateResignationSelfServiceRequest : CreateResignationSelfServiceRequest
    {
    }

    public class ResignationSubmitRequest
    {
        [MaxLength(4000)] public string? Note { get; set; }
        [MaxLength(30)] public string? SourceChannel { get; set; }
        [MaxLength(100)] public string? RequestCorrelationId { get; set; }
        [MaxLength(100)] public string? IdempotencyKey { get; set; }
    }

    public class ResignationCancelRequest
    {
        [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
        [MaxLength(100)] public string? IdempotencyKey { get; set; }
    }

    public class ResignationHandoffRequest
    {
        public Guid? OffboardingTemplateId { get; set; }
        public Guid? FinalEmploymentStatusId { get; set; }
        public Guid? FinalPayrollPeriodId { get; set; }
        public bool IsEligibleForRehire { get; set; } = true;
        public bool CreateOffboardingChecklist { get; set; } = true;
        [MaxLength(2000)] public string? Notes { get; set; }
    }

    public class ResignationHandoffResponse
    {
        public Guid ResignationRequestId { get; set; }
        public Guid EmployeeSeparationId { get; set; }
        public string SeparationNumber { get; set; } = string.Empty;
        public Guid? OffboardingChecklistId { get; set; }
        public string? OffboardingChecklistNumber { get; set; }
        public int CreatedTaskCount { get; set; }
        public string RequestStatus { get; set; } = string.Empty;
    }

    public class ResignationWorkflowResponse
    {
        public Guid ResignationRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public bool HasWorkflow { get; set; }
        public bool IsSynchronized { get; set; }
        public ResignationDetailResponse? Resignation { get; set; }
        public WorkflowInstanceDetailResponse? Workflow { get; set; }
    }
}
