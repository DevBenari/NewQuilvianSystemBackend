namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class PreconditionGateCheckResponse
    {
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public DateOnly CutoffDate { get; set; }
        public bool CanInitialize { get; set; }
        public bool AttendanceLocked { get; set; }
        public string AttendanceStatus { get; set; } = string.Empty;
        public string AttendanceMessage { get; set; } = string.Empty;
        public bool LeaveLocked { get; set; }
        public string LeaveStatus { get; set; } = string.Empty;
        public string LeaveMessage { get; set; } = string.Empty;
        public bool OvertimeLocked { get; set; }
        public bool OvertimeHandoffExecuted { get; set; }
        public string OvertimeStatus { get; set; } = string.Empty;
        public string OvertimeMessage { get; set; } = string.Empty;
        public List<string> UnresolvedIssues { get; set; } = new();
        public string SummaryMessage { get; set; } = string.Empty;
    }
}
