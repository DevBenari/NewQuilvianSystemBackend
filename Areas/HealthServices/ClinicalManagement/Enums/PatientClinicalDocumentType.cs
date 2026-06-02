namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    public enum PatientClinicalDocumentType
    {
        Unknown = 0,
        LaboratoryResult = 1,
        RadiologyResult = 2,
        SupportingExamResult = 3,
        MedicalResume = 4,
        ReferralLetter = 5,
        ExternalMedicalRecord = 6,
        ProcedureReport = 7,
        ConsultationReport = 8,
        DischargeSummary = 9,
        Other = 99
    }
}
