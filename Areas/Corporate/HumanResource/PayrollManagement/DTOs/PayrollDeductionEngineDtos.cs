namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class DeductionItemResultDto
    {
        public Guid DeductionScheduleId { get; set; }
        public string DeductionCategory { get; set; } = string.Empty;
        public int PriorityLevel { get; set; }
        public decimal ScheduledAmount { get; set; }
        public decimal ActualDeductedAmount { get; set; }
        public decimal DeferredAmount { get; set; }
        public string ExecutionStatus { get; set; } = string.Empty;
        public string? DeferralReason { get; set; }
    }

    public class DeductionSimulationResultDto
    {
        public Guid EmployeeId { get; set; }
        public decimal GrossPay { get; set; }
        public decimal MaxAllowedDeduction { get; set; } // Hard-cap 50% Gross Pay (DED-B1, PP 36/2021)
        public decimal TotalScheduledDeduction { get; set; }
        public decimal TotalActualDeduction { get; set; }
        public decimal TotalDeferredDeduction { get; set; }
        public bool IsCapped { get; set; }
        public List<DeductionItemResultDto> Items { get; set; } = new();
    }

    public class ProcessPayrollDeductionsRequestDto
    {
        public Guid PayrollRunId { get; set; }
        public Guid EmployeeId { get; set; }
        public decimal GrossPay { get; set; }
    }
}
