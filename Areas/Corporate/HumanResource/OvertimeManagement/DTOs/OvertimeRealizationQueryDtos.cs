namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimeRealizationQueryRequest
    {
        public string? Search { get; set; }
        public string? RealizationStatus { get; set; }
        public string? RequestStatus { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool? IsPayrollPosted { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }

    public class OvertimeRealizationFilterMetadataResponse
    {
        public IReadOnlyCollection<string> RealizationStatuses { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> RequestStatuses { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> DayTypes { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> OvertimeCategories { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> SortFields { get; set; } = Array.Empty<string>();
    }

    public class OvertimeRealizationSummaryResponse
    {
        public int TotalRealization { get; set; }
        public int Draft { get; set; }
        public int WaitingVerification { get; set; }
        public int NeedRevision { get; set; }
        public int Verified { get; set; }
        public int Rejected { get; set; }
        public int PostedToPayroll { get; set; }
        public int Cancelled { get; set; }
        public int TotalActualMinutes { get; set; }
        public int TotalBreakMinutes { get; set; }
        public int TotalEligibleMinutes { get; set; }
        public int TotalVerifiedMinutes { get; set; }
    }

    public class OvertimeRealizationListResponse
    {
        public Guid Id { get; set; }
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
        public DateTime? ActualStartAt { get; set; }
        public DateTime? ActualEndAt { get; set; }
        public int RequestedMinutesSnapshot { get; set; }
        public int ApprovedMinutesSnapshot { get; set; }
        public int ActualMinutes { get; set; }
        public int ActualBreakMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int VerifiedMinutes { get; set; }
        public int VarianceMinutes { get; set; }
        public bool IsPayrollPosted { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class OvertimeRealizationOptionResponse
    {
        public Guid Id { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public string RequestNumber { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string RealizationStatus { get; set; } = string.Empty;
        public DateOnly ActualStartDate { get; set; }
        public int EligibleMinutes { get; set; }
    }

    public class OvertimeRealizationDetailItemResponse
    {
        public Guid Id { get; set; }
        public Guid? OvertimeRequestDetailId { get; set; }
        public int SequenceNumber { get; set; }
        public DateOnly OvertimeDate { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public Guid? AttendanceId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public DateTime? AttendanceCheckInAt { get; set; }
        public DateTime? AttendanceCheckOutAt { get; set; }
        public DateTime ActualStartAt { get; set; }
        public DateTime ActualEndAt { get; set; }
        public int ActualMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public int VerifiedMinutes { get; set; }
        public int VarianceFromApprovedMinutes { get; set; }
        public string DayType { get; set; } = string.Empty;
        public Guid? OvertimeRateId { get; set; }
        public string? OvertimeRateCode { get; set; }
        public string? RateBandSnapshot { get; set; }
        public string? CalculationMethodSnapshot { get; set; }
        public decimal RateMultiplierSnapshot { get; set; }
        public decimal? FixedAmountSnapshot { get; set; }
        public string DetailStatus { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class OvertimeRealizationDetailResponse
    {
        public OvertimeRealizationListResponse Header { get; set; } = new();
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string? RealizationNotes { get; set; }
        public string? EvidenceSummaryJson { get; set; }
        public string? CalculationResultJson { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? PostedToPayrollAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public List<OvertimeRealizationDetailItemResponse> Details { get; set; } = new();
    }
}
