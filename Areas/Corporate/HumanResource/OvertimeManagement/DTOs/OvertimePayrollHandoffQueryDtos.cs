using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class OvertimePayrollHandoffQueryRequest
    {
        public string? Search { get; set; }
        public Guid? WorkforceProfileId { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public Guid? PayrollRunId { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public string? HandoffStatus { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool ExcludeCompensatoryLeave { get; set; } = true;
        public string? SortBy { get; set; } = "overtimeDate";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class OvertimePayrollHandoffFilterMetadataResponse
    {
        public List<string> HandoffStatuses { get; set; } = new();
        public List<string> RealizationStatuses { get; set; } = new();
        public List<string> BlockedPayrollRunStatuses { get; set; } = new();
        public List<string> SortFields { get; set; } = new();
        public int DefaultPageSize { get; set; } = 25;
        public int MaximumPageSize { get; set; } = 200;
    }

    public class OvertimePayrollHandoffSummaryResponse
    {
        public int TotalVerifiedRealization { get; set; }
        public int ReadyToPost { get; set; }
        public int PostedToPayroll { get; set; }
        public int ConvertedToCompensatoryLeave { get; set; }
        public int ReconciliationIssue { get; set; }
        public int TotalVerifiedMinutes { get; set; }
        public int TotalPostedMinutes { get; set; }
    }

    public class OvertimePayrollHandoffListResponse
    {
        public Guid OvertimeRealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public int RealizationVersion { get; set; }
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly OvertimeDate { get; set; }
        public int VerifiedMinutes { get; set; }
        public int PostedMinutes { get; set; }
        public string RealizationStatus { get; set; } = string.Empty;
        public string HandoffStatus { get; set; } = string.Empty;
        public bool HasCompensatoryLeave { get; set; }
        public Guid? PayrollOvertimeInputId { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? PayrollRunId { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public DateTime? PostedToPayrollAt { get; set; }
    }

    public class OvertimePayrollHandoffDetailResponse : OvertimePayrollHandoffListResponse
    {
        public int RequestedMinutes { get; set; }
        public int ApprovedMinutes { get; set; }
        public int ActualMinutes { get; set; }
        public int EligibleMinutes { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public string? CalculationSnapshotJson { get; set; }
        public string? PayrollInputNotes { get; set; }
        public DateTime? ImportedAt { get; set; }
        public Guid? ImportedByUserId { get; set; }
        public List<OvertimePayrollRateSnapshotResponse> RateSnapshots { get; set; } = new();
    }

    public class OvertimePayrollHandoffOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string HandoffStatus { get; set; } = string.Empty;
        public DateOnly OvertimeDate { get; set; }
        public int VerifiedMinutes { get; set; }
    }

}
