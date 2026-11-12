namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class CalculatePph21ResultDto
    {
        public Guid RunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int TotalEmployeesProcessed { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalPph21Amount { get; set; }
        public decimal TotalNetPay { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<EmployeePph21CalculationDto> Details { get; set; } = new();
    }

    public class EmployeePph21CalculationDto
    {
        public Guid PayrollRunEmployeeId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string PtkpStatus { get; set; } = "TK/0";
        public string TerCategory { get; set; } = "A";
        public decimal GrossPay { get; set; }
        public decimal TerPercentage { get; set; }
        public decimal Pph21Amount { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
    }
}
