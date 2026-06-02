namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    public enum MedicalCertificateStatus
    {
        Draft = 0,
        Issued = 1,
        Verified = 2,
        Approved = 3,
        Rejected = 4,
        Revoked = 5,
        Cancelled = 6,
        EnteredInError = 7
    }
}
