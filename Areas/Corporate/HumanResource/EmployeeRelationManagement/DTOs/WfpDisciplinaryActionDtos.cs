using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs
{
    public class WfpDisciplinaryActionSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int DraftData { get; set; }
        public int ApprovedData { get; set; }
        public int AcknowledgedData { get; set; }
        public int AppealedData { get; set; }
        public int ConfidentialData { get; set; }
    }

    public class WfpDisciplinaryActionResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid DisciplinaryActionTypeId { get; set; }
        public string? DisciplinaryActionTypeCode { get; set; }
        public string? DisciplinaryActionTypeName { get; set; }
        public Guid? ViolationTypeId { get; set; }
        public string? ViolationTypeName { get; set; }
        public Guid? SanctionTypeId { get; set; }
        public string? SanctionTypeName { get; set; }
        public Guid? EmployeeRelationCaseTypeId { get; set; }
        public string? EmployeeRelationCaseTypeName { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string? ActionLevel { get; set; }
        public DateTime ActionDate { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string ActionStatus { get; set; } = string.Empty;
        public bool IsAcknowledged { get; set; }
        public bool IsAppealed { get; set; }
        public string? AppealStatus { get; set; }
        public bool IsConfidential { get; set; }
        public string AccessClassification { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WfpDisciplinaryActionDetailResponse : WfpDisciplinaryActionResponse
    {
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? DisciplinaryCaseId { get; set; }
        public Guid? DisciplinaryDecisionId { get; set; }
        public Guid? IncidentReportId { get; set; }
        public string? Reason { get; set; }
        public string? DecisionSummary { get; set; }
        public string? ConfidentialNotes { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public bool RequiresEnhancedAudit { get; set; }
        public Guid? IssuedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class WfpDisciplinaryActionFilterMetadataResponse
    {
        public WfpDisciplinaryActionDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpDisciplinaryActionStringOptionResponse> ActionStatusOptions { get; set; } = new();
        public List<WfpDisciplinaryActionStringOptionResponse> AppealStatusOptions { get; set; } = new();
        public List<WfpDisciplinaryActionStringOptionResponse> AccessClassificationOptions { get; set; } = new();
        public List<WfpDisciplinaryActionSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpDisciplinaryActionDefaultFilterResponse
    {
        public Guid? DisciplinaryActionTypeId { get; set; }
        public Guid? ViolationTypeId { get; set; }
        public Guid? SanctionTypeId { get; set; }
        public string? ActionStatus { get; set; }
        public bool? IsAcknowledged { get; set; }
        public bool? IsAppealed { get; set; }
        public bool? IsConfidential { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "actionDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpDisciplinaryActionStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpDisciplinaryActionSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpDisciplinaryActionRequest
    {
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? DisciplinaryCaseId { get; set; }
        public Guid? DisciplinaryDecisionId { get; set; }
        public Guid? IncidentReportId { get; set; }

        [Required]
        public Guid DisciplinaryActionTypeId { get; set; }

        public Guid? ViolationTypeId { get; set; }
        public Guid? SanctionTypeId { get; set; }
        public Guid? EmployeeRelationCaseTypeId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }

        [MaxLength(40)]
        public string? ActionLevel { get; set; }

        public DateTime ActionDate { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(250)]
        public string Subject { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Reason { get; set; }

        [MaxLength(2000)]
        public string? DecisionSummary { get; set; }

        [MaxLength(4000)]
        public string? ConfidentialNotes { get; set; }

        public bool IsConfidential { get; set; } = true;

        [Required]
        [MaxLength(30)]
        public string AccessClassification { get; set; } = "HighlyRestricted";

        public bool RequiresEnhancedAudit { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateWfpDisciplinaryActionRequest : CreateWfpDisciplinaryActionRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpDisciplinaryActionStatusRequest
    {
        [Required]
        [MaxLength(40)]
        public string ActionStatus { get; set; } = string.Empty;

        public DateTime? EffectiveEndDate { get; set; }
    }

    public class AcknowledgeWfpDisciplinaryActionRequest
    {
        public bool IsAcknowledged { get; set; } = true;
    }

    public class AppealWfpDisciplinaryActionRequest
    {
        public bool IsAppealed { get; set; } = true;

        [MaxLength(40)]
        public string? AppealStatus { get; set; }
    }
}
