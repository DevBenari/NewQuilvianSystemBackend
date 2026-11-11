using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class FinanceReceivableSyncItemDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required, MaxLength(100)]
        public string DeductionCategory { get; set; } = "KasbonPinjamanRS";
        // 'GantiRugiAsetKasir', 'KasbonPinjamanRS', 'CicilanKoperasi', 'IuranPaguyuban'

        [Range(1, 4)]
        public int PriorityLevel { get; set; } = 2;

        [MaxLength(100)]
        public string? ExternalReferenceId { get; set; }

        [Range(0, 1000000000)]
        public decimal TotalLoanPrincipal { get; set; }

        [Range(0, 1000000000)]
        public decimal MonthlyInstallmentAmount { get; set; }

        [Range(0, 1000000000)]
        public decimal RemainingPrincipal { get; set; }

        [Range(1, 120)]
        public int TotalInstallmentMonths { get; set; } = 1;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class FinanceSyncResultDto
    {
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public int TotalSchedulesProcessed { get; set; }
        public int TotalSchedulesCreated { get; set; }
        public int TotalSchedulesUpdated { get; set; }
        public decimal TotalInstallmentAmount { get; set; }
        public DateTime SyncedAt { get; set; }
        public bool IsLocked { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class DeductionScheduleResponseDto
    {
        public Guid Id { get; set; }
        public string ScheduleNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNik { get; set; }
        public string DeductionCategory { get; set; } = string.Empty;
        public int PriorityLevel { get; set; }
        public string? ExternalReferenceId { get; set; }
        public decimal TotalLoanPrincipal { get; set; }
        public decimal MonthlyInstallmentAmount { get; set; }
        public int TotalInstallmentMonths { get; set; }
        public int PaidInstallmentCount { get; set; }
        public decimal RemainingPrincipal { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ScheduleStatus { get; set; } = "Active";
        public bool AutoCarryOverOnCapExceeded { get; set; }
    }

    public class DeductionScheduleFilterDto
    {
        public Guid? EmployeeId { get; set; }
        public string? DeductionCategory { get; set; }
        public int? PriorityLevel { get; set; }
        public string? ScheduleStatus { get; set; }
    }
}
