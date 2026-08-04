namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants
{
    public static class LeaveRequestValueConstants
    {
        public static class Status
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string WaitingApproval = "WaitingApproval";
            public const string NeedRevision = "NeedRevision";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
            public const string Taken = "Taken";
            public const string Completed = "Completed";
            public const string Recalled = "Recalled";
            public const string Expired = "Expired";
        }

        public static class HalfDayPeriod
        {
            public const string FirstHalf = "FirstHalf";
            public const string SecondHalf = "SecondHalf";
        }

        public static class AttachmentType
        {
            public const string SupportingDocument = "SupportingDocument";
            public const string MedicalCertificate = "MedicalCertificate";
            public const string HandoverDocument = "HandoverDocument";
            public const string Other = "Other";
        }

        public static class AttachmentVerificationStatus
        {
            public const string Pending = "Pending";
            public const string Verified = "Verified";
            public const string Rejected = "Rejected";
            public const string ReuploadRequired = "ReuploadRequired";
        }

        public static class SourceChannel
        {
            public const string Web = "Web";
            public const string Mobile = "Mobile";
            public const string Api = "Api";
        }

        public const string WorkflowReferenceType = "LEAVE_REQUEST";
        public const string DefaultWorkflowCode = "LEAVE_REQUEST";
    }
}
