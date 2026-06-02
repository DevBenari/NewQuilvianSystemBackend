namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums
{
    public enum PatientEncounterGuarantorStatus
    {
        Draft = 0,
        PendingVerification = 1,
        Eligible = 2,
        NotEligible = 3,
        NeedApproval = 4,
        NeedReferral = 5,
        NeedGuaranteeLetter = 6,
        Approved = 7,
        Rejected = 8,
        Cancelled = 9,
        Inactive = 10
    }
}
