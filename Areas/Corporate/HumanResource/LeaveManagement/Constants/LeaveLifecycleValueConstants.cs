namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants
{
    public static class LeaveLifecycleValueConstants
    {
        public static class CancellationStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string WaitingApproval = "WaitingApproval";
            public const string NeedRevision = "NeedRevision";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
            public const string Applied = "Applied";
            public const string Failed = "Failed";
        }

        public static class RecallStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string WaitingApproval = "WaitingApproval";
            public const string NeedRevision = "NeedRevision";
            public const string Acknowledged = "Acknowledged";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Applied = "Applied";
            public const string Cancelled = "Cancelled";
            public const string Failed = "Failed";
        }

        public static class ReferenceType
        {
            public const string LeaveCancellation = "LEAVE_CANCELLATION";
            public const string LeaveRecall = "LEAVE_RECALL";
            public const string LeaveReturnToWork = "LEAVE_RETURN_TO_WORK";
        }

        public static class ReconciliationSeverity
        {
            public const string Info = "Info";
            public const string Warning = "Warning";
            public const string Critical = "Critical";
        }
    }
}
