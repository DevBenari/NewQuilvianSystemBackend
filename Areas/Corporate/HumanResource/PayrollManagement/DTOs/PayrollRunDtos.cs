using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class CreatePayrollRunCommand
    {
        [Range(2020, 2100, ErrorMessage = "Tahun periode tidak valid.")]
        public int PeriodYear { get; set; }

        [Range(1, 12, ErrorMessage = "Bulan periode harus antara 1 hingga 12.")]
        public int PeriodMonth { get; set; }

        public string RunType { get; set; } = "Regular"; // Regular, OffCycle, Adjustment

        public bool BypassPreconditions { get; set; } = false;

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class PayrollRunSummaryDto
    {
        public Guid Id { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public string RunType { get; set; } = "Regular";
        public string RunStatus { get; set; } = "Draft";
        public int TotalEmployeeCount { get; set; }
        public int ProcessedEmployeeCount { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal TotalNetPay { get; set; }
        public DateTime? CalculatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PayrollRunDetailDto
    {
        public Guid Id { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public string RunType { get; set; } = "Regular";
        public string RunStatus { get; set; } = "Draft";
        public DateTime PeriodStartDateSnapshot { get; set; }
        public DateTime PeriodEndDateSnapshot { get; set; }
        public DateTime? AttendanceCutoffDateSnapshot { get; set; }
        public DateTime? PaymentDateSnapshot { get; set; }
        public int TotalEmployeeCount { get; set; }
        public int ProcessedEmployeeCount { get; set; }
        public decimal TotalBaseSalary { get; set; }
        public decimal TotalEarning { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalNetPay { get; set; }
        public bool IsLocked { get; set; }
        public string? ConfigurationSnapshotJson { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PayrollRunEmployeeItemDto> Employees { get; set; } = new();
    }

    public class PayrollRunEmployeeItemDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public string PayrollStatus { get; set; } = "Pending";
        public decimal BaseSalary { get; set; }
        public decimal RecurringEarning { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal NetPay { get; set; }
        public bool HasVarianceAlert { get; set; }
    }

    public class PayrollRunOperationResultDto
    {
        public Guid RunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public string RunStatus { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
