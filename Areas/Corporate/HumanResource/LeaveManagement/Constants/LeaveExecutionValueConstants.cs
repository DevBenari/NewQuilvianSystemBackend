namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants
{
    public static class LeaveExecutionValueConstants
    {
        public static class ExecutionStatus
        {
            public const string Scheduled = "Scheduled";
            public const string Active = "Active";
            public const string Completed = "Completed";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";
            public const string Reversed = "Reversed";
        }

        public static class AttendanceIntegrationStatus
        {
            public const string Pending = "Pending";
            public const string Applied = "Applied";
            public const string Conflict = "Conflict";
            public const string Failed = "Failed";
            public const string Reversed = "Reversed";
            public const string Skipped = "Skipped";
        }

        public static class MonitoringStatus
        {
            public const string MissingExecution = "MissingExecution";
            public const string Scheduled = "Scheduled";
            public const string StartDue = "StartDue";
            public const string Active = "Active";
            public const string CompletionDue = "CompletionDue";
            public const string Completed = "Completed";
            public const string AttendanceConflict = "AttendanceConflict";
            public const string BalancePending = "BalancePending";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";
            public const string Reversed = "Reversed";
        }

        public static class AttendanceSegment
        {
            public const string Type = "Leave";
            public const string Source = "LeaveRequest";
        }

        public static class BalanceStage
        {
            public const string OnLeaveStart = "OnLeaveStart";
            public const string OnCompletion = "OnCompletion";
            public const string CancellationRestore = "CancellationRestore";
        }
    }
}
