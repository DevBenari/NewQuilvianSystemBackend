namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models
{
    public static class WorkflowValueConstants
    {
        public static class ApproverSource
        {
            public const string DirectManager = "DirectManager";
            public const string OrganizationHead = "OrganizationHead";
            public const string DepartmentHead = "DepartmentHead";
            public const string SiteHr = "SiteHr";
            public const string CorporateHr = "CorporateHr";
            public const string PayrollOfficer = "PayrollOfficer";
            public const string FinanceOfficer = "FinanceOfficer";
            public const string CostCenterOwner = "CostCenterOwner";
            public const string CredentialingCommittee = "CredentialingCommittee";
            public const string SpecificUser = "SpecificUser";
            public const string SpecificRole = "SpecificRole";
        }

        public static class ActionType
        {
            public const string Submit = "Submit";
            public const string Approve = "Approve";
            public const string Reject = "Reject";
            public const string RequestRevision = "RequestRevision";
            public const string Return = "Return";
            public const string Cancel = "Cancel";
            public const string Withdraw = "Withdraw";
            public const string Recall = "Recall";
            public const string Verify = "Verify";
            public const string Acknowledge = "Acknowledge";
        }
    }
}
