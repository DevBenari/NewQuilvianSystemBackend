using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    /// <summary>
    /// Employee Self Service request contracts for profile change workflow.
    /// </summary>
    public class EmployeeProfileChangeStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class EmployeeProfileChangeAllowedFieldResponse
    {
        public string TargetEntityName { get; set; } = string.Empty;
        public string FieldGroup { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ValueType { get; set; } = string.Empty;
        public bool RequiresVerificationDefault { get; set; } = true;
    }

    public class EmployeeProfileChangeTransitionResponse
    {
        public string FromStatus { get; set; } = string.Empty;
        public List<string> AllowedActions { get; set; } = new();
    }

    public class EmployeeProfileChangeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public EmployeeProfileChangeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<EmployeeProfileChangeStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<EmployeeProfileChangeStringOptionResponse> RequestStatuses { get; set; } = new();
        public List<EmployeeProfileChangeStringOptionResponse> RequestCategories { get; set; } = new();
        public List<EmployeeProfileChangeStringOptionResponse> VerificationStatuses { get; set; } = new();
        public List<EmployeeProfileChangeStringOptionResponse> VerificationTypes { get; set; } = new();
        public List<EmployeeProfileChangeStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<EmployeeProfileChangeAllowedFieldResponse> AllowedFields { get; set; } = new();
        public List<EmployeeProfileChangeTransitionResponse> Transitions { get; set; } = new();
        public int MaximumEvidenceFileSizeMb { get; set; } = 10;
        public List<string> AllowedEvidenceExtensions { get; set; } = new();
    }

    public class EmployeeProfileChangeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Period { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public string? RequestStatus { get; set; }
        public string? RequestCategory { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmployeeProfileChangeSummaryResponse
    {
        public int TotalData { get; set; }
        public int DraftData { get; set; }
        public int SubmittedData { get; set; }
        public int UnderVerificationData { get; set; }
        public int NeedRevisionData { get; set; }
        public int ApprovedData { get; set; }
        public int RejectedData { get; set; }
        public int CancelledData { get; set; }
        public int AppliedData { get; set; }
    }

    public class EmployeeProfileChangeListResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public string RequestCategory { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public string? RequestReasonText { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string? RequestedByUserName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public int CurrentStepOrder { get; set; }
        public int DetailCount { get; set; }
        public int PendingVerificationCount { get; set; }
        public int VerifiedVerificationCount { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class EmployeeProfileChangeDetailResponse
    {
        public Guid Id { get; set; }
        public string FieldGroup { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ValueType { get; set; } = string.Empty;
        public string? TargetEntityName { get; set; }
        public Guid? TargetEntityId { get; set; }
        public bool RequiresVerification { get; set; }
        public string DetailStatus { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string? Description { get; set; }
    }

    public class EmployeeProfileChangeVerificationResponse
    {
        public Guid Id { get; set; }
        public Guid ProfileChangeRequestId { get; set; }
        public Guid? ProfileChangeDetailId { get; set; }
        public string? DetailFieldName { get; set; }
        public string VerificationType { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool IsFinalVerification { get; set; }
        public string? VerificationNote { get; set; }
        public string? EvidenceFilePath { get; set; }
        public string? EvidenceFileName { get; set; }
        public string? EvidenceDownloadUrl { get; set; }
    }

    public class EmployeeProfileChangeResponse : EmployeeProfileChangeListResponse
    {
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByUserName { get; set; }
        public Guid? AppliedByUserId { get; set; }
        public string? AppliedByUserName { get; set; }
        public List<EmployeeProfileChangeDetailResponse> Details { get; set; } = new();
        public List<EmployeeProfileChangeVerificationResponse> Verifications { get; set; } = new();
    }

    public class CreateEmployeeProfileChangeDetailRequest
    {
        [Required, MaxLength(100)]
        public string FieldGroup { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string FieldName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? NewValue { get; set; }

        [MaxLength(50)]
        public string? ValueType { get; set; }

        [Required, MaxLength(150)]
        public string TargetEntityName { get; set; } = string.Empty;

        public Guid? TargetEntityId { get; set; }
        public bool RequiresVerification { get; set; } = true;
        public int SortOrder { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class CreateEmployeeProfileChangeRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? RequestReasonId { get; set; }

        [Required, MaxLength(50)]
        public string RequestCategory { get; set; } = "Profile";

        [MaxLength(500)]
        public string? RequestReasonText { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, MinLength(1)]
        public List<CreateEmployeeProfileChangeDetailRequest> Details { get; set; } = new();
    }

    /// <summary>
    /// Kontrak create khusus Employee Self Service. Workforce profile dan workflow
    /// definition ditentukan backend dari user login serta konfigurasi workflow.
    /// </summary>
    public class CreateEmployeeProfileChangeSelfServiceRequest
    {
        public Guid? RequestReasonId { get; set; }

        [Required, MaxLength(50)]
        public string RequestCategory { get; set; } = "Profile";

        [MaxLength(500)]
        public string? RequestReasonText { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, MinLength(1)]
        public List<CreateEmployeeProfileChangeDetailRequest> Details { get; set; } = new();
    }

    public class UpdateEmployeeProfileChangeRequest
    {
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? RequestReasonId { get; set; }

        [Required, MaxLength(50)]
        public string RequestCategory { get; set; } = "Profile";

        [MaxLength(500)]
        public string? RequestReasonText { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, MinLength(1)]
        public List<CreateEmployeeProfileChangeDetailRequest> Details { get; set; } = new();
    }

    /// <summary>
    /// Kontrak update khusus Employee Self Service. Employee tidak dapat mengganti
    /// workflow definition melalui request body.
    /// </summary>
    public class UpdateEmployeeProfileChangeSelfServiceRequest
    {
        public Guid? RequestReasonId { get; set; }

        [Required, MaxLength(50)]
        public string RequestCategory { get; set; } = "Profile";

        [MaxLength(500)]
        public string? RequestReasonText { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, MinLength(1)]
        public List<CreateEmployeeProfileChangeDetailRequest> Details { get; set; } = new();
    }

    public class EmployeeProfileChangeActionNoteRequest
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class EmployeeProfileChangeRejectRequest
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class EmployeeProfileChangeRevisionRequest
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class EmployeeProfileChangeVerificationDecisionRequest
    {
        [Required, MaxLength(50)]
        public string VerificationStatus { get; set; } = "Verified";

        [MaxLength(500)]
        public string? VerificationNote { get; set; }

        public IFormFile? EvidenceFile { get; set; }
    }

    public class ApplyEmployeeProfileChangeRequest
    {
        public bool EnforceOldValueMatch { get; set; } = true;

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class EmployeeProfileChangeApplyResponse
    {
        public Guid RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public int AppliedDetailCount { get; set; }
        public List<string> AppliedFields { get; set; } = new();
    }

    public class EmployeeProfileChangeServiceResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static EmployeeProfileChangeServiceResult<T> Ok(T? data, string message) =>
            new() { Success = true, StatusCode = StatusCodes.Status200OK, Message = message, Data = data };

        public static EmployeeProfileChangeServiceResult<T> Fail(int statusCode, string message) =>
            new() { Success = false, StatusCode = statusCode, Message = message };
    }
}
