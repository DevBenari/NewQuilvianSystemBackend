namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants
{
    public static class OvertimeValueConstants
    {
        public static class RoundingMethod
        {
            public const string None = "None";
            public const string Up = "Up";
            public const string Down = "Down";
            public const string Nearest = "Nearest";

            public static readonly IReadOnlyCollection<string> All =
                new[] { None, Up, Down, Nearest };
        }

        public static class DayType
        {
            public const string Workday = "Workday";
            public const string RestDay = "RestDay";
            public const string Holiday = "Holiday";
            public const string SpecialHoliday = "SpecialHoliday";

            public static readonly IReadOnlyCollection<string> All =
                new[] { Workday, RestDay, Holiday, SpecialHoliday };
        }

        public static class TimeBand
        {
            public const string AllDay = "AllDay";
            public const string FirstHour = "FirstHour";
            public const string NextHour = "NextHour";
            public const string Night = "Night";
            public const string Custom = "Custom";

            public static readonly IReadOnlyCollection<string> All =
                new[] { AllDay, FirstHour, NextHour, Night, Custom };

            public static bool UsesMinuteRange(string value) =>
                string.Equals(value, FirstHour, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, NextHour, StringComparison.OrdinalIgnoreCase);

            public static bool UsesClockRange(string value) =>
                string.Equals(value, Night, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, Custom, StringComparison.OrdinalIgnoreCase);
        }

        public static class CalculationMethod
        {
            public const string Multiplier = "Multiplier";
            public const string FixedAmount = "FixedAmount";
            public const string HigherOfMultiplierOrFixed = "HigherOfMultiplierOrFixed";

            public static readonly IReadOnlyCollection<string> All =
                new[] { Multiplier, FixedAmount, HigherOfMultiplierOrFixed };
        }

        public static class Workflow
        {
            public const string RequestType = "OvertimeRequest";
            public const string ReferenceType = "OVERTIME_REQUEST";
            public const string ActiveStatus = "Active";
        }

        public static class ResolutionSource
        {
            public const string Specific = "Specific";
            public const string Fallback = "Fallback";
        }

        public static class PlanStatus
        {
            public const string Draft = "Draft";
            public const string Validated = "Validated";
            public const string Published = "Published";
            public const string PartiallyConverted = "PartiallyConverted";
            public const string Converted = "Converted";
            public const string Cancelled = "Cancelled";
            public const string Closed = "Closed";

            public static readonly IReadOnlyCollection<string> All =
                new[]
                {
                    Draft,
                    Validated,
                    Published,
                    PartiallyConverted,
                    Converted,
                    Cancelled,
                    Closed
                };
        }

        public static class PlanDetailStatus
        {
            public const string Draft = "Draft";
            public const string Validated = "Validated";
            public const string Published = "Published";
            public const string RequestGenerated = "RequestGenerated";
            public const string Skipped = "Skipped";
            public const string Cancelled = "Cancelled";

            public static readonly IReadOnlyCollection<string> All =
                new[]
                {
                    Draft,
                    Validated,
                    Published,
                    RequestGenerated,
                    Skipped,
                    Cancelled
                };
        }

        public static class RequestSource
        {
            public const string EmployeeSelfService = "EmployeeSelfService";
            public const string ManagerPlanning = "ManagerPlanning";
            public const string HrAdmin = "HrAdmin";
            public const string Emergency = "Emergency";
            public const string SystemGenerated = "SystemGenerated";

            public static readonly IReadOnlyCollection<string> All =
                new[]
                {
                    EmployeeSelfService,
                    ManagerPlanning,
                    HrAdmin,
                    Emergency,
                    SystemGenerated
                };
        }

        public static class RequestStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string NeedRevision = "NeedRevision";
            public const string ApprovedForWork = "ApprovedForWork";
            public const string Rejected = "Rejected";
            public const string InProgress = "InProgress";
            public const string WaitingRealization = "WaitingRealization";
            public const string WaitingVerification = "WaitingVerification";
            public const string Realized = "Realized";
            public const string PostedToPayroll = "PostedToPayroll";
            public const string Cancelled = "Cancelled";

            public static readonly IReadOnlyCollection<string> All =
                new[]
                {
                    Draft,
                    Submitted,
                    NeedRevision,
                    ApprovedForWork,
                    Rejected,
                    InProgress,
                    WaitingRealization,
                    WaitingVerification,
                    Realized,
                    PostedToPayroll,
                    Cancelled
                };
        }

        public static class OvertimeCategory
        {
            public const string BeforeShift = "BeforeShift";
            public const string AfterShift = "AfterShift";
            public const string RestDay = "RestDay";
            public const string Holiday = "Holiday";
            public const string Emergency = "Emergency";
            public const string OnCall = "OnCall";

            public static readonly IReadOnlyCollection<string> All =
                new[]
                {
                    BeforeShift,
                    AfterShift,
                    RestDay,
                    Holiday,
                    Emergency,
                    OnCall
                };
        }

        public static class RealizationStatus
        {
            public const string Draft = "Draft";
            public const string WaitingVerification = "WaitingVerification";
            public const string NeedRevision = "NeedRevision";
            public const string Verified = "Verified";
            public const string Rejected = "Rejected";
            public const string PostedToPayroll = "PostedToPayroll";
            public const string Cancelled = "Cancelled";

            public static readonly IReadOnlyCollection<string> All =
                new[]
                {
                    Draft,
                    WaitingVerification,
                    NeedRevision,
                    Verified,
                    Rejected,
                    PostedToPayroll,
                    Cancelled
                };
        }

        public static class RealizationDetailStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string NeedRevision = "NeedRevision";
            public const string Verified = "Verified";
            public const string Rejected = "Rejected";
            public const string Posted = "Posted";
            public const string Cancelled = "Cancelled";

            public static readonly IReadOnlyCollection<string> All =
                new[]
                {
                    Draft,
                    Submitted,
                    NeedRevision,
                    Verified,
                    Rejected,
                    Posted,
                    Cancelled
                };
        }

        public static class AttendanceMatchStatus
        {
            public const string Ready = "Ready";
            public const string AttendancePending = "AttendancePending";
            public const string AttendanceNotFound = "AttendanceNotFound";
            public const string IncompleteAttendance = "IncompleteAttendance";
            public const string NoOverlap = "NoOverlap";
            public const string RateNotResolved = "RateNotResolved";
            public const string PolicyBlocked = "PolicyBlocked";
        }

        public static class CalculationTrigger
        {
            public const string Manual = "Manual";
            public const string Recalculate = "Recalculate";
            public const string System = "System";
        }

        public static class VerificationType
        {
            public const string Supervisor = "Supervisor";
            public const string Manager = "Manager";
            public const string HR = "HR";
            public const string Payroll = "Payroll";

            public static readonly IReadOnlyCollection<string> All =
                new[] { Supervisor, Manager, HR, Payroll };
        }

        public static class VerificationStatus
        {
            public const string NotStarted = "NotStarted";
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string NeedRevision = "NeedRevision";
            public const string Skipped = "Skipped";

            public static readonly IReadOnlyCollection<string> All =
                new[] { Pending, Approved, Rejected, NeedRevision, Skipped };

            public static readonly IReadOnlyCollection<string> FilterAll =
                new[] { NotStarted, Pending, Approved, Rejected, NeedRevision, Skipped };
        }

        public static class VerificationAction
        {
            public const string Start = "Start";
            public const string Approve = "Approve";
            public const string NeedRevision = "NeedRevision";
            public const string Reject = "Reject";
        }

        public static class CompensatoryStatus
        {
            public const string Pending = "Pending";
            public const string Available = "Available";
            public const string PartiallyUsed = "PartiallyUsed";
            public const string Used = "Used";
            public const string Expired = "Expired";
            public const string Cancelled = "Cancelled";

            public static readonly IReadOnlyCollection<string> All =
                new[] { Pending, Available, PartiallyUsed, Used, Expired, Cancelled };
        }

        public static class CompensatoryLedger
        {
            public const string LeaveCategory = "Compensatory";
            public const string Active = "Active";
            public const string Posted = "Posted";
            public const string DirectionCredit = "Credit";
            public const string DirectionDebit = "Debit";
            public const string TransactionTypeCredit = "CompensatoryCredit";
            public const string TransactionTypeReversal = "CompensatoryReversal";
            public const string TransactionTypeExpiry = "CompensatoryExpiry";
            public const string PostingBatchType = "OvertimeCompensatory";
            public const string SourceTypeCredit = "OvertimeCompensatoryTimeOff";
            public const string SourceTypeReversal = "OvertimeCompensatoryReversal";
            public const string SourceTypeExpiry = "OvertimeCompensatoryExpiry";
        }


        public static class PeriodStatus
        {
            public const string Open = "Open";
            public const string Closing = "Closing";
            public const string Closed = "Closed";
            public const string Reopened = "Reopened";
            public const string Cancelled = "Cancelled";

            public static readonly IReadOnlyCollection<string> All =
                new[] { Open, Closing, Closed, Reopened, Cancelled };
        }

        public static class SchedulerJobType
        {
            public const string AutoCalculate = "AutoCalculate";
            public const string ExpireCompensatory = "ExpireCompensatory";
            public const string Reconcile = "Reconcile";
            public const string Monitor = "Monitor";
            public const string ClosePeriod = "ClosePeriod";
            public const string FullCycle = "FullCycle";

            public static readonly IReadOnlyCollection<string> All =
                new[] { AutoCalculate, ExpireCompensatory, Reconcile, Monitor, ClosePeriod, FullCycle };
        }

        public static class SchedulerJobStatus
        {
            public const string Pending = "Pending";
            public const string Running = "Running";
            public const string RetryScheduled = "RetryScheduled";
            public const string Completed = "Completed";
            public const string CompletedWithIssues = "CompletedWithIssues";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";

            public static readonly IReadOnlyCollection<string> All =
                new[] { Pending, Running, RetryScheduled, Completed, CompletedWithIssues, Failed, Cancelled };
        }

        public static class PayrollHandoffStatus
        {
            public const string Ready = "Ready";
            public const string Posted = "Posted";
            public const string CompensatoryLeave = "CompensatoryLeave";
            public const string ReconciliationIssue = "ReconciliationIssue";

            public static readonly IReadOnlyCollection<string> All =
                new[] { Ready, Posted, CompensatoryLeave, ReconciliationIssue };
        }

        public static class PayrollHandoff
        {
            public const string SourceType = "Overtime";
            public const string CalculationOwner = "Payroll";
        }

        public static class PayrollRunStatus
        {
            public static readonly IReadOnlyCollection<string> Blocked =
                new[] { "Approved", "Closed", "Posted", "Cancelled" };
        }

        public static class PayrollPeriodStatus
        {
            public static readonly IReadOnlyCollection<string> Blocked =
                new[] { "Approved", "Closed", "Posted", "Cancelled" };
        }

        public static class PayrollEmployeeStatus
        {
            public static readonly IReadOnlyCollection<string> Blocked =
                new[] { "Finalized", "Approved", "Paid", "Closed", "Cancelled" };
        }

    }
}
