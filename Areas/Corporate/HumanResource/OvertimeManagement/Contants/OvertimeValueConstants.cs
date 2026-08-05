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
    }
}
