namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants
{
    public static class SchedulingRequestValueConstants
    {
        public static class Workflow
        {
            public const string ScheduleChangeCode = "SCHEDULE_CHANGE_REQUEST";
            public const string ScheduleChangeReferenceType = "SCHEDULE_CHANGE_REQUEST";
            public const string ShiftSwapCode = "SHIFT_SWAP_REQUEST";
            public const string ShiftSwapReferenceType = "SHIFT_SWAP_REQUEST";
        }

        public static class ScheduleChangeStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string UnderReview = "UnderReview";
            public const string NeedRevision = "NeedRevision";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
            public const string Applied = "Applied";
        }

        public static class ScheduleChangeType
        {
            public const string ScheduleChange = "ScheduleChange";
            public const string ShiftChange = "ShiftChange";
            public const string DayOffChange = "DayOffChange";
            public const string TemporarySchedule = "TemporarySchedule";

            public static readonly string[] All =
            {
                ScheduleChange,
                ShiftChange,
                DayOffChange,
                TemporarySchedule
            };
        }

        public static class ShiftSwapStatus
        {
            public const string Draft = "Draft";
            public const string PendingTarget = "PendingTarget";
            public const string TargetAccepted = "TargetAccepted";
            public const string TargetRejected = "TargetRejected";
            public const string PendingApproval = "PendingApproval";
            public const string NeedRevision = "NeedRevision";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
            public const string Applied = "Applied";
        }

        public static readonly string[] SortDirections = { "asc", "desc" };
        public static readonly int[] PageSizes = { 10, 25, 50, 100 };
    }
}
