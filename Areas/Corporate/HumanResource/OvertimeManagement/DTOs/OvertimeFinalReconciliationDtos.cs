namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimeReconciliationRequest
    {
        public Guid? OvertimePeriodId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool AllowRepair { get; set; } = false;
        public int VerificationOverdueHours { get; set; } = 24;
    }

    public class OvertimeReconciliationFindingResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public string Message { get; set; } = string.Empty;
        public string ReferenceType { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
        public string? ReferenceNumber { get; set; }
        public bool IsBlocking { get; set; }
        public bool IsRepairable { get; set; }
        public bool WasRepaired { get; set; }
    }

    public class OvertimeFinalReconciliationResponse
    {
        public Guid? OvertimePeriodId { get; set; }
        public string? PeriodCode { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalRequest { get; set; }
        public int OpenPlan { get; set; }
        public int DraftRequest { get; set; }
        public int OpenWorkflowRequest { get; set; }
        public int AttendanceNotFinal { get; set; }
        public int ApprovedPendingRealization { get; set; }
        public int WaitingVerification { get; set; }
        public int VerificationOverdue { get; set; }
        public int NeedRevision { get; set; }
        public int VerifiedAwaitingSettlement { get; set; }
        public int PostedToPayroll { get; set; }
        public int CompensatoryLeave { get; set; }
        public int ExpiredCompensatoryPending { get; set; }
        public int PayrollReconciliationIssue { get; set; }
        public int CompensatoryReconciliationIssue { get; set; }
        public int WorkflowLifecycleIssue { get; set; }
        public int RepairAttempted { get; set; }
        public int RepairSucceeded { get; set; }
        public int BlockingCount { get; set; }
        public int WarningCount { get; set; }
        public bool IsCloseReady { get; set; }
        public DateTime EvaluatedAt { get; set; }
        public List<OvertimeReconciliationFindingResponse> Findings { get; set; } = new();
    }

    public class OvertimeCompensatoryExpiryResponse
    {
        public DateOnly AsOfDate { get; set; }
        public int CandidateCount { get; set; }
        public int ExpiredCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public int ExpiredMinutes { get; set; }
        public decimal ExpiredDays { get; set; }
        public List<string> Messages { get; set; } = new();
    }
}
