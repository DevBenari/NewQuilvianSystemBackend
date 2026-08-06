namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants
{
    public static class ResignationValueConstants
    {
        public static class Workflow
        {
            public const string Code = "RESIGNATION_REQUEST";
            public const string ReferenceType = "RESIGNATION_REQUEST";
        }

        public static class Status
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string UnderReview = "UnderReview";
            public const string NeedRevision = "NeedRevision";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
            public const string HandoffCompleted = "HandoffCompleted";
        }

        public static readonly string[] SortDirections = { "asc", "desc" };
        public static readonly int[] PageSizes = { 10, 25, 50, 100 };
    }
}
