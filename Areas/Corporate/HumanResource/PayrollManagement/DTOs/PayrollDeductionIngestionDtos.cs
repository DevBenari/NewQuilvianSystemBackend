namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class IngestDeductionsResultDto
    {
        public Guid RunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int TotalEmployeesProcessed { get; set; }
        public int TotalLoanInstallmentsDeducted { get; set; }
        public decimal TotalLoanDeductionAmount { get; set; }
        public int TotalRestitutionsDeducted { get; set; }
        public decimal TotalRestitutionDeductionAmount { get; set; }
        public decimal TotalDeductionsAmount { get; set; }
        public int TotalEmployeesCappedByLimit { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<EmployeeDeductionSummaryDto> Details { get; set; } = new();
    }

    public class EmployeeDeductionSummaryDto
    {
        public Guid PayrollRunEmployeeId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal GrossPay { get; set; }
        public decimal MaxAllowedLoanDeduction { get; set; }
        public decimal RequestedLoanDeduction { get; set; }
        public decimal ActualLoanDeduction { get; set; }
        public decimal ActualRestitutionDeduction { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public bool IsCappedByLimit { get; set; }
        public List<PayrollComponentSummaryDto> Deductions { get; set; } = new();
    }
}
