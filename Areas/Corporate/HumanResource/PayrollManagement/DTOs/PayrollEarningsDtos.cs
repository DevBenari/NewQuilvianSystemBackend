namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class PayrollEarningsCalculationResultDto
    {
        public Guid RunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int TotalEmployeesCalculated { get; set; }
        public decimal TotalBaseSalary { get; set; }
        public decimal TotalRecurringAllowance { get; set; }
        public decimal TotalGrossPay { get; set; }
        public DateTime CalculatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<PayrollEmployeeEarningsResultDto> EmployeeResults { get; set; } = new();
    }

    public class PayrollEmployeeEarningsResultDto
    {
        public Guid PayrollRunEmployeeId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public decimal StructuralAllowance { get; set; }
        public decimal FunctionalAllowance { get; set; }
        public decimal MedicalHazardAllowance { get; set; }
        public decimal TotalRecurringEarning { get; set; }
        public decimal GrossPay { get; set; }
        public List<PayrollComponentSummaryDto> Components { get; set; } = new();
    }

    public class PayrollComponentSummaryDto
    {
        public string ComponentCode { get; set; } = string.Empty;
        public string ComponentName { get; set; } = string.Empty;
        public string ComponentType { get; set; } = "Earning";
        public decimal Amount { get; set; }
        public bool IsTaxable { get; set; }
    }
}
