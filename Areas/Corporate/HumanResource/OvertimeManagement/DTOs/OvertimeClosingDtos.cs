using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimePeriodQueryRequest
    {
        public string? Search { get; set; }
        public string? PeriodStatus { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? SortBy { get; set; } = "startDate";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateOvertimePeriodRequest
    {
        [Required, MaxLength(50)]
        public string PeriodCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string PeriodName { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool RequireAttendanceFinal { get; set; } = true;
        public bool RequireVerificationComplete { get; set; } = true;
        public bool RequireSettlementComplete { get; set; } = true;
        public DateTime? ScheduledCloseAt { get; set; }
    }

    public class UpdateOvertimePeriodRequest : CreateOvertimePeriodRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class ValidateOvertimePeriodRequest
    {
        public bool AllowRepair { get; set; } = false;
        public int VerificationOverdueHours { get; set; } = 24;
    }

    public class CloseOvertimePeriodRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public bool AllowRepair { get; set; } = true;
        public bool ForceClose { get; set; } = false;
    }

    public class ReopenOvertimePeriodRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class CancelOvertimePeriodRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class OvertimePeriodFilterMetadataResponse
    {
        public List<string> PeriodStatuses { get; set; } = new();
        public List<string> SortFields { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class OvertimePeriodSummaryResponse
    {
        public int TotalPeriod { get; set; }
        public int OpenPeriod { get; set; }
        public int ClosingPeriod { get; set; }
        public int ClosedPeriod { get; set; }
        public int ReopenedPeriod { get; set; }
        public int ScheduledToClose { get; set; }
        public int PeriodWithBlockingIssue { get; set; }
    }

    public class OvertimePeriodListResponse
    {
        public Guid Id { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid? LegalEntityId { get; set; }
        public string? LegalEntityName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string PeriodStatus { get; set; } = string.Empty;
        public DateTime? ScheduledCloseAt { get; set; }
        public DateTime? LastValidatedAt { get; set; }
        public DateTime? LastReconciledAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int ReopenCount { get; set; }
        public int CloseVersion { get; set; }
        public bool IsActive { get; set; }
    }

    public class OvertimePeriodDetailResponse : OvertimePeriodListResponse
    {
        public bool RequireAttendanceFinal { get; set; }
        public bool RequireVerificationComplete { get; set; }
        public bool RequireSettlementComplete { get; set; }
        public Guid? ClosedByUserId { get; set; }
        public string? ClosedByUserName { get; set; }
        public string? CloseReason { get; set; }
        public DateTime? ReopenedAt { get; set; }
        public Guid? ReopenedByUserId { get; set; }
        public string? ReopenedByUserName { get; set; }
        public string? ReopenReason { get; set; }
        public string? ValidationSnapshotJson { get; set; }
        public string? ReconciliationSnapshotJson { get; set; }
    }

    public class OvertimePeriodOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string PeriodStatus { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }

    public class OvertimePeriodActionResponse
    {
        public Guid OvertimePeriodId { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public int CloseVersion { get; set; }
        public int ReopenCount { get; set; }
        public bool WasForced { get; set; }
        public DateTime ActionAt { get; set; }
        public OvertimeFinalReconciliationResponse? Reconciliation { get; set; }
    }
}
