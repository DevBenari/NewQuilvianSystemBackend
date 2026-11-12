using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs
{
    // ==========================================
    // BE-HRD-141: Compensatory Time Off (CTO) DTOs
    // ==========================================

    public class GenerateCompensatoryTimeOffResultDto
    {
        public Guid CompensatoryTimeOffId { get; set; }
        public string CreditNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly EarnedDate { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public int SourceOvertimeMinutes { get; set; }
        public decimal GrantedDays { get; set; }
        public decimal RemainingHours { get; set; }
        public string CompensatoryStatus { get; set; } = string.Empty;
        public Guid? LeaveBalanceLedgerId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CompensatoryTimeOffTicketDto
    {
        public Guid Id { get; set; }
        public string CreditNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly EarnedDate { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public bool IsExpiringSoon { get; set; }
        public decimal GrantedDays { get; set; }
        public decimal RemainingHours { get; set; }
        public int SourceOvertimeMinutes { get; set; }
        public string CompensatoryStatus { get; set; } = string.Empty;
        public bool IsExpired { get; set; }
        public string? Notes { get; set; }
    }

    // ==========================================
    // BE-HRD-142: Expiration Job DTOs
    // ==========================================

    public class CompensatoryExpirationRunResultDto
    {
        public int TotalProcessedTickets { get; set; }
        public int ExpiredTicketsCount { get; set; }
        public decimal TotalForfeitedDays { get; set; }
        public decimal TotalForfeitedHours { get; set; }
        public DateTime ExecutedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ==========================================
    // BE-HRD-143: Period Management DTOs
    // ==========================================

    public class OvertimePeriodDto
    {
        public Guid Id { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly CutoffDate { get; set; }
        public string PeriodStatus { get; set; } = string.Empty;
        public decimal TotalCtoGrantedHours { get; set; }
        public bool IsHandoffExecuted { get; set; }
        public int TotalVerifiedRealizations { get; set; }
        public int TotalRetrospectiveAdjustments { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? ReopenedAt { get; set; }
        public int ReopenCount { get; set; }
    }

    public class CloseOvertimePeriodCommand
    {
        [MaxLength(1000)]
        public string? CloseReason { get; set; }

        public bool ForceCloseWithPendingAnomalies { get; set; } = false;
    }

    public class ReopenOvertimePeriodCommand
    {
        [Required(ErrorMessage = "Alasan pembukaan kembali periode wajib diisi.")]
        [MaxLength(1000)]
        public string ReopenReason { get; set; } = string.Empty;
    }

    public class OvertimePeriodOperationResultDto
    {
        public Guid PeriodId { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public int TotalIncludedRealizations { get; set; }
        public int RetrospectiveCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ==========================================
    // BE-HRD-144: Payroll Handoff DTOs
    // ==========================================

    public class OvertimePayrollHandoffPackageDto
    {
        public Guid PeriodId { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public DateOnly CutoffDate { get; set; }
        public DateTime HandoffTimestamp { get; set; }
        public int TotalEmployeesCount { get; set; }
        public int TotalRealizationRecords { get; set; }
        public int RegularOvertimeMinutes { get; set; }
        public int HolidayOvertimeMinutes { get; set; }
        public decimal TotalOvertimeHours { get; set; }
        public int RetrospectiveRecordsCount { get; set; }
        public decimal RetrospectiveHours { get; set; }
        public string HandoffBatchNumber { get; set; } = string.Empty;
        public List<OvertimePayrollEmployeeItemDto> EmployeeSummaries { get; set; } = new();
    }

    public class OvertimePayrollEmployeeItemDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int RegularMinutes { get; set; }
        public int HolidayMinutes { get; set; }
        public int RetrospectiveMinutes { get; set; }
        public decimal TotalHours { get; set; }
        public int RealizationCount { get; set; }
    }

    public class ExecutePayrollHandoffCommand
    {
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
