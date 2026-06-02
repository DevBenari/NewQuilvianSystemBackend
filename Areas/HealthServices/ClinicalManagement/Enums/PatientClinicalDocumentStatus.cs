namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    public enum PatientClinicalDocumentStatus
    {
        Draft = 0,
        Uploaded = 1,
        Verified = 2,
        Approved = 3,
        Rejected = 4,
        Archived = 5,
        Cancelled = 6,
        EnteredInError = 7
    }
}
