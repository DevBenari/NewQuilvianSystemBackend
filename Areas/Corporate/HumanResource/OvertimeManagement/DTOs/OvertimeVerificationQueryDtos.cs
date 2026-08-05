namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimeVerificationQueryRequest
    {
        public string? Search { get; set; }
        public string? VerificationStatus { get; set; }
        public string? VerificationType { get; set; }
        public string? RealizationStatus { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? VerifierUserId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool? HasVariance { get; set; }
        public bool? IsFinalVerification { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }

    public class OvertimeVerificationFilterMetadataResponse
    {
        public IReadOnlyCollection<string> VerificationStatuses { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> VerificationTypes { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> RealizationStatuses { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> SortFields { get; set; } = Array.Empty<string>();
    }

    public class OvertimeVerificationSummaryResponse
    {
        public int TotalQueue { get; set; }
        public int NotStarted { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int NeedRevision { get; set; }
        public int Rejected { get; set; }
        public int Finalized { get; set; }
        public int WithVariance { get; set; }
        public int TotalEligibleMinutes { get; set; }
        public int TotalVerifiedMinutes { get; set; }
        public int TotalAdjustmentMinutes { get; set; }
    }

    public class OvertimeVerificationListResponse
    {
        public Guid OvertimeRealizationId { get; set; }
        public Guid? OvertimeVerificationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public int RealizationVersion { get; set; }
        public string RealizationStatus { get; set; } = string.Empty;
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public DateOnly ActualStartDate { get; set; }
        public DateOnly ActualEndDate { get; set; }
        public int ActualMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int VerifiedMinutes { get; set; }
        public int VarianceMinutes { get; set; }
        public string VerificationType { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public Guid? VerifierUserId { get; set; }
        public Guid? VerifierWorkforceProfileId { get; set; }
        public string? VerifierDisplayName { get; set; }
        public bool HasVariance { get; set; }
        public bool RequiresRevision { get; set; }
        public bool IsFinalVerification { get; set; }
        public DateTime? ActionAt { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class OvertimeVerificationOptionResponse
    {
        public Guid OvertimeRealizationId { get; set; }
        public Guid? OvertimeVerificationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public string RequestNumber { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public int EligibleMinutes { get; set; }
    }

    public class OvertimeVerificationRecordResponse
    {
        public Guid Id { get; set; }
        public int VerificationOrder { get; set; }
        public string VerificationType { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public Guid? WorkflowStepId { get; set; }
        public Guid? VerifierUserId { get; set; }
        public Guid? VerifierWorkforceProfileId { get; set; }
        public string? VerifierDisplayName { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public string? RejectionReasonCode { get; set; }
        public string? RejectionReasonName { get; set; }
        public int SubmittedMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int VerifiedMinutes { get; set; }
        public bool IsAttendanceMatched { get; set; }
        public bool IsPolicyCompliant { get; set; }
        public bool HasVariance { get; set; }
        public bool RequiresRevision { get; set; }
        public bool IsFinalVerification { get; set; }
        public string? VerificationResultJson { get; set; }
        public string? Comments { get; set; }
        public DateTime? ActionAt { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class OvertimeVerificationDetailResponse
    {
        public OvertimeRealizationDetailResponse Realization { get; set; } = new();
        public OvertimeVerificationRecordResponse? CurrentVerification { get; set; }
        public List<OvertimeVerificationRecordResponse> VerificationHistory { get; set; } = new();
        public bool CanStart { get; set; }
        public bool CanApprove { get; set; }
        public bool CanRequestRevision { get; set; }
        public bool CanReject { get; set; }
        public bool IsLocked { get; set; }
    }
}
