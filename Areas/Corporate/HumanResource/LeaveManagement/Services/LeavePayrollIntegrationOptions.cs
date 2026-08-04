namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeavePayrollIntegrationOptions
    {
        public bool Enabled { get; set; } = true;
        public bool AutoCreateAttendanceInputs { get; set; } = true;
        public bool AutoCreateLeaveAllowanceVariableInputs { get; set; } = false;
        public bool AutoCreateEncashmentVariableInputs { get; set; } = true;
        public bool SubmitVariableInputs { get; set; } = false;
        public string? LeaveAllowancePayrollComponentCode { get; set; } = "LEAVE_ALLOWANCE";
        public string? LeaveEncashmentPayrollComponentCode { get; set; } = "LEAVE_ENCASHMENT";
        public string CurrencyCode { get; set; } = "IDR";
        public int MaximumItemPerExecution { get; set; } = 1000;
    }
}
