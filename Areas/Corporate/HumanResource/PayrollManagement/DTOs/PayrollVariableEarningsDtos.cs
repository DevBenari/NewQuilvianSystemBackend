namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class IngestOvertimeAndShiftResultDto
    {
        public Guid RunId { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public int TotalEmployeesProcessed { get; set; }
        public int TotalOvertimeHours { get; set; }
        public decimal TotalOvertimeAmount { get; set; }
        public int TotalNightShifts { get; set; }
        public decimal TotalNightShiftAllowance { get; set; }
        public decimal TotalVariableEarning { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<EmployeeVariableEarningSummaryDto> Details { get; set; } = new();
    }

    public class EmployeeVariableEarningSummaryDto
    {
        public Guid PayrollRunEmployeeId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int OvertimeMinutes { get; set; }
        public decimal OvertimeRegularAmount { get; set; }
        public decimal OvertimeRetrospectiveAmount { get; set; }
        public int NightShiftCount { get; set; }
        public decimal NightShiftAllowance { get; set; }
        public decimal TotalVariableEarning { get; set; }
        public decimal UpdatedGrossPay { get; set; }
    }
}
