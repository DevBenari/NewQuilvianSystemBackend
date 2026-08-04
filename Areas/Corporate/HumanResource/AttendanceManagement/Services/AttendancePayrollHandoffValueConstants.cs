namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services
{
    public static class AttendancePayrollHandoffValueConstants
    {
        public static class ReadinessStatus
        {
            public const string Ready = "Ready";
            public const string AlreadyImported = "AlreadyImported";
            public const string MissingPayrollProfile = "MissingPayrollProfile";
            public const string MissingPayrollRunEmployee = "MissingPayrollRunEmployee";
            public const string Unprocessed = "Unprocessed";
            public const string PayrollBlocked = "PayrollBlocked";
            public const string Locked = "Locked";
            public const string Excluded = "Excluded";
            public const string PeriodMismatch = "PeriodMismatch";
            public const string InvalidWorkforce = "InvalidWorkforce";
        }

        public static class HandoffStatus
        {
            public const string Completed = "Completed";
            public const string CompletedWithErrors = "CompletedWithErrors";
            public const string Failed = "Failed";
            public const string RolledBack = "RolledBack";
        }

        public static class ExecutionResultStatus
        {
            public const string Created = "Created";
            public const string Updated = "Updated";
            public const string Idempotent = "Idempotent";
            public const string ValidationFailed = "ValidationFailed";
        }

        public static class ReconciliationIssueType
        {
            public const string MissingInput = "MissingInput";
            public const string ChangedAfterImport = "ChangedAfterImport";
            public const string OrphanInput = "OrphanInput";
            public const string OutsidePeriod = "OutsidePeriod";
        }

        public static class ReasonCode
        {
            public const string MissingWorkforceProfile = "MISSING_WORKFORCE_PROFILE";
            public const string AttendanceNotProcessed = "ATTENDANCE_NOT_PROCESSED";
            public const string AttendanceNotPayrollEligible = "ATTENDANCE_NOT_PAYROLL_ELIGIBLE";
            public const string PayrollInputExcluded = "PAYROLL_INPUT_EXCLUDED";
            public const string PayrollInputBlocked = "PAYROLL_INPUT_BLOCKED";
            public const string PayrollInputNotReady = "PAYROLL_INPUT_NOT_READY";
            public const string MissingPayrollProfile = "MISSING_PAYROLL_PROFILE";
            public const string MissingPayrollRunEmployee = "MISSING_PAYROLL_RUN_EMPLOYEE";
            public const string PayrollBlockingException = "PAYROLL_BLOCKING_EXCEPTION";
            public const string AttendanceLockedByOtherRun = "ATTENDANCE_LOCKED_BY_OTHER_RUN";
            public const string PayrollPeriodMismatch = "PAYROLL_PERIOD_MISMATCH";
            public const string ExistingInputKeyConflict = "EXISTING_INPUT_KEY_CONFLICT";
        }

        public static readonly string[] TerminalPayrollRunStatuses =
        {
            "Approved",
            "Paid",
            "Posted",
            "Closed",
            "Cancelled"
        };

        public static readonly string[] TerminalPayrollPeriodStatuses =
        {
            "Approved",
            "Closed",
            "Posted",
            "Cancelled"
        };
    }
}
