namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    public enum PatientConsentMethod
    {
        Unknown = 0,
        WrittenPaper = 1,
        DigitalSignature = 2,
        ElectronicConsent = 3,
        VerbalConsent = 4,
        ScannedDocument = 5,
        Other = 99
    }
}
