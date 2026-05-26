namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums
{
    public enum ExternalUserStatus
    {
        Unknown = 0,
        Active = 1,
        Inactive = 2,
        PendingApproval = 3,
        Suspended = 4,
        Blacklisted = 5,
        ContractEnded = 6,
        AccessExpired = 7
    }
}
