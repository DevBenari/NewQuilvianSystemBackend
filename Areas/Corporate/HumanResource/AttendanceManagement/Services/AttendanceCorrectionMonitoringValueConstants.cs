namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public static class AttendanceCorrectionMonitoringValueConstants
    {
        public static class MonitoringStatus
        {
            public const string Draft = "Draft";
            public const string WaitingApproval = "WaitingApproval";
            public const string NeedRevision = "NeedRevision";
            public const string ApprovedPendingApply = "ApprovedPendingApply";
            public const string Applied = "Applied";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
            public const string MissingWorkflow = "MissingWorkflow";
            public const string WorkflowMismatch = "WorkflowMismatch";
            public const string Overdue = "Overdue";
            public const string Stale = "Stale";
            public const string Completed = "Completed";
        }

        public static class DueStatus
        {
            public const string Overdue = "Overdue";
            public const string DueToday = "DueToday";
            public const string Upcoming = "Upcoming";
            public const string Completed = "Completed";
            public const string NoDueDate = "NoDueDate";
        }

        public static class IssueCode
        {
            public const string MissingWorkflow = "MISSING_WORKFLOW";
            public const string WorkflowStatusMismatch = "WORKFLOW_STATUS_MISMATCH";
            public const string AutoApplyPending = "AUTO_APPLY_PENDING";
            public const string StaleWorkflow = "STALE_WORKFLOW";
            public const string OverdueAssignment = "OVERDUE_ASSIGNMENT";
            public const string PayrollBlocking = "PAYROLL_BLOCKING_EXCEPTION";
            public const string AttendanceLocked = "ATTENDANCE_LOCKED";
            public const string PayrollAlreadyProcessed = "PAYROLL_ALREADY_PROCESSED";
            public const string AppliedFlagMismatch = "APPLIED_FLAG_MISMATCH";
        }
    }
}
