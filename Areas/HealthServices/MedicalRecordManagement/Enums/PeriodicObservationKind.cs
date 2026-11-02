namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Jenis pencatatan berkala `ClinicalManagement` yang diringkas per hari di linimasa berkas
    /// (`RM-DEC-052`, `BE-22`). Satu pasien rawat inap dapat menghasilkan ratusan pengukuran;
    /// tanpa ringkasan, dokumen penting tenggelam di antaranya.
    /// </summary>
    public enum PeriodicObservationKind
    {
        DailyObservation = 1,
        FluidBalance = 2,
        BloodGlucose = 3,
        EquipmentUsage = 4,
        WsdDrain = 5,
        SurgicalSiteSurveillance = 6
    }
}
