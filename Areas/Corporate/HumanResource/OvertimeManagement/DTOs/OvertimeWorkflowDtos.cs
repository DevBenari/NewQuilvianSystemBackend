using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class StartOvertimeWorkflowRequest
    {
        [MaxLength(4000)]
        public string? Comment { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        [MaxLength(30)]
        public string SourceChannel { get; set; } = "Web";
    }

    public class SynchronizeOvertimeWorkflowRequest
    {
        public bool AllowAutoApply { get; set; } = true;
    }

    public class OvertimeWorkflowIntegrationResponse
    {
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string PreviousRequestStatus { get; set; } = string.Empty;
        public string CurrentRequestStatus { get; set; } = string.Empty;
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public string? WorkflowRequestNumber { get; set; }
        public string WorkflowStatus { get; set; } = string.Empty;
        public int CurrentStepOrder { get; set; }
        public string? CurrentStepCode { get; set; }
        public bool WorkflowCreated { get; set; }
        public bool WorkflowSubmitted { get; set; }
        public bool LifecycleSynchronized { get; set; }
        public DateTime ActionAt { get; set; }
    }
}
