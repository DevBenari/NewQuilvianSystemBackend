namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    public enum PatientConsentStatus
    {
        Draft = 0,
        PendingSignature = 1,
        Signed = 2,
        Verified = 3,
        Approved = 4,
        Rejected = 5,
        Withdrawn = 6,
        Expired = 7,
        Cancelled = 8,
        EnteredInError = 9
    }
}
