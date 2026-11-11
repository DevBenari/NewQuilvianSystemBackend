namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    public enum PriorityTier
    {
        Standar = 1,
        Khusus = 2,
        Cito = 3
    }

    public enum HrServiceSlaStatus
    {
        WithinSla = 1,
        Warning75 = 2,
        Overdue = 3,
        Escalated = 4
    }

    public enum HrServiceRequestStatus
    {
        Draft = 1,
        Submitted = 2,
        Assigned = 3,
        InReview = 4,
        WaitingStaffResponse = 5,
        Approved = 6,
        Rejected = 7,
        InProgress = 8,
        Resolved = 9,
        Closed = 10,
        Cancelled = 11
    }

    public enum DocumentIssuanceStatus
    {
        Active = 1,
        Expired = 2,
        Revoked = 3
    }

    public enum DocumentApprovalStatus
    {
        Submitted = 1,
        CbApproved = 2,
        Authorized = 3,
        Rejected = 4
    }
}
