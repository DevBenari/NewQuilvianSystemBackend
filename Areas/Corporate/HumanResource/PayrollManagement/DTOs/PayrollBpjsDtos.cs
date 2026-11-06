namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class CalculateBpjsResultDto
    {
        public Guid RunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int TotalEmployeesProcessed { get; set; }
        public decimal TotalEmployeeBpjsContribution { get; set; }
        public decimal TotalEmployerBpjsContribution { get; set; }
        public decimal TotalBpjsKesehatan { get; set; }
        public decimal TotalBpjsKetenagakerjaan { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<EmployeeBpjsCalculationDto> Details { get; set; } = new();
    }

    public class EmployeeBpjsCalculationDto
    {
        public Guid PayrollRunEmployeeId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal CalculationBaseWage { get; set; }
        public decimal BpjsKesehatanBaseWage { get; set; }
        public decimal BpjsJpBaseWage { get; set; }
        public decimal BpjsKesehatanEmployee { get; set; }
        public decimal BpjsKesehatanEmployer { get; set; }
        public decimal BpjsJkkEmployer { get; set; }
        public decimal BpjsJkmEmployer { get; set; }
        public decimal BpjsJhtEmployee { get; set; }
        public decimal BpjsJhtEmployer { get; set; }
        public decimal BpjsJpEmployee { get; set; }
        public decimal BpjsJpEmployer { get; set; }
        public decimal TotalEmployeeContribution { get; set; }
        public decimal TotalEmployerContribution { get; set; }
    }
}
