namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums
{
    public enum JobOfferStatus
    {
        Draft = 1,
        PendingApproval = 2,
        Approved = 3,
        RejectedByApprover = 4,
        SentConditional = 5,
        ConditionalAccepted = 6,
        ConditionalRejected = 7,
        FinalOfferSent = 8,
        FinalAccepted = 9,
        FinalRejected = 10,
        Revoked = 11,
        Expired = 12
    }
}
