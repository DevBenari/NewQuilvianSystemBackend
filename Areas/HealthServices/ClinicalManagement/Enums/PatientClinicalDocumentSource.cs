namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    public enum PatientClinicalDocumentSource
    {
        Unknown = 0,
        InternalUpload = 1,
        ExternalHospital = 2,
        PatientProvided = 3,
        Laboratory = 4,
        Radiology = 5,
        DoctorGenerated = 6,
        NurseGenerated = 7,
        SystemGenerated = 8,
        Other = 99
    }
}
