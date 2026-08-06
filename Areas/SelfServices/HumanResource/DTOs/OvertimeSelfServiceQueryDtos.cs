namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs
{
    public class MyOvertimeQueryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Status { get; set; }
        public string? RequestSource { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "overtimeDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class MyOvertimeStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MyOvertimeMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ssZ";
        public List<MyOvertimeStringOptionResponse> StatusOptions { get; set; } = new();
        public List<MyOvertimeStringOptionResponse> RequestSourceOptions { get; set; } = new();
        public List<MyOvertimeStringOptionResponse> OvertimeCategoryOptions { get; set; } = new();
        public List<MyOvertimeStringOptionResponse> DayTypeOptions { get; set; } = new();
        public List<MyOvertimeStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class MyOvertimeSummaryResponse
    {
        public int TotalRequest { get; set; }
        public int Draft { get; set; }
        public int Submitted { get; set; }
        public int NeedRevision { get; set; }
        public int ApprovedForWork { get; set; }
        public int Rejected { get; set; }
        public int WaitingRealization { get; set; }
        public int WaitingVerification { get; set; }
        public int Realized { get; set; }
        public int PostedToPayroll { get; set; }
        public int Cancelled { get; set; }
        public int TotalRequestedMinutes { get; set; }
        public int TotalApprovedMinutes { get; set; }
        public int ManagerPlannedRequest { get; set; }
        public int EmployeeSelfServiceRequest { get; set; }
    }

    public class MyOvertimeListResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestSource { get; set; } = string.Empty;
        public DateOnly OvertimeDate { get; set; }
        public DateOnly? PlannedEndDate { get; set; }
        public DateTime? PlannedStartAt { get; set; }
        public DateTime? PlannedEndAt { get; set; }
        public int RequestedMinutes { get; set; }
        public int ApprovedMinutes { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? WorkDescription { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? OvertimePolicyId { get; set; }
        public string? OvertimePolicyCode { get; set; }
        public string? OvertimePolicyName { get; set; }
        public Guid? SourceOvertimePlanDetailId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public bool IsPolicyCompliant { get; set; }
        public bool HasConflict { get; set; }
        public bool CanEdit { get; set; }
        public bool CanSubmit { get; set; }
        public bool CanCancel { get; set; }
        public bool CanDelete { get; set; }
        public bool IsReadOnly { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class MyOvertimeDetailItemResponse
    {
        public Guid Id { get; set; }
        public int SequenceNumber { get; set; }
        public DateOnly OvertimeDate { get; set; }
        public DateTime PlannedStartAt { get; set; }
        public DateTime PlannedEndAt { get; set; }
        public DateTime? ApprovedStartAt { get; set; }
        public DateTime? ApprovedEndAt { get; set; }
        public int RequestedMinutes { get; set; }
        public int ApprovedMinutes { get; set; }
        public int BreakMinutes { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string OvertimeCategory { get; set; } = string.Empty;
        public Guid? OvertimeRateId { get; set; }
        public string? RateCodeSnapshot { get; set; }
        public decimal RateMultiplierSnapshot { get; set; }
        public string WorkDescription { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string DetailStatus { get; set; } = string.Empty;
    }

    public class MyOvertimeDetailResponse : MyOvertimeListResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? CostCenterId { get; set; }
        public string? CostCenterName { get; set; }

        public Guid? WorkScheduleAssignmentId { get; set; }
        public Guid? RosterPeriodId { get; set; }
        public Guid? ShiftAssignmentId { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public string? WorkScheduleCode { get; set; }
        public string? WorkScheduleName { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftCode { get; set; }
        public string? ShiftName { get; set; }

        public Guid? RequestReasonId { get; set; }
        public string? RequestReasonName { get; set; }
        public int EstimatedBreakMinutes { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsBeforeShift { get; set; }
        public bool IsAfterShift { get; set; }
        public bool IsRestDay { get; set; }
        public bool IsHoliday { get; set; }
        public bool HasScheduleConflict { get; set; }
        public bool HasLeaveConflict { get; set; }
        public bool HasTrainingConflict { get; set; }
        public bool HasMinimumRestConflict { get; set; }
        public bool HasWorkHourLimitConflict { get; set; }
        public string? ValidationResultJson { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public int CurrentApprovalStep { get; set; }
        public string? ApprovalNotes { get; set; }
        public List<MyOvertimeDetailItemResponse> Details { get; set; } = new();
    }
}
