namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants
{
    public static class LeavePayrollIntegrationValueConstants
    {
        public static class IntegrationStatus
        {
            public const string Applied = "Applied";
        }

        public static class SourceType
        {
            public const string LeaveAllowance = "LeaveAllowance";
            public const string LeaveEncashment = "LeaveEncashment";
        }

        public static class InputType
        {
            public const string LeaveIntegration = "LeaveIntegration";
        }

        public static class InputStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string Verified = "Verified";
            public const string Processed = "Processed";
            public const string Posted = "Posted";
        }

        public static class ReadinessStatus
        {
            public const string Ready = "Ready";
            public const string AlreadySynchronized = "AlreadySynchronized";
            public const string MissingPayrollRunEmployee = "MissingPayrollRunEmployee";
            public const string MissingPayrollAttendanceInput = "MissingPayrollAttendanceInput";
            public const string PayrollEmployeeFrozen = "PayrollEmployeeFrozen";
            public const string Blocked = "Blocked";
        }

        public static class IssueType
        {
            public const string MissingPayrollRunEmployee = "MissingPayrollRunEmployee";
            public const string MissingPayrollAttendanceInput = "MissingPayrollAttendanceInput";
            public const string LeaveDayMismatch = "LeaveDayMismatch";
            public const string EmployeeAggregateMismatch = "EmployeeAggregateMismatch";
            public const string MissingLeaveAllowanceInput = "MissingLeaveAllowanceInput";
            public const string MissingEncashmentInput = "MissingEncashmentInput";
            public const string VariableInputMismatch = "VariableInputMismatch";
            public const string TerminalVariableInput = "TerminalVariableInput";
        }

        public static readonly string[] TerminalPayrollRunStatuses =
        {
            "Submitted", "Approved", "Finalized", "Posted", "Paid", "Closed", "Cancelled", "Reversed"
        };

        public static readonly string[] TerminalPayrollPeriodStatuses =
        {
            "Closed", "Finalized", "Posted", "Paid", "Cancelled"
        };

        public static readonly string[] TerminalVariableInputStatuses =
        {
            InputStatus.Verified, InputStatus.Processed, InputStatus.Posted
        };
    }
}
