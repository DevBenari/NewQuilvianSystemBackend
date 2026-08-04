using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.DTOs
{
    public class RequestReasonSummaryResponse
    {
        public int TotalRequestReason { get; set; }
        public int ActiveRequestReason { get; set; }
        public int InactiveRequestReason { get; set; }
        public int CommentRequiredReason { get; set; }
        public int AttachmentRequiredReason { get; set; }
        public int EmployeeSelectableReason { get; set; }
        public int ManagerSelectableReason { get; set; }
        public int GlobalReason { get; set; }
        public int WorkflowSpecificReason { get; set; }
    }

    public class RequestReasonResponse : WorkflowMasterAuditResponse
    {
        public Guid Id { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? WorkflowCode { get; set; }
        public string? WorkflowName { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string? ReasonCategory { get; set; }
        public bool IsCommentRequired { get; set; }
        public bool IsAttachmentRequired { get; set; }
        public bool IsEmployeeSelectable { get; set; }
        public bool IsManagerSelectable { get; set; }
        public int SortOrder { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class RequestReasonDetailResponse : RequestReasonResponse
    {
    }

    public class RequestReasonOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string? ReasonCategory { get; set; }
        public bool IsCommentRequired { get; set; }
        public bool IsAttachmentRequired { get; set; }
    }

    public class RequestReasonOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<RequestReasonOptionResponse> Items { get; set; } = new();
    }

    public class RequestReasonFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public RequestReasonDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkflowMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkflowMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class RequestReasonDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string? RequestType { get; set; }
        public string? ReasonCategory { get; set; }
        public bool? IsCommentRequired { get; set; }
        public bool? IsAttachmentRequired { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateRequestReasonRequest
    {
        public Guid? WorkflowDefinitionId { get; set; }

        [Required, MaxLength(100)]
        public string RequestType { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string ReasonCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ReasonName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ReasonCategory { get; set; }

        public bool IsCommentRequired { get; set; }
        public bool IsAttachmentRequired { get; set; }
        public bool IsEmployeeSelectable { get; set; } = true;
        public bool IsManagerSelectable { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateRequestReasonRequest : CreateRequestReasonRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class RequestReasonCreateResponse
    {
        public Guid Id { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
