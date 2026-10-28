namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Jalur yang menutup kunjungan secara klinis — penguncian otomatis pertama (`RM-DEC-051`,
    /// `AGG-RM-02`, A4.12). Hanya penanda asal; tidak memengaruhi aturan apa pun.
    /// </summary>
    public enum ClinicalClosureSource
    {
        EncounterStatusPatch = 1,
        DoctorQueue = 2,
        NurseStation = 3,
        EmergencyVisit = 4,
        EmergencyAdmissionDisposition = 5,
        InpatientDischarge = 6,
        BillingFinalization = 7
    }
}
