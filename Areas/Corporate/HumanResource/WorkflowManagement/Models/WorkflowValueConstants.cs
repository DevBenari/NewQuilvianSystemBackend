namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    public static class WorkflowValueConstants
    {
        public static class WorkflowStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string InProgress = "InProgress";
            public const string RevisionRequested = "RevisionRequested";
            public const string Returned = "Returned";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Cancelled = "Cancelled";
            public const string Withdrawn = "Withdrawn";
            public const string Completed = "Completed";
        }

        public static class StepStatus
        {
            public const string Pending = "Pending";
            public const string Available = "Available";
            public const string InProgress = "InProgress";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string RevisionRequested = "RevisionRequested";
            public const string Returned = "Returned";
            public const string Skipped = "Skipped";
            public const string Cancelled = "Cancelled";
            public const string Completed = "Completed";
        }

        public static class AssignmentStatus
        {
            public const string Pending = "Pending";
            public const string Available = "Available";
            public const string InProgress = "InProgress";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string RevisionRequested = "RevisionRequested";
            public const string Returned = "Returned";
            public const string Delegated = "Delegated";
            public const string Skipped = "Skipped";
            public const string Cancelled = "Cancelled";
            public const string Completed = "Completed";
        }

        public static class StepType
        {
            public const string Approval = "Approval";
            public const string Review = "Review";
            public const string Verification = "Verification";
            public const string Notification = "Notification";
            public const string Acknowledgement = "Acknowledgement";
            public const string SystemAction = "SystemAction";
        }

        public static class ApprovalMode
        {
            public const string Any = "Any";
            public const string All = "All";
            public const string Sequential = "Sequential";
            public const string Percentage = "Percentage";
        }

        public static class ApproverSource
        {
            public const string RequesterManager = "RequesterManager";
            public const string ManagerLevel = "ManagerLevel";
            public const string Position = "Position";
            public const string OrganizationUnit = "OrganizationUnit";
            public const string Role = "Role";
            public const string SpecificUser = "SpecificUser";
            public const string ApprovalMatrix = "ApprovalMatrix";
            public const string RequesterSelected = "RequesterSelected";
            public const string OrganizationHead = "OrganizationHead";
            public const string DepartmentHead = "DepartmentHead";
            public const string SiteHr = "SiteHr";
            public const string CorporateHr = "CorporateHr";
            public const string PayrollOfficer = "PayrollOfficer";
            public const string FinanceOfficer = "FinanceOfficer";
            public const string CostCenterOwner = "CostCenterOwner";
            public const string CredentialingCommittee = "CredentialingCommittee";
            public const string SpecificRole = "SpecificRole";

            // Legacy value. Resolver may accept this value for existing data,
            // but new records should use RequesterManager.
            public const string DirectManager = "DirectManager";
        }

        public static class ActionType
        {
            public const string Submit = "Submit";
            public const string Start = "Start";
            public const string Approve = "Approve";
            public const string Reject = "Reject";
            public const string RequestRevision = "RequestRevision";
            public const string Return = "Return";
            public const string Cancel = "Cancel";
            public const string Withdraw = "Withdraw";
            public const string Recall = "Recall";
            public const string Verify = "Verify";
            public const string Acknowledge = "Acknowledge";
            public const string Delegate = "Delegate";
            public const string Reassign = "Reassign";
            public const string Skip = "Skip";
            public const string MoveNext = "MoveNext";
            public const string Complete = "Complete";
            public const string AutoApprove = "AutoApprove";
            public const string AutoReject = "AutoReject";
            public const string RevokeDelegation = "RevokeDelegation";
        }

        public static class DelegationStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string Approved = "Approved";
            public const string Active = "Active";
            public const string Expired = "Expired";
            public const string Rejected = "Rejected";
            public const string Revoked = "Revoked";
            public const string Cancelled = "Cancelled";
        }

        public static class CommentType
        {
            public const string General = "General";
            public const string Requester = "Requester";
            public const string Approver = "Approver";
            public const string Internal = "Internal";
            public const string Revision = "Revision";
            public const string Rejection = "Rejection";
            public const string System = "System";
        }

        public static class RejectAction
        {
            public const string ReturnToRequester = "ReturnToRequester";
            public const string ReturnToPreviousStep = "ReturnToPreviousStep";
            public const string ReturnToSpecificStep = "ReturnToSpecificStep";
            public const string CancelRequest = "CancelRequest";
            public const string CloseRequest = "CloseRequest";
        }

        public static class SourceChannel
        {
            public const string Web = "Web";
            public const string Mobile = "Mobile";
            public const string Api = "Api";
            public const string System = "System";
            public const string Scheduler = "Scheduler";
            public const string Integration = "Integration";
        }
    }
}
