using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    public class PreviewOvertimePayrollHandoffRequest
    {
        [Required]
        public Guid PayrollRunId { get; set; }

        public Guid? PayrollComponentId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class PostOvertimePayrollHandoffRequest : PreviewOvertimePayrollHandoffRequest
    {
        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }
    }

    public class ReconcileOvertimePayrollHandoffRequest
    {
        public Guid? PayrollRunId { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public bool AllowRepair { get; set; } = false;

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }
    }

    public class RollbackOvertimePayrollHandoffRequest
    {
        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }
    }

    public class OvertimePayrollReadinessIssueResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error";
    }

    public class OvertimePayrollRateSnapshotResponse
    {
        public Guid? OvertimeRateId { get; set; }
        public DateOnly OvertimeDate { get; set; }
        public string DayType { get; set; } = string.Empty;
        public string RateBand { get; set; } = string.Empty;
        public int VerifiedMinutes { get; set; }
        public decimal RateMultiplier { get; set; }
        public decimal HourlyRateSnapshot { get; set; }
    }

    public class OvertimePayrollHandoffPreviewResponse
    {
        public Guid OvertimeRealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public DateOnly OvertimeDate { get; set; }
        public int VerifiedMinutes { get; set; }
        public Guid PayrollRunId { get; set; }
        public string PayrollRunNumber { get; set; } = string.Empty;
        public string PayrollRunStatus { get; set; } = string.Empty;
        public Guid PayrollRunEmployeeId { get; set; }
        public Guid PayrollPeriodId { get; set; }
        public string PayrollPeriodCode { get; set; } = string.Empty;
        public Guid PayrollComponentId { get; set; }
        public string PayrollComponentCode { get; set; } = string.Empty;
        public string PayrollComponentName { get; set; } = string.Empty;
        public decimal WeightedRateMultiplier { get; set; }
        public decimal HourlyRateSnapshot { get; set; }
        public bool HasCompensatoryLeave { get; set; }
        public bool HasExistingPayrollInput { get; set; }
        public Guid? ExistingPayrollInputId { get; set; }
        public bool CanPost { get; set; }
        public List<OvertimePayrollReadinessIssueResponse> Issues { get; set; } = new();
        public List<OvertimePayrollRateSnapshotResponse> RateSnapshots { get; set; } = new();
    }

    public class OvertimePayrollHandoffMutationResponse
    {
        public Guid OvertimeRealizationId { get; set; }
        public string RealizationNumber { get; set; } = string.Empty;
        public Guid OvertimeRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid PayrollRunId { get; set; }
        public Guid PayrollRunEmployeeId { get; set; }
        public Guid PayrollPeriodId { get; set; }
        public Guid PayrollComponentId { get; set; }
        public Guid? PayrollOvertimeInputId { get; set; }
        public int PostedMinutes { get; set; }
        public decimal PostedAmount { get; set; }
        public string RealizationStatus { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public bool IsExisting { get; set; }
        public bool IsRolledBack { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    public class OvertimePayrollHandoffReconciliationResponse
    {
        public Guid OvertimeRealizationId { get; set; }
        public Guid? PayrollOvertimeInputId { get; set; }
        public Guid? PayrollRunId { get; set; }
        public Guid? PayrollRunEmployeeId { get; set; }
        public bool IsConsistent { get; set; }
        public bool WasRepaired { get; set; }
        public List<OvertimePayrollReadinessIssueResponse> Issues { get; set; } = new();
    }
}
