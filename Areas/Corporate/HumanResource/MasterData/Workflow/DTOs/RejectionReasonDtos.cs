using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs
{
    public class RejectionReasonSummaryResponse
    {
        public int TotalRejectionReason { get; set; }
        public int ActiveRejectionReason { get; set; }
        public int InactiveRejectionReason { get; set; }
        public int CommentRequiredReason { get; set; }
        public int AttachmentRequiredReason { get; set; }
        public int ResubmittableReason { get; set; }
        public int GlobalReason { get; set; }
        public int WorkflowSpecificReason { get; set; }
        public int StepSpecificReason { get; set; }
    }

    public class RejectionReasonResponse : WorkflowMasterAuditResponse
    {
        public Guid Id { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? WorkflowCode { get; set; }
        public string? WorkflowName { get; set; }
        public Guid? WorkflowStepId { get; set; }
        public string? WorkflowStepCode { get; set; }
        public string? WorkflowStepName { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string? ReasonCategory { get; set; }
        public string RejectAction { get; set; } = string.Empty;
        public string? ReturnToStepCode { get; set; }
        public bool IsCommentRequired { get; set; }
        public bool IsAttachmentRequired { get; set; }
        public bool AllowResubmit { get; set; }
        public int SortOrder { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class RejectionReasonDetailResponse : RejectionReasonResponse
    {
    }

    public class RejectionReasonOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowStepId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string RejectAction { get; set; } = string.Empty;
        public bool IsCommentRequired { get; set; }
        public bool IsAttachmentRequired { get; set; }
        public bool AllowResubmit { get; set; }
    }

    public class RejectionReasonOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<RejectionReasonOptionResponse> Items { get; set; } = new();
    }

    public class RejectionReasonFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public RejectionReasonDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkflowMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkflowMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkflowMasterLookupOptionResponse> RejectActions { get; set; } = new();
    }

    public class RejectionReasonDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowStepId { get; set; }
        public string? RequestType { get; set; }
        public string? ReasonCategory { get; set; }
        public string? RejectAction { get; set; }
        public bool? AllowResubmit { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateRejectionReasonRequest
    {
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowStepId { get; set; }

        [Required, MaxLength(100)]
        public string RequestType { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ReasonCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ReasonName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReasonCategory { get; set; }

        [Required, MaxLength(50)]
        public string RejectAction { get; set; } = "ReturnToRequester";

        [MaxLength(50)]
        public string? ReturnToStepCode { get; set; }

        public bool IsCommentRequired { get; set; } = true;
        public bool IsAttachmentRequired { get; set; }
        public bool AllowResubmit { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateRejectionReasonRequest : CreateRejectionReasonRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class RejectionReasonCreateResponse
    {
        public Guid Id { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string RejectAction { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
